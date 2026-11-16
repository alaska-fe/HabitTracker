using HabitTracker.Core.Services;
using HabitTracker.Tests.Doubles;
using Xunit;

namespace HabitTracker.Tests.Services;

public class HabitServiceReadTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");

    private readonly FakeClock _clock = new(Today);
    private readonly InMemoryHabitRepository _repository = new();
    private readonly HabitService _service;

    public HabitServiceReadTests()
    {
        _service = new HabitService(_repository, _clock);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRepositoryData()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, name: "Пить воду"));
        _repository.Seed(TestData.CreateHabit(id: 2, name: "Читать"));

        var habits = await _service.GetAllAsync();

        Assert.Equal(2, habits.Count);
        Assert.Equal(new[] { 1, 2 }, habits.Select(habit => habit.Id));
    }

    [Fact]
    public async Task GetStatisticsAsync_EmptyRepository_AllZeros()
    {
        var statistics = await _service.GetStatisticsAsync();

        Assert.Equal(0, statistics.TotalHabits);
        Assert.Empty(statistics.Rows);
    }

    [Fact]
    public async Task GetStatisticsAsync_UsesClockToday_ForEffectiveStreak()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, currentStreak: 5, bestStreak: 5,
            lastCompletedDate: Today.AddDays(-2))); // серия прервана относительно часов

        var statistics = await _service.GetStatisticsAsync();

        Assert.Equal(0, statistics.LongestCurrentStreak);
        Assert.Equal(5, statistics.LongestEverStreak);
        Assert.Equal(0, statistics.DoneToday);
    }

    [Fact]
    public async Task GetStatisticsAsync_MixedHabits_Aggregates()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, name: "Пить воду", currentStreak: 7, bestStreak: 7,
            lastCompletedDate: Today));
        _repository.Seed(TestData.CreateHabit(id: 2, name: "Читать", currentStreak: 21, bestStreak: 25,
            lastCompletedDate: Today));
        _repository.Seed(TestData.CreateHabit(id: 3, name: "Бег"));

        var statistics = await _service.GetStatisticsAsync();

        Assert.Equal(3, statistics.TotalHabits);
        Assert.Equal(2, statistics.DoneToday);
        Assert.Equal(1, statistics.GoalsReached);
        Assert.Equal(44.4, statistics.AverageProgressPercent);
        Assert.Equal(new[] { 2, 1, 3 }, statistics.Rows.Select(row => row.Id));
    }
}