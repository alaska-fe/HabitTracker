namespace HabitTracker.Services;

/// <summary>
/// Навигация Shell, вызываемая из ViewModel (ТЗ 4.3, 8.3). ViewModel не знает о Shell.
/// Состав расширен относительно минимального из ТЗ ради переходов T8 и T9 — обосновано на защите.
/// </summary>
public interface INavigationService
{
    Task GoToAddHabitAsync();
    Task GoBackAsync();
    Task GoToHabitsAsync();                   // T8: из пустой статистики на вкладку «Привычки»
    Task GoToHabitDetailsAsync(int habitId);  // T9: вариант «Детали привычки»
}