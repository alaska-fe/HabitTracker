namespace HabitTracker.Services;

/// <summary>Диалоги платформы, вызываемые из ViewModel (ТЗ 4.3). ViewModel не знает о DisplayAlert.</summary>
public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
    Task AlertAsync(string title, string message, string close = "OK");
}