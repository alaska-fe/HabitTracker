using HabitTracker.Core.Models;

namespace HabitTracker.Core.Rules;

/// <summary>
/// Сортировка списка для экрана «Привычки» (ТЗ 7.3) — чистая функция, исходный список не изменяется.
/// Порядок: сначала невыполненные сегодня; внутри — по убыванию эффективной серии,
/// затем по названию без учёта регистра; выполненные сегодня — в конце.
/// </summary>
public static class HabitSorter
{
    public static IReadOnlyList<Habit> SortForToday(IReadOnlyList<Habit> habits, DateTime today) =>
        habits
            .OrderBy(habit => HabitRules.IsDoneToday(habit, today)) // false < true: невыполненные раньше
            .ThenByDescending(habit => HabitRules.GetEffectiveStreak(habit, today))
            .ThenBy(habit => habit.Name, StringComparer.OrdinalIgnoreCase) // детерминированно на любой культуре
            .ToList();
}