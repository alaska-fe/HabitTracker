using HabitTracker.Core.Errors;
using HabitTracker.Core.Services;
using HabitTracker.Tests.Doubles;
using Xunit;

namespace HabitTracker.Tests.Services;

public class HabitServiceDeleteTests
{
    private readonly FakeClock _clock = new(TestData.Day("2027-03-10"));
    private readonly InMemoryHabitRepository _repository = new();
    private readonly HabitService _service;

    public HabitServiceDeleteTests()
    {
        _service = new HabitService(_repository, _clock);
    }

    [Fact]
    public async Task DeleteAsync_Existing_Removes()
    {
        _repository.Seed(TestData.CreateHabit(id: 1));

        await _service.DeleteAsync(1);

        Assert.Empty(await _repository.GetAllAsync());
        Assert.Equal(1, _repository.DeleteCalls);
    }

    [Fact]
    public async Task DeleteAsync_Missing_NotFound()
    {
        var exception = await Assert.ThrowsAsync<HabitException>(() => _service.DeleteAsync(99));

        Assert.Equal(HabitErrorKind.NotFound, exception.Kind);
        Assert.Equal(0, _repository.DeleteCalls);
    }
}