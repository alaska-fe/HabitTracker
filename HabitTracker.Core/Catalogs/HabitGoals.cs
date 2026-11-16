namespace HabitTracker.Core.Catalogs;

/// <summary>
/// Допустимые значения цели (ТЗ 6.4) — единый источник для валидации и Picker на экране добавления.
/// Имя по аналогии с HabitIcons/HabitColors.
/// </summary>
public static class HabitGoals
{
    public const int Default = 21;

    public static readonly IReadOnlyList<int> All = new[] { 7, 14, 21, 30, 66, 100 };
}