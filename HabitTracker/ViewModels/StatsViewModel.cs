using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Errors;
using HabitTracker.Services;
using HabitTracker.Texts;

namespace HabitTracker.ViewModels;

public sealed class StatsViewModel : BaseViewModel
{
    private readonly IHabitService _service;
    private readonly INavigationService _navigation;

    private bool _isLoading;
    private string _totalText = "0";
    private string _todayText = Messages.TodayTile(0, 0);
    private string _goalsText = "0";
    private string _averageProgressText = Messages.AverageProgressLine(0);
    private string _longestCurrentText = Messages.LongestCurrentLine(0);
    private string _longestEverText = Messages.LongestEverLine(0);

    public StatsViewModel(IHabitService service, IDialogService dialogs, INavigationService navigation)
        : base(dialogs)
    {
        _service = service;
        _navigation = navigation;

        Rows.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(ShowEmptyState));
        };

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(() => RunSafeAsync(AddCoreAsync));
    }

    public ObservableCollection<StatsRowModel> Rows { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand AddCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(ShowEmptyState));
            }
        }
    }

    public string TotalText { get => _totalText; private set => SetProperty(ref _totalText, value); }
    public string TodayText { get => _todayText; private set => SetProperty(ref _todayText, value); }
    public string GoalsText { get => _goalsText; private set => SetProperty(ref _goalsText, value); }

    public string AverageProgressText
    {
        get => _averageProgressText;
        private set => SetProperty(ref _averageProgressText, value);
    }

    public string LongestCurrentText
    {
        get => _longestCurrentText;
        private set => SetProperty(ref _longestCurrentText, value);
    }

    public string LongestEverText
    {
        get => _longestEverText;
        private set => SetProperty(ref _longestEverText, value);
    }

    public bool HasItems => Rows.Count > 0;
    public bool ShowEmptyState => !IsLoading && Rows.Count == 0;

    private Task LoadAsync() => RunSafeAsync(LoadCoreAsync, busy => IsLoading = busy, OnLoadErrorAsync);

    private async Task LoadCoreAsync()
    {
        ClearError();
        var statistics = await _service.GetStatisticsAsync();

        TotalText = statistics.TotalHabits.ToString(CultureInfo.InvariantCulture);
        TodayText = Messages.TodayTile(statistics.DoneToday, statistics.TotalHabits);
        GoalsText = statistics.GoalsReached.ToString(CultureInfo.InvariantCulture);
        AverageProgressText = Messages.AverageProgressLine(statistics.AverageProgressPercent);
        LongestCurrentText = Messages.LongestCurrentLine(statistics.LongestCurrentStreak);
        LongestEverText = Messages.LongestEverLine(statistics.LongestEverStreak);

        Rows.Clear();
        foreach (var row in statistics.Rows)
        {
            Rows.Add(new StatsRowModel(row));
        }
    }

    private Task OnLoadErrorAsync(HabitException ex)
    {
        ShowError(Messages.LoadFailed);
        return Task.CompletedTask;
    }

    private async Task AddCoreAsync()
    {
        await _navigation.GoToHabitsAsync();
        await _navigation.GoToAddHabitAsync();
    }
}