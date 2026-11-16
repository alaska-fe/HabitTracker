using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Services;
using HabitTracker.Services;
using HabitTracker.Texts;

namespace HabitTracker.ViewModels;

/// <summary>
/// Базовая ViewModel (ТЗ 4.2, 10.1): занятость, баннер ошибки, строка состояния,
/// единая «обёртка безопасности» для команд. Не знает о SQLite и Shell.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private const int StatusHideDelayMs = 3000;

    private readonly IDialogService _dialogs;
    private CancellationTokenSource? _statusCts;

    private bool _isBusy;
    private string? _errorMessage;
    private string? _statusMessage;

    protected BaseViewModel(IDialogService dialogs)
    {
        _dialogs = dialogs;
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatus));
            }
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    /// <summary>
    /// Единая обёртка команды (ТЗ 10.1): журналирует ошибки, ожидаемые HabitException
    /// превращает в уведомления, страховочный catch не даёт приложению упасть,
    /// занятость сбрасывается в finally. setBusy — флаг формы ИЛИ конкретной карточки;
    /// onHabitError — экран-специфичная реакция (баннер / inline / диалог).
    /// </summary>
    protected async Task RunSafeAsync(
        Func<Task> action,
        Action<bool>? setBusy = null,
        Func<HabitException, Task>? onHabitError = null)
    {
        setBusy?.Invoke(true);
        try
        {
            await action();
        }
        catch (HabitException ex)
        {
            Debug.WriteLine($"[HabitTracker] Ожидаемая ошибка {ex.Kind}: {ex.Message}");
            if (onHabitError is not null)
            {
                await onHabitError(ex);
            }
            else
            {
                await HandleHabitErrorAsync(ex);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HabitTracker] Непредвиденная ошибка: {ex}");
            await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
        }
        finally
        {
            setBusy?.Invoke(false);
        }
    }

    protected void ShowError(string message) => ErrorMessage = message;

    protected void ClearError() => ErrorMessage = null;

    /// <summary>Строка состояния: новое сообщение заменяет прежнее, таймер ~3 с сбрасывается (ТЗ 9.4).</summary>
    protected void ShowStatus(string message)
    {
        _statusCts?.Cancel();
        _statusCts?.Dispose();
        _statusCts = new CancellationTokenSource();
        StatusMessage = message;
        _ = HideStatusAsync(_statusCts.Token);
    }

    private async Task HideStatusAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(StatusHideDelayMs, token);
            StatusMessage = null;
        }
        catch (OperationCanceledException)
        {
            // Сообщение заменено новым — штатная ситуация, не ошибка.
        }
    }

    private async Task HandleHabitErrorAsync(HabitException ex)
    {
        switch (ex.Kind)
        {
            case HabitErrorKind.Storage:
                ShowError(Messages.SaveFailed);
                break;
            case HabitErrorKind.ClockMovedBack:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.ClockMovedBack);
                break;
            case HabitErrorKind.LimitReached:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.LimitReached(HabitService.MaxHabits));
                break;
            case HabitErrorKind.NotFound:
                ShowStatus(Messages.NotFound);
                break;
            default:
                await _dialogs.AlertAsync(Messages.ErrorTitle, Messages.SaveFailed);
                break;
        }
    }
}