namespace HabitTracker.Services;

/// <summary>
/// Навигация через Shell (ТЗ 8). Флаг защищает от двойного перехода (ТЗ 8.3).
/// Маршруты регистрируются в AppShell на шагах 6 и 10.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private bool _isNavigating;

    public Task GoToAddHabitAsync() => NavigateOnceAsync("add-habit");

    public Task GoBackAsync() => NavigateOnceAsync("..");

    public Task GoToHabitsAsync() => NavigateOnceAsync("//habits");

    public Task GoToHabitDetailsAsync(int habitId) => NavigateOnceAsync($"habit-details?id={habitId}");

    private async Task NavigateOnceAsync(string route)
    {
        if (_isNavigating)
        {
            return;
        }

        _isNavigating = true;
        try
        {
            var shell = Shell.Current ?? throw new InvalidOperationException("Shell ещё не создан.");
            await shell.GoToAsync(route);
        }
        finally
        {
            _isNavigating = false;
        }
    }
}