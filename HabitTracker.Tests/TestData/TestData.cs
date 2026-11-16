using System.Globalization;
using HabitTracker.Core.Models;

namespace HabitTracker.Tests;

/// <summary>
/// Фабрика тестовых данных: привычка с заданными полями и разбор дат в инвариантной культуре.
/// Даты в тестах фиксированные — набор не зависит от текущего календаря (ТЗ 16).
/// </summary>
internal static class TestData
{
    public static DateTime Day(string isoDate) =>
        DateTime.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static Habit CreateHabit(
        int id = 1,
        string name = "Привычка",
        int goalDays = 21,
        int currentStreak = 0,
        int bestStreak = 0,
        DateTime? lastCompletedDate = null,
        string iconKey = "book",
        string colorKey = "blue") => new()
    {
        Id = id,
        Name = name,
        Description = string.Empty,
        IconKey = iconKey,
        ColorKey = colorKey,
        GoalDays = goalDays,
        CurrentStreak = currentStreak,
        BestStreak = bestStreak,
        LastCompletedDate = lastCompletedDate,
        CreatedAt = Day("2027-01-01")
    };
}