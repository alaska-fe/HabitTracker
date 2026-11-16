using System.Globalization;
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

public sealed class HabitDetailsViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IHabitService _service;
    private readonly IClock _clock;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private HabitCardModel? _card;
    private string _descriptionText = Messages.NoDescription;
    private string _currentStreakText = string.Empty;
    private string _bestStreakText = string.Empty;
    private string _createdAtText = string.Empty;
    private string _progressText = string.Empty;

    public HabitDetailsViewModel(
        IHabitService service,
        IClock clock,
        IDialogService dialogs,
        INavigationService navigation) : base(dialogs)
    {
        _service = service;
        _clock = clock;
        _dialogs = dialogs;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        MarkCommand = new AsyncRelayCommand(MarkAsync, CanMark);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => !IsBusy);
    }

    public int HabitId { get; private set; }

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand MarkCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public HabitCardModel? Card
    {
        get => _card;
        private set
        {
            if (SetProperty(ref _card, value))
            {
                OnPropertyChanged(nameof(HasCard));
                MarkCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasCard => Card is not null;

    public string DescriptionText { get => _descriptionText; private set => SetProperty(ref _descriptionText, value); }
    public string CurrentStreakText { get => _currentStreakText; private set => SetProperty(ref _currentStreakText, value); }
    public string BestStreakText { get => _bestStreakText; private set => SetProperty(ref _bestStreakText, value); }
    public string CreatedAtText { get => _createdAtText; private set => SetProperty(ref _createdAtText, value); }
    public string ProgressText { get => _progressText; private set => SetProperty(ref _progressText, value); }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) &&
            int.TryParse(value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            HabitId = id;
        }
    }

    private bool CanMark() => Card is { IsBusy: false, IsDoneToday: false };

    private Task LoadAsync() => RunSafeAsync(LoadCoreAsync, busy => IsBusy = busy, OnLoadErrorAsync);

    private async Task LoadCoreAsync()
    {
        ClearError();
        var habits = await _service.GetAllAsync();
        var habit = habits.FirstOrDefault(item => item.Id == HabitId);

        if (habit is null)
        {
            await NotifyNotFoundAndLeaveAsync();
            return;
        }

        Apply(habit);
    }

    private void Apply(Habit habit)
    {
        Card = new HabitCardModel(habit, _clock.Today);
        DescriptionText = string.IsNullOrWhiteSpace(habit.Description) ? Messages.NoDescription : habit.Description;
        CurrentStreakText = Messages.ShortDays(Card.EffectiveStreak);
        BestStreakText = Messages.ShortDays(habit.BestStreak);
        CreatedAtText = Messages.CreatedAtText(habit.CreatedAt);
        ProgressText = Messages.ProgressDetails(
            HabitRules.GetProgressPercent(Card.Progress), Card.ProgressText);
    }

    private async Task OnLoadErrorAsync(HabitException ex)
    {
        if (ex.Kind == HabitErrorKind.NotFound)
        {
            await NotifyNotFoundAndLeaveAsync();
            return;
        }

        ShowError(Messages.LoadFailed);
    }

    private Task MarkAsync() => RunSafeAsync(MarkCoreAsync, busy => IsBusy = busy, OnMarkErrorAsync);

    private async Task MarkCoreAsync()
    {
        if (Card is null || Card.IsDoneToday)
        {
            return;
        }

        var result = await _service.MarkCompletedTodayAsync(HabitId);
        Apply(result.Habit);
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
                await NotifyNotFoundAndLeaveAsync();
                break;
            case HabitErrorKind.ClockMovedBack:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.ClockMovedBack);
                break;
            default:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
                break;
        }
    }

    private async Task DeleteAsync()
    {
        if (Card is null || IsBusy)
        {
            return;
        }

        var confirmed = await _dialogs.ConfirmAsync(
            Messages.ConfirmDeleteTitle, Messages.ConfirmDelete(Card.Name),
            Messages.DeleteAccept, Messages.DeleteCancel);
        if (!confirmed)
        {
            return;
        }

        await RunSafeAsync(DeleteCoreAsync, busy => IsBusy = busy, OnDeleteErrorAsync);
    }

    private async Task DeleteCoreAsync()
    {
        var name = Card?.Name ?? string.Empty;
        await _service.DeleteAsync(HabitId);
        WeakReferenceMessenger.Default.Send(new HabitDeletedMessage(name));
        await _navigation.GoBackAsync();
    }

    private async Task OnDeleteErrorAsync(HabitException ex)
    {
        if (ex.Kind == HabitErrorKind.NotFound)
        {
            await NotifyNotFoundAndLeaveAsync();
            return;
        }

        await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
    }

    private async Task NotifyNotFoundAndLeaveAsync()
    {
        WeakReferenceMessenger.Default.Send(new HabitNotFoundMessage());
        await _navigation.GoBackAsync();
    }
}