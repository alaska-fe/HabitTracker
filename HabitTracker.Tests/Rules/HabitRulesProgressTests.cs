using HabitTracker.Core.Rules;
using Xunit;

namespace HabitTracker.Tests.Rules;

public class HabitRulesProgressTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");

    // ТЗ 11.1: 0 %, промежуточное, ровно цель, больше цели; цель минимальная (7) и максимальная (100).
    [Theory]
    [InlineData(0,   21,  0.0)]
    [InlineData(7,   21,  0.333333)]
    [InlineData(21,  21,  1.0)]
    [InlineData(25,  21,  1.0)]      // выше цели — индикатор остаётся на 100 %
    [InlineData(1,   7,   0.142857)]
    [InlineData(7,   7,   1.0)]
    [InlineData(1,   100, 0.01)]
    [InlineData(100, 100, 1.0)]
    public void GetProgress_ControlValues(int currentStreak, int goalDays, double expectedProgress)
    {
        var habit = TestData.CreateHabit(
            goalDays: goalDays,
            currentStreak: currentStreak,
            bestStreak: currentStreak,
            lastCompletedDate: currentStreak > 0 ? Today : null);

        Assert.Equal(expectedProgress, HabitRules.GetProgress(habit, Today), precision: 6);
    }

    [Fact]
    public void GetProgress_NonPositiveGoal_ReturnsZero()
    {
        // Повреждённые данные: GoalDays = 0 не должен давать NaN/Infinity (ТЗ 10.3).
        var habit = TestData.CreateHabit(goalDays: 0, currentStreak: 5, lastCompletedDate: Today);

        Assert.Equal(0.0, HabitRules.GetProgress(habit, Today));
    }

    [Theory]
    [InlineData(0,  21, false)]
    [InlineData(20, 21, false)]
    [InlineData(21, 21, true)]
    [InlineData(25, 21, true)]
    public void IsGoalReached_ByEffectiveStreak(int currentStreak, int goalDays, bool expected)
    {
        var habit = TestData.CreateHabit(
            goalDays: goalDays,
            currentStreak: currentStreak,
            lastCompletedDate: currentStreak > 0 ? Today : null);

        Assert.Equal(expected, HabitRules.IsGoalReached(habit, Today));
    }

    [Fact]
    public void IsGoalReached_NonPositiveGoal_ReturnsFalse()
    {
        var habit = TestData.CreateHabit(goalDays: 0, currentStreak: 0);

        Assert.False(HabitRules.IsGoalReached(habit, Today));
    }

    [Theory]
    [InlineData(0.0,          0)]
    [InlineData(0.125,        13)]  // 12.5 → 13: AwayFromZero, не «банковское» 12
    [InlineData(0.375,        38)]  // 37.5 → 38
    [InlineData(1.0,          100)]
    [InlineData(0.3333333333, 33)]
    public void GetProgressPercent_RoundsToWholeAwayFromZero(double progress, int expectedPercent)
    {
        Assert.Equal(expectedPercent, HabitRules.GetProgressPercent(progress));
    }
}