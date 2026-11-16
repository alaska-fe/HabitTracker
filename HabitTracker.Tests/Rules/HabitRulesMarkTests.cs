using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;
using Xunit;

namespace HabitTracker.Tests.Rules;

public class HabitRulesMarkTests
{
    // Контрольная таблица ТЗ 6.5 (S5 — исключение — отдельным тестом ниже).
    // Даты без года из ТЗ — 2027; високосные S7/S8 — 2028; граница года S9 — 2026→2027.
    [Theory]
    [InlineData(null,         "2027-03-10", 0,  0,  1,  1,  MarkOutcome.Completed,        21)] // S1
    [InlineData("2027-03-09", "2027-03-10", 3,  5,  4,  5,  MarkOutcome.Completed,        21)] // S2
    [InlineData("2027-03-10", "2027-03-10", 3,  5,  3,  5,  MarkOutcome.AlreadyDoneToday, 21)] // S3
    [InlineData("2027-03-08", "2027-03-10", 7,  7,  1,  7,  MarkOutcome.Completed,        21)] // S4
    [InlineData("2027-01-31", "2027-02-01", 4,  5,  5,  5,  MarkOutcome.Completed,        21)] // S6
    [InlineData("2028-02-28", "2028-02-29", 2,  5,  3,  5,  MarkOutcome.Completed,        21)] // S7
    [InlineData("2028-02-29", "2028-03-01", 2,  5,  3,  5,  MarkOutcome.Completed,        21)] // S8
    [InlineData("2026-12-31", "2027-01-01", 9,  9,  10, 10, MarkOutcome.Completed,        21)] // S9
    [InlineData("2027-03-09", "2027-03-10", 20, 20, 21, 21, MarkOutcome.GoalReached,      21)] // S10
    [InlineData("2027-03-09", "2027-03-10", 21, 21, 22, 22, MarkOutcome.Completed,        21)] // S11
    [InlineData("2027-03-09", "2027-03-10", 4,  4,  5,  5,  MarkOutcome.GoalReached,      5)]  // S12
    public void MarkCompletedToday_ControlScenarios(
        string? lastDate,
        string todayDate,
        int currentBefore,
        int bestBefore,
        int expectedCurrent,
        int expectedBest,
        MarkOutcome expectedOutcome,
        int goalDays)
    {
        var today = TestData.Day(todayDate);
        DateTime? last = lastDate is null ? null : TestData.Day(lastDate);
        var habit = TestData.CreateHabit(
            goalDays: goalDays,
            currentStreak: currentBefore,
            bestStreak: bestBefore,
            lastCompletedDate: last);

        var result = HabitRules.MarkCompletedToday(habit, today);

        Assert.Equal(expectedOutcome, result.Outcome);
        Assert.Equal(expectedCurrent, habit.CurrentStreak);
        Assert.Equal(expectedBest, habit.BestStreak);
        Assert.Same(habit, result.Habit);

        if (expectedOutcome == MarkOutcome.AlreadyDoneToday)
        {
            Assert.Equal(last, habit.LastCompletedDate); // R2: данные не меняются
        }
        else
        {
            Assert.Equal(today, habit.LastCompletedDate);
        }
    }

    [Fact]
    public void MarkCompletedToday_ClockMovedBack_ThrowsAndKeepsData() // S5
    {
        var last = TestData.Day("2027-03-11");
        var habit = TestData.CreateHabit(currentStreak: 3, bestStreak: 5, lastCompletedDate: last);

        var exception = Assert.Throws<HabitException>(
            () => HabitRules.MarkCompletedToday(habit, TestData.Day("2027-03-10")));

        Assert.Equal(HabitErrorKind.ClockMovedBack, exception.Kind);
        Assert.Equal(3, habit.CurrentStreak);
        Assert.Equal(5, habit.BestStreak);
        Assert.Equal(last, habit.LastCompletedDate);
    }

    [Fact]
    public void MarkCompletedToday_IgnoresTimeOfDay_InStoredDate()
    {
        // Повреждённые данные: дата со временем. Сравниваем только даты (ТЗ 16).
        var habit = TestData.CreateHabit(
            currentStreak: 3,
            bestStreak: 5,
            lastCompletedDate: new DateTime(2027, 3, 9, 23, 45, 0));

        var result = HabitRules.MarkCompletedToday(habit, new DateTime(2027, 3, 10, 8, 30, 0));

        Assert.Equal(MarkOutcome.Completed, result.Outcome);
        Assert.Equal(4, habit.CurrentStreak);
        Assert.Equal(new DateTime(2027, 3, 10), habit.LastCompletedDate); // время обнулено
    }

    [Fact]
    public void MarkCompletedToday_GoalReached_ExactlyOnce()
    {
        var habit = TestData.CreateHabit(goalDays: 7);
        var start = TestData.Day("2027-03-01");
        var outcomes = new List<MarkOutcome>();

        for (var i = 0; i < 8; i++)
        {
            outcomes.Add(HabitRules.MarkCompletedToday(habit, start.AddDays(i)).Outcome);
        }

        Assert.Equal(
            new[]
            {
                MarkOutcome.Completed, MarkOutcome.Completed, MarkOutcome.Completed,
                MarkOutcome.Completed, MarkOutcome.Completed, MarkOutcome.Completed,
                MarkOutcome.GoalReached, // день 7 — ровно один раз (R7)
                MarkOutcome.Completed    // день 8 — серия выше цели
            },
            outcomes);
        Assert.Equal(8, habit.CurrentStreak);
        Assert.Equal(8, habit.BestStreak);
    }
}