using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;
using Xunit;

namespace HabitTracker.Tests.Rules;

public class HabitSorterTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");
    private static readonly DateTime Yesterday = TestData.Day("2027-03-09");

    [Fact]
    public void SortForToday_NotDone_BeforeDone_EvenWithSmallerStreak()
    {
        var done = TestData.CreateHabit(id: 1, name: "Выполненная", currentStreak: 50, bestStreak: 50, lastCompletedDate: Today);
        var notDone = TestData.CreateHabit(id: 2, name: "Невыполненная", currentStreak: 1, bestStreak: 1, lastCompletedDate: Yesterday);

        var sorted = HabitSorter.SortForToday(new[] { done, notDone }, Today);

        Assert.Equal(new[] { 2, 1 }, sorted.Select(habit => habit.Id));
    }

    [Fact]
    public void SortForToday_WithinNotDone_ByEffectiveStreakDescending()
    {
        var low = TestData.CreateHabit(id: 1, currentStreak: 3, lastCompletedDate: Yesterday);
        var high = TestData.CreateHabit(id: 2, currentStreak: 10, lastCompletedDate: Yesterday);
        var mid = TestData.CreateHabit(id: 3, currentStreak: 7, lastCompletedDate: Yesterday);

        var sorted = HabitSorter.SortForToday(new[] { low, high, mid }, Today);

        Assert.Equal(new[] { 2, 3, 1 }, sorted.Select(habit => habit.Id));
    }

    [Fact]
    public void SortForToday_EqualStreak_ByNameIgnoringCase()
    {
        var yabloko = TestData.CreateHabit(id: 1, name: "яблоко", currentStreak: 5, lastCompletedDate: Yesterday);
        var abrikos = TestData.CreateHabit(id: 2, name: "Абрикос", currentStreak: 5, lastCompletedDate: Yesterday);
        var banan = TestData.CreateHabit(id: 3, name: "Банан", currentStreak: 5, lastCompletedDate: Yesterday);

        var sorted = HabitSorter.SortForToday(new[] { yabloko, abrikos, banan }, Today);

        Assert.Equal(new[] { 2, 3, 1 }, sorted.Select(habit => habit.Id));
    }

    [Fact]
    public void SortForToday_BrokenStreak_SortsAsZero()
    {
        var broken = TestData.CreateHabit(id: 1, name: "Прервана", currentStreak: 12, bestStreak: 12,
            lastCompletedDate: TestData.Day("2027-03-01"));
        var fresh = TestData.CreateHabit(id: 2, name: "Свежая", currentStreak: 1, bestStreak: 1,
            lastCompletedDate: Yesterday);

        var sorted = HabitSorter.SortForToday(new[] { broken, fresh }, Today);

        Assert.Equal(new[] { 2, 1 }, sorted.Select(habit => habit.Id));
    }

    [Fact]
    public void SortForToday_DoneGroup_AlsoOrderedByStreak()
    {
        var notDone = TestData.CreateHabit(id: 1, currentStreak: 1, lastCompletedDate: Yesterday);
        var doneLow = TestData.CreateHabit(id: 2, currentStreak: 5, lastCompletedDate: Today);
        var doneHigh = TestData.CreateHabit(id: 3, currentStreak: 9, lastCompletedDate: Today);

        var sorted = HabitSorter.SortForToday(new[] { doneLow, notDone, doneHigh }, Today);

        Assert.Equal(new[] { 1, 3, 2 }, sorted.Select(habit => habit.Id));
    }

    [Fact]
    public void SortForToday_EmptyList_ReturnsEmpty()
    {
        var sorted = HabitSorter.SortForToday(Array.Empty<Habit>(), Today);

        Assert.Empty(sorted);
    }

    [Fact]
    public void SortForToday_DoesNotMutateSourceList()
    {
        var first = TestData.CreateHabit(id: 1, currentStreak: 1, lastCompletedDate: Yesterday);
        var second = TestData.CreateHabit(id: 2, currentStreak: 9, lastCompletedDate: Yesterday);
        var source = new List<Habit> { first, second };

        _ = HabitSorter.SortForToday(source, Today);

        Assert.Same(first, source[0]);
        Assert.Same(second, source[1]);
    }
}