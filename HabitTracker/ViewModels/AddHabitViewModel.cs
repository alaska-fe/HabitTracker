using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;
using HabitTracker.Core.Texts;
using HabitTracker.Core.Validation;
using HabitTracker.Messaging;
using HabitTracker.Services;
using HabitTracker.Texts;

namespace HabitTracker.ViewModels;

public sealed class AddHabitViewModel : BaseViewModel
{
    private readonly IHabitService _service;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private string _name = string.Empty;
    private string _description = string.Empty;
    private PickerOption _selectedIcon;
    private PickerOption _selectedColor;
    private PickerOption _selectedGoal;
    private string? _nameError;
    private string? _descriptionError;
    private bool _nameTouched;

    public AddHabitViewModel(IHabitService service, IDialogService dialogs, INavigationService navigation)
        : base(dialogs)
    {
        _service = service;
        _dialogs = dialogs;
        _navigation = navigation;

        IconOptions = HabitIcons.All
            .Select(option => new PickerOption(option.Key, $"{option.Glyph} {option.Title}"))
            .ToList();
        ColorOptions = HabitColors.All
            .Select(option => new PickerOption(option.Key, option.Title))
            .ToList();
        GoalOptions = HabitGoals.All
            .Select(goal => new PickerOption(
                goal.ToString(CultureInfo.InvariantCulture),
                $"{goal} {HabitTexts.DaysText(goal)}"))
            .ToList();

        _selectedIcon = IconOptions.First(option => option.Key == HabitIcons.Default);
        _selectedColor = ColorOptions.First(option => option.Key == HabitColors.Default);
        _selectedGoal = GoalOptions.First(option =>
            option.Key == HabitGoals.Default.ToString(CultureInfo.InvariantCulture));

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
    }

    public IReadOnlyList<PickerOption> IconOptions { get; }
    public IReadOnlyList<PickerOption> ColorOptions { get; }
    public IReadOnlyList<PickerOption> GoalOptions { get; }

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }

    public bool CanLeaveWithoutConfirm { get; private set; }

    public string Name
    {
        get => _name;
        set
        {
            if (!SetProperty(ref _name, value ?? string.Empty))
            {
                return;
            }

            OnPropertyChanged(nameof(NameCounter));
            OnPropertyChanged(nameof(PreviewName));

            if (_nameTouched)
            {
                ValidateName();
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (!SetProperty(ref _description, value ?? string.Empty))
            {
                return;
            }

            OnPropertyChanged(nameof(DescriptionCounter));
            DescriptionError = HabitValidator.GetDescriptionError(_description);
        }
    }

    public PickerOption SelectedIcon
    {
        get => _selectedIcon;
        set
        {
            if (value is null || !SetProperty(ref _selectedIcon, value))
            {
                return;
            }

            OnPropertyChanged(nameof(PreviewGlyph));
        }
    }

    public PickerOption SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (value is null || !SetProperty(ref _selectedColor, value))
            {
                return;
            }

            OnPropertyChanged(nameof(PreviewColorHex));
        }
    }

    public PickerOption SelectedGoal
    {
        get => _selectedGoal;
        set
        {
            if (value is null)
            {
                return;
            }

            SetProperty(ref _selectedGoal, value);
        }
    }

    public string? NameError
    {
        get => _nameError;
        private set
        {
            if (SetProperty(ref _nameError, value))
            {
                OnPropertyChanged(nameof(HasNameError));
            }
        }
    }

    public bool HasNameError => !string.IsNullOrEmpty(NameError);

    public string? DescriptionError
    {
        get => _descriptionError;
        private set
        {
            if (SetProperty(ref _descriptionError, value))
            {
                OnPropertyChanged(nameof(HasDescriptionError));
            }
        }
    }

    public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);

    public string NameCounter => Messages.CharCounter(Name.Length, HabitValidator.NameMaxLength);
    public string DescriptionCounter => Messages.CharCounter(Description.Length, HabitValidator.DescriptionMaxLength);

    public string PreviewGlyph => HabitIcons.GetGlyph(SelectedIcon.Key);
    public string PreviewColorHex => HabitColors.GetHex(SelectedColor.Key);
    public string PreviewName => Name;
    public string PreviewStreakText => HabitTexts.StreakText(0);

    public bool HasUnsavedInput =>
        !string.IsNullOrWhiteSpace(Name) ||
        !string.IsNullOrWhiteSpace(Description) ||
        SelectedIcon.Key != HabitIcons.Default ||
        SelectedColor.Key != HabitColors.Default ||
        SelectedGoal.Key != HabitGoals.Default.ToString(CultureInfo.InvariantCulture);

    public void OnNameLostFocus()
    {
        _nameTouched = true;
        ValidateName();
    }

    private void ValidateName() => NameError = HabitValidator.GetNameError(Name);

    private Task SaveAsync() => RunSafeAsync(SaveCoreAsync, busy => IsBusy = busy, OnSaveErrorAsync);

    private async Task SaveCoreAsync()
    {
        _nameTouched = true;
        ValidateName();
        DescriptionError = HabitValidator.GetDescriptionError(Description);

        if (HasNameError || HasDescriptionError)
        {
            return;
        }

        var request = new NewHabit(
            Name,
            Description,
            SelectedIcon.Key,
            SelectedColor.Key,
            int.Parse(SelectedGoal.Key, CultureInfo.InvariantCulture));

        var habit = await _service.AddAsync(request);

        CanLeaveWithoutConfirm = true;
        WeakReferenceMessenger.Default.Send(new HabitAddedMessage(habit.Name));
        await _navigation.GoBackAsync();
    }

    private async Task OnSaveErrorAsync(HabitException ex)
    {
        switch (ex.Kind)
        {
            case HabitErrorKind.DuplicateName:
                NameError = Messages.DuplicateName;
                break;
            case HabitErrorKind.ValidationFailed:
                if (!ApplyFieldErrors(ex.FieldErrors))
                {
                    await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
                }

                break;
            case HabitErrorKind.LimitReached:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.LimitReached(HabitService.MaxHabits));
                break;
            default:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
                break;
        }
    }

    private bool ApplyFieldErrors(IReadOnlyDictionary<string, string> errors)
    {
        var applied = false;

        if (errors.TryGetValue(nameof(NewHabit.Name), out var nameError))
        {
            NameError = nameError;
            applied = true;
        }

        if (errors.TryGetValue(nameof(NewHabit.Description), out var descriptionError))
        {
            DescriptionError = descriptionError;
            applied = true;
        }

        return applied;
    }

    private Task CancelAsync() => RunSafeAsync(CancelCoreAsync);

    private async Task CancelCoreAsync()
    {
        if (HasUnsavedInput)
        {
            var confirmed = await _dialogs.ConfirmAsync(
                Messages.ConfirmDiscardTitle,
                Messages.ConfirmDiscard,
                Messages.DiscardAccept,
                Messages.DiscardCancel);

            if (!confirmed)
            {
                return;
            }
        }

        CanLeaveWithoutConfirm = true;
        await _navigation.GoBackAsync();
    }
}