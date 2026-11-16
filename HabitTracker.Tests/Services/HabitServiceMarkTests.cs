using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;
using HabitTracker.Tests.Doubles;
using Xunit;

namespace HabitTracker.Tests.Services;

public class HabitServiceMarkTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");

    private readonly FakeClock _clock = new(Today);
    private readonly InMemoryHabitRepository _repository = new();
    private readonly HabitService _service;

    public HabitServiceMarkTests()
    {
        _service = new HabitService(_repository, _clock);
    }

    [Fact]
    public async Task MarkAsync_FirstTime_CompletedAndPersisted()
    {
        _repository.Seed(TestData.CreateHabit(id: 1));

        var result = await _service.MarkCompletedTodayAsync(1);

        Assert.Equal(MarkOutcome.Completed, result.Outcome);
        Assert.Equal(1, result.Habit.CurrentStreak);
        Assert.Equal(1, result.Habit.BestStreak);
        Assert.Equal(Today, result.Habit.LastCompletedDate);

        var stored = await _repository.GetByIdAsync(1);
        Assert.Equal(1, stored!.CurrentStreak);
        Assert.Equal(Today, stored.LastCompletedDate);
        Assert.Equal(1, _repository.UpdateCalls);
    }

    [Fact]
    public async Task MarkAsync_ConsecutiveDay_IncrementsStreak()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, currentStreak: 3, bestStreak: 5,
            lastCompletedDate: Today.AddDays(-1)));

        var result = await _service.MarkCompletedTodayAsync(1);

        Assert.Equal(MarkOutcome.Completed, result.Outcome);
        Assert.Equal(4, result.Habit.CurrentStreak);
        Assert.Equal(5, result.Habit.BestStreak);
    }

    [Fact]
    public async Task MarkAsync_AlreadyDoneToday_NoWrite()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, currentStreak: 3, bestStreak: 5, lastCompletedDate: Today));

        var result = await _service.MarkCompletedTodayAsync(1);

        Assert.Equal(MarkOutcome.AlreadyDoneToday, result.Outcome);
        Assert.Equal(3, result.Habit.CurrentStreak);
        Assert.Equal(0, _repository.UpdateCalls); // R2: в БД не пишем
    }

    [Fact]
    public async Task MarkAsync_GoalReached_ReturnedOnce()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, goalDays: 21, currentStreak: 20, bestStreak: 20,
            lastCompletedDate: Today.AddDays(-1)));

        var result = await _service.MarkCompletedTodayAsync(1);

        Assert.Equal(MarkOutcome.GoalReached, result.Outcome);
        Assert.Equal(21, result.Habit.CurrentStreak);
        Assert.Equal(21, result.Habit.BestStreak);
    }

    [Fact]
    public async Task MarkAsync_Gap_ResetsStreakToOne()
    {
        _repository.Seed(TestData.CreateHabit(id: 1, currentStreak: 7, bestStreak: 7,
            lastCompletedDate: Today.AddDays(-3)));

        var result = await _service.MarkCompletedTodayAsync(1);

        Assert.Equal(MarkOutcome.Completed, result.Outcome);
        Assert.Equal(1, result.Habit.CurrentStreak);
        Assert.Equal(7, result.Habit.BestStreak); // рекорд сохранён (R4/R6)
    }

    [Fact]
    public async Task MarkAsync_MissingHabit_NotFound()
    {
        var exception = await Assert.ThrowsAsync<HabitException>(() => _service.MarkCompletedTodayAsync(99));

        Assert.Equal(HabitErrorKind.NotFound, exception.Kind);
        Assert.Equal(0, _repository.UpdateCalls);
    }

    [Fact]
    public async Task MarkAsync_ClockMovedBack_ThrowsAndDataUnchanged()
    {
        _repository.Seed(TestData.CreateHabit(id: 1));
        await _service.MarkCompletedTodayAsync(1); // отмечено 10.03

        _clock.SetToday(Today.AddDays(-1)); // устройство «откатилось» на 09.03

        var exception = await Assert.ThrowsAsync<HabitException>(() => _service.MarkCompletedTodayAsync(1));

        Assert.Equal(HabitErrorKind.ClockMovedBack, exception.Kind);
        var stored = await _repository.GetByIdAsync(1);
        Assert.Equal(1, stored!.CurrentStreak);
        Assert.Equal(Today, stored.LastCompletedDate);
        Assert.Equal(1, _repository.UpdateCalls); // второй записи не было
    }

    [Fact]
    public async Task MarkAsync_NextDay_AfterMidnight_Increments()
    {
        _repository.Seed(TestData.CreateHabit(id: 1));
        await _service.MarkCompletedTodayAsync(1);

        _clock.AdvanceDays(1); // «наступило завтра»
        var result = await _service.MarkCompletedTodayAsync(1);

        Assert.Equal(MarkOutcome.Completed, result.Outcome);
        Assert.Equal(2, result.Habit.CurrentStreak);
    }

    [Fact]
    public async Task MarkAsync_UpdateFails_StorageAndDataUnchanged()
    {
        var repository = new UpdateFailingRepository();
        repository.Seed(TestData.CreateHabit(id: 1, currentStreak: 3, bestStreak: 5,
            lastCompletedDate: Today.AddDays(-1)));
        var service = new HabitService(repository, _clock);

        var exception = await Assert.ThrowsAsync<HabitException>(() => service.MarkCompletedTodayAsync(1));

        Assert.Equal(HabitErrorKind.Storage, exception.Kind);
        var stored = await repository.GetByIdAsync(1);
        Assert.Equal(3, stored!.CurrentStreak); // в «БД» данные прежние
        Assert.Equal(Today.AddDays(-1), stored.LastCompletedDate);
    }

    private sealed class UpdateFailingRepository : InMemoryHabitRepository
    {
        public override Task UpdateAsync(Habit habit) =>
            Task.FromException(new InvalidOperationException("Имитация сбоя записи."));
    }
}