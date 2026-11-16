using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;

namespace HabitTracker.Core.Statistics;

/// <summary>
/// Расчёт сводной статистики (ТЗ 5.6) — чистая статическая логика:
/// принимает список привычек и текущую дату, ничего не знает о БД и UI.
/// </summary>
public static class HabitStatisticsCalculator
{
    public static HabitStatistics Calculate(IReadOnlyList<Habit> habits, DateTime today)
    {
        var rows = habits
            .Select(habit => new HabitProgressRow(
                habit.Id,
                habit.Name,
                HabitIcons.GetGlyph(habit.IconKey),
                HabitColors.GetHex(habit.ColorKey),
                HabitRules.GetEffectiveStreak(habit, today),
                habit.GoalDays,
                HabitRules.GetProgress(habit, today),
                HabitRules.IsDoneToday(habit, today),
                HabitRules.IsGoalReached(habit, today)))
            .OrderByDescending(row => row.Progress) // ТЗ 7.4: по убыванию прогресса,
            .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase) // затем по названию
            .ToList();

        if (habits.Count == 0)
        {
            return new HabitStatistics(0, 0, 0, 0.0, 0, 0, rows);
        }

        var averagePercent = Math.Round(
            rows.Average(row => row.Progress * 100),
            1,
            MidpointRounding.AwayFromZero);

        return new HabitStatistics(
            TotalHabits: habits.Count,
            DoneToday: rows.Count(row => row.IsDoneToday),
            GoalsReached: rows.Count(row => row.IsGoalReached),
            AverageProgressPercent: averagePercent,
            LongestCurrentStreak: rows.Max(row => row.EffectiveStreak),
            LongestEverStreak: habits.Max(habit => habit.BestStreak),
            Rows: rows);
    }
}