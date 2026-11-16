using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;
using HabitTracker.Tests.Doubles;
using Xunit;

namespace HabitTracker.Tests.Services;

public class HabitServiceAddTests
{
    private static readonly DateTime Today = TestData.Day("2027-03-10");

    private readonly FakeClock _clock = new(Today);
    private readonly InMemoryHabitRepository _repository = new();
    private readonly HabitService _service;

    public HabitServiceAddTests()
    {
        _service = new HabitService(_repository, _clock);
    }

    private static NewHabit Request(string name = "Пить воду") =>
        new(name, "2 литра в день", "water", "blue", 21);

    [Fact]
    public async Task AddAsync_ValidRequest_PersistsNormalizedHabit()
    {
        var habit = await _service.AddAsync(Request(name: "  Пить   воду  "));

        Assert.True(habit.Id > 0);
        Assert.Equal("Пить воду", habit.Name);
        Assert.Equal("2 литра в день", habit.Description);
        Assert.Equal("water", habit.IconKey);
        Assert.Equal("blue", habit.ColorKey);
        Assert.Equal(21, habit.GoalDays);
        Assert.Equal(0, habit.CurrentStreak);
        Assert.Equal(0, habit.BestStreak);
        Assert.Null(habit.LastCompletedDate);
        Assert.Equal(Today, habit.CreatedAt); // из FakeClock, не DateTime.Now

        var stored = await _repository.GetByIdAsync(habit.Id);
        Assert.NotNull(stored);
        Assert.Equal("Пить воду", stored!.Name);
        Assert.Equal(1, _repository.InsertCalls);
    }

    [Fact]
    public async Task AddAsync_DuplicateNameIgnoringCase_Throws()
    {
        await _service.AddAsync(Request(name: "Пить воду"));

        var exception = await Assert.ThrowsAsync<HabitException>(() => _service.AddAsync(Request(name: "пить ВОДУ")));

        Assert.Equal(HabitErrorKind.DuplicateName, exception.Kind);
        Assert.Single(await _repository.GetAllAsync());
    }

    [Fact]
    public async Task AddAsync_DuplicateAfterNormalization_Throws()
    {
        await _service.AddAsync(Request(name: "Пить воду"));

        var exception = await Assert.ThrowsAsync<HabitException>(
            () => _service.AddAsync(Request(name: "  пить   воду  ")));

        Assert.Equal(HabitErrorKind.DuplicateName, exception.Kind);
    }

    [Fact]
    public async Task AddAsync_AtLimit_ThrowsLimitReached()
    {
        for (var i = 1; i <= HabitService.MaxHabits; i++)
        {
            _repository.Seed(TestData.CreateHabit(id: i, name: $"Привычка {i}"));
        }

        var exception = await Assert.ThrowsAsync<HabitException>(() => _service.AddAsync(Request(name: "Новая")));

        Assert.Equal(HabitErrorKind.LimitReached, exception.Kind);
        Assert.Equal(HabitService.MaxHabits, (await _repository.GetAllAsync()).Count);
        Assert.Equal(0, _repository.InsertCalls);
    }

    [Fact]
    public async Task AddAsync_LastAllowedHabit_Succeeds()
    {
        for (var i = 1; i < HabitService.MaxHabits; i++)
        {
            _repository.Seed(TestData.CreateHabit(id: i, name: $"Привычка {i}"));
        }

        var habit = await _service.AddAsync(Request(name: "Тридцатая"));

        Assert.True(habit.Id > 0);
        Assert.Equal(HabitService.MaxHabits, (await _repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task AddAsync_InvalidRequest_ThrowsValidation_AndNothingWritten()
    {
        var exception = await Assert.ThrowsAsync<HabitException>(() => _service.AddAsync(Request(name: "ab")));

        Assert.Equal(HabitErrorKind.ValidationFailed, exception.Kind);
        Assert.NotEmpty(exception.FieldErrors);
        Assert.Empty(await _repository.GetAllAsync());
        Assert.Equal(0, _repository.InsertCalls);
    }
}