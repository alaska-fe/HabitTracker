using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Models;
using HabitTracker.Core.Statistics;
using Xunit;

namespace HabitTracker.Tests.Statistics;

public class HabitStatisticsCalculatorTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");

    [Fact]
    public void Calculate_EmptyList_AllZeros()
    {
        var stats = HabitStatisticsCalculator.Calculate(Array.Empty<Habit>(), Today);

        Assert.Equal(0, stats.TotalHabits);
        Assert.Equal(0, stats.DoneToday);
        Assert.Equal(0, stats.GoalsReached);
        Assert.Equal(0.0, stats.AverageProgressPercent);
        Assert.Equal(0, stats.LongestCurrentStreak);
        Assert.Equal(0, stats.LongestEverStreak);
        Assert.Empty(stats.Rows);
    }

    [Fact]
    public void Calculate_MixedHabits_AggregatesAndOrdersRows()
    {
        var water = TestData.CreateHabit(id: 1, name: "Пить воду", iconKey: "water", colorKey: "blue",
            goalDays: 21, currentStreak: 7, bestStreak: 7, lastCompletedDate: Today);
        var read = TestData.CreateHabit(id: 2, name: "Читать", iconKey: "book", colorKey: "green",
            goalDays: 21, currentStreak: 21, bestStreak: 25, lastCompletedDate: Today);
        var run = TestData.CreateHabit(id: 3, name: "Бег", goalDays: 21);

        var stats = HabitStatisticsCalculator.Calculate(new[] { water, read, run }, Today);

        Assert.Equal(3, stats.TotalHabits);
        Assert.Equal(2, stats.DoneToday);
        Assert.Equal(1, stats.GoalsReached);
        Assert.Equal(44.4, stats.AverageProgressPercent); // (33.33… + 100 + 0) / 3
        Assert.Equal(21, stats.LongestCurrentStreak);
        Assert.Equal(25, stats.LongestEverStreak);

        Assert.Equal(new[] { 2, 1, 3 }, stats.Rows.Select(row => row.Id)); // по убыванию прогресса

        var readRow = stats.Rows[0];
        Assert.Equal("📚", readRow.Glyph);
        Assert.Equal("#16A34A", readRow.ColorHex);
        Assert.True(readRow.IsGoalReached);
        Assert.True(readRow.IsDoneToday);
        Assert.Equal(1.0, readRow.Progress);

        Assert.Equal(1.0 / 3, stats.Rows[1].Progress, precision: 10);
    }

    [Fact]
    public void Calculate_AveragePercent_RoundsHalfAwayFromZero()
    {
        var quarter = TestData.CreateHabit(id: 1, goalDays: 100, currentStreak: 25, bestStreak: 25, lastCompletedDate: Today);
        var empty1 = TestData.CreateHabit(id: 2);
        var empty2 = TestData.CreateHabit(id: 3);
        var empty3 = TestData.CreateHabit(id: 4);

        var stats = HabitStatisticsCalculator.Calculate(new[] { quarter, empty1, empty2, empty3 }, Today);

        // (25 + 0 + 0 + 0) / 4 = 6.25 → 6.3 (AwayFromZero, а не «банковское» 6.2)
        Assert.Equal(6.3, stats.AverageProgressPercent);
    }

    [Fact]
    public void Calculate_RowsWithEqualProgress_OrderedByNameIgnoringCase()
    {
        var yagoda = TestData.CreateHabit(id: 1, name: "Ягода");
        var abrikos = TestData.CreateHabit(id: 2, name: "абрикос");

        var stats = HabitStatisticsCalculator.Calculate(new[] { yagoda, abrikos }, Today);

        Assert.Equal(new[] { 2, 1 }, stats.Rows.Select(row => row.Id));
    }

    [Fact]
    public void Calculate_BrokenStreak_EverRecordComesFromBestStreak()
    {
        var broken = TestData.CreateHabit(id: 1, currentStreak: 12, bestStreak: 30,
            lastCompletedDate: TestData.Day("2027-03-01"));

        var stats = HabitStatisticsCalculator.Calculate(new[] { broken }, Today);

        Assert.Equal(0, stats.LongestCurrentStreak);
        Assert.Equal(30, stats.LongestEverStreak);
        Assert.Equal(0.0, stats.Rows[0].Progress);
        Assert.False(stats.Rows[0].IsGoalReached);
    }

    [Fact]
    public void Calculate_UnknownCatalogKeys_FallBackToDefaults()
    {
        var corrupted = TestData.CreateHabit(id: 1, iconKey: "unknown", colorKey: "unknown");

        var stats = HabitStatisticsCalculator.Calculate(new[] { corrupted }, Today);

        Assert.Equal(HabitIcons.GetGlyph(HabitIcons.Default), stats.Rows[0].Glyph);
        Assert.Equal(HabitColors.GetHex(HabitColors.Default), stats.Rows[0].ColorHex);
    }
}