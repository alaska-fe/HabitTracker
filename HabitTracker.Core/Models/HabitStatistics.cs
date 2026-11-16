using System.Diagnostics.CodeAnalysis;

namespace HabitTracker.Core.Models;

/// <summary>Сводная статистика (ТЗ 5.6). Расчёт — HabitStatisticsCalculator.</summary>
/// <remarks>DTO без логики: сгенерированные record-члены исключены из замера покрытия.</remarks>
[ExcludeFromCodeCoverage]
public sealed record HabitStatistics(
    int TotalHabits,
    int DoneToday,
    int GoalsReached,
    double AverageProgressPercent,
    int LongestCurrentStreak,
    int LongestEverStreak,
    IReadOnlyList<HabitProgressRow> Rows);

/// <summary>Строка списка на экране «Статистика» — вычисленное представление на сегодняшнюю дату.</summary>
/// <remarks>DTO без логики: сгенерированные record-члены исключены из замера покрытия.</remarks>
[ExcludeFromCodeCoverage]
public sealed record HabitProgressRow(
    int Id, string Name, string Glyph, string ColorHex,
    int EffectiveStreak, int GoalDays, double Progress, bool IsDoneToday, bool IsGoalReached);