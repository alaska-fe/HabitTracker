using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;
using HabitTracker.Messaging;
using HabitTracker.Services;
using HabitTracker.Texts;

namespace HabitTracker.ViewModels;

public sealed class HabitsViewModel : BaseViewModel
{
    private readonly IHabitService _service;
    private readonly IClock _clock;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private bool _isLoading;
    private string _summaryText = string.Empty;

    public HabitsViewModel(
        IHabitService service,
        IClock clock,
        IDialogService dialogs,
        INavigationService navigation) : base(dialogs)
    {
        _service = service;
        _clock = clock;
        _dialogs = dialogs;
        _navigation = navigation;

        Habits.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(ShowEmptyState));
        };

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(() => _navigation.GoToAddHabitAsync());
        OpenDetailsCommand = new AsyncRelayCommand<HabitCardModel>(OpenDetailsAsync);

        MarkCommand = new AsyncRelayCommand<HabitCardModel>(
            MarkCardAsync, CanMarkCard, AsyncRelayCommandOptions.AllowConcurrentExecutions);
        DeleteCommand = new AsyncRelayCommand<HabitCardModel>(
            DeleteCardAsync, CanDeleteCard, AsyncRelayCommandOptions.AllowConcurrentExecutions);

        WeakReferenceMessenger.Default.Register<HabitAddedMessage>(this, (_, message) =>
            ShowStatus(Messages.HabitAdded(message.Name)));
        WeakReferenceMessenger.Default.Register<HabitDeletedMessage>(this, (_, message) =>
            ShowStatus(Messages.HabitDeleted(message.Name)));
        WeakReferenceMessenger.Default.Register<HabitNotFoundMessage>(this, (_, _) =>
            ShowStatus(Messages.NotFound));
    }

    public ObservableCollection<HabitCardModel> Habits { get; } = new();

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand<HabitCardModel> OpenDetailsCommand { get; }
    public IAsyncRelayCommand<HabitCardModel> MarkCommand { get; }
    public IAsyncRelayCommand<HabitCardModel> DeleteCommand { get; }

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

    public string SummaryText { get => _summaryText; private set => SetProperty(ref _summaryText, value); }

    public bool HasItems => Habits.Count > 0;
    public bool ShowEmptyState => !IsLoading && Habits.Count == 0;

    private static bool CanMarkCard(HabitCardModel? card) => card is { IsBusy: false, IsDoneToday: false };
    private static bool CanDeleteCard(HabitCardModel? card) => card is { IsBusy: false };

    private Task OpenDetailsAsync(HabitCardModel? card) =>
        card is null ? Task.CompletedTask : _navigation.GoToHabitDetailsAsync(card.Id);

    private Task LoadAsync() =>
        RunSafeAsync(LoadCoreAsync, busy => IsLoading = busy, OnLoadErrorAsync);

    private async Task LoadCoreAsync()
    {
        ClearError();
        var habits = await _service.GetAllAsync();
        var sorted = HabitSorter.SortForToday(habits, _clock.Today);

        foreach (var old in Habits)
        {
            old.PropertyChanged -= OnCardPropertyChanged;
        }
        Habits.Clear();

        foreach (var habit in sorted)
        {
            var card = new HabitCardModel(habit, _clock.Today);
            card.PropertyChanged += OnCardPropertyChanged;
            Habits.Add(card);
        }

        UpdateSummary();
    }

    private Task OnLoadErrorAsync(HabitException ex)
    {
        ShowError(Messages.LoadFailed);
        return Task.CompletedTask;
    }

    private async Task MarkCardAsync(HabitCardModel? card)
    {
        if (card is null || card.IsBusy || card.IsDoneToday)
        {
            return;
        }

        await RunSafeAsync(() => MarkCoreAsync(card), busy => card.IsBusy = busy, OnMarkErrorAsync);
    }

    private async Task MarkCoreAsync(HabitCardModel card)
    {
        var result = await _service.MarkCompletedTodayAsync(card.Id);

        card.Update(result.Habit, _clock.Today);
        Resort();
        UpdateSummary();
        ClearError();

        switch (result.Outcome)
        {
            case MarkOutcome.AlreadyDoneToday:
                ShowStatus(Messages.AlreadyDoneToday);
                break;
            case MarkOutcome.GoalReached:
                ShowStatus(Messages.HabitMarked(result.Habit.CurrentStreak));
                await _dialogs.AlertAsync(Messages.GoalReachedTitle,
                    Messages.GoalReached(result.Habit.Name, result.Habit.GoalDays));
                break;
            default:
                ShowStatus(Messages.HabitMarked(result.Habit.CurrentStreak));
                break;
        }
    }

    private async Task OnMarkErrorAsync(HabitException ex)
    {
        switch (ex.Kind)
        {
            case HabitErrorKind.NotFound:
                ShowStatus(Messages.NotFound);
                await LoadCoreAsync();
                break;
            case HabitErrorKind.ClockMovedBack:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.ClockMovedBack);
                break;
            default:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
                break;
        }
    }

    private async Task DeleteCardAsync(HabitCardModel? card)
    {
        if (card is null || card.IsBusy)
        {
            return;
        }

        var confirmed = await _dialogs.ConfirmAsync(
            Messages.ConfirmDeleteTitle, Messages.ConfirmDelete(card.Name),
            Messages.DeleteAccept, Messages.DeleteCancel);
        if (!confirmed)
        {
            return;
        }

        await RunSafeAsync(() => DeleteCoreAsync(card), busy => card.IsBusy = busy, OnDeleteErrorAsync);
    }

    private async Task DeleteCoreAsync(HabitCardModel card)
    {
        await _service.DeleteAsync(card.Id);
        card.PropertyChanged -= OnCardPropertyChanged;
        Habits.Remove(card);
        UpdateSummary();
        ClearError();
        ShowStatus(Messages.HabitDeleted(card.Name));
    }

    private async Task OnDeleteErrorAsync(HabitException ex)
    {
        if (ex.Kind == HabitErrorKind.NotFound)
        {
            ShowStatus(Messages.NotFound);
            await LoadCoreAsync();
        }
        else
        {
            await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
        }
    }

    private void Resort()
    {
        var sorted = HabitSorter.SortForToday(Habits.Select(card => card.Source).ToList(), _clock.Today);
        for (var target = 0; target < sorted.Count; target++)
        {
            var current = Habits.IndexOf(Habits.First(card => card.Id == sorted[target].Id));
            if (current != target)
            {
                Habits.Move(current, target);
            }
        }
    }

    private void UpdateSummary()
    {
        SummaryText = Messages.TodaySummary(Habits.Count(card => card.IsDoneToday), Habits.Count);
    }

    private void OnCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(HabitCardModel.IsBusy) or nameof(HabitCardModel.IsDoneToday) or null or "")
        {
            MarkCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }
    }
}