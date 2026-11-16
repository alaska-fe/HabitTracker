using HabitTracker.Core.Rules;
using Xunit;

namespace HabitTracker.Tests.Rules;

public class HabitRulesEffectiveStreakTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");

    // Таблица эффективной серии ТЗ 6.5 (today = 10.03) + дополнительная строка правила 6.2.
    [Theory]
    [InlineData(null,         0,  0,  false)] // E1
    [InlineData("2027-03-10", 5,  5,  false)] // E2
    [InlineData("2027-03-09", 5,  5,  false)] // E3
    [InlineData("2027-03-08", 5,  0,  true)]  // E4
    [InlineData("2027-01-01", 12, 0,  true)]  // E5
    [InlineData("2027-03-11", 5,  5,  false)] // доп.: last > today — дата устройства некорректна
    public void EffectiveStreak_ControlScenarios(
        string? lastDate,
        int currentStreak,
        int expectedEffective,
        bool expectedBroken)
    {
        DateTime? last = lastDate is null ? null : TestData.Day(lastDate);
        var habit = TestData.CreateHabit(currentStreak: currentStreak, lastCompletedDate: last);

        Assert.Equal(expectedEffective, HabitRules.GetEffectiveStreak(habit, Today));
        Assert.Equal(expectedBroken, HabitRules.IsStreakBroken(habit, Today));
    }

    [Theory]
    [InlineData(null,         false)]
    [InlineData("2027-03-10", true)]
    [InlineData("2027-03-09", false)]
    [InlineData("2027-03-11", false)]
    public void IsDoneToday_OnlyForToday(string? lastDate, bool expected)
    {
        DateTime? last = lastDate is null ? null : TestData.Day(lastDate);
        var habit = TestData.CreateHabit(lastCompletedDate: last);

        Assert.Equal(expected, HabitRules.IsDoneToday(habit, Today));
    }

    [Fact]
    public void GetEffectiveStreak_IgnoresTimeOfDay()
    {
        var habit = TestData.CreateHabit(
            currentStreak: 5,
            lastCompletedDate: new DateTime(2027, 3, 9, 22, 0, 0));

        Assert.Equal(5, HabitRules.GetEffectiveStreak(habit, Today));
    }
}