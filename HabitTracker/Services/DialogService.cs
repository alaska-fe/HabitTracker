namespace HabitTracker.Services;

/// <summary>
/// Диалоги через DisplayAlertAsync текущей страницы (.NET 10).
/// SemaphoreSlim гарантирует «не более одного диалога одновременно» (ТЗ 9.4).
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly SemaphoreSlim _dialogGate = new(1, 1);

    public async Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        await _dialogGate.WaitAsync();
        try
        {
            return await GetPage().DisplayAlertAsync(title, message, accept, cancel);
        }
        finally
        {
            _dialogGate.Release();
        }
    }

    public async Task AlertAsync(string title, string message, string close = "OK")
    {
        await _dialogGate.WaitAsync();
        try
        {
            await GetPage().DisplayAlertAsync(title, message, close);
        }
        finally
        {
            _dialogGate.Release();
        }
    }

    private static Page GetPage() =>
        Shell.Current?.CurrentPage
        ?? Application.Current?.Windows.FirstOrDefault()?.Page
        ?? throw new InvalidOperationException("Нет активной страницы для показа диалога.");
}