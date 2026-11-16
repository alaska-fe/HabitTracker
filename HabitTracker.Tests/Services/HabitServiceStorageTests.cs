using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;
using HabitTracker.Tests.Doubles;
using Xunit;

namespace HabitTracker.Tests.Services;

public class HabitServiceStorageTests
{
    private static readonly NewHabit ValidRequest = new("Пить воду", "", "water", "blue", 21);

    [Theory]
    [InlineData("GetAll")]
    [InlineData("Add")]
    [InlineData("Mark")]
    [InlineData("Delete")]
    [InlineData("Statistics")]
    public async Task AnyOperation_RepositoryFails_ThrowsStorageWithInner(string operation)
    {
        var inner = new InvalidOperationException("Имитация сбоя SQLite.");
        var service = new HabitService(
            new FailingHabitRepository(inner),
            new FakeClock(TestData.Day("2027-03-10")));

        var exception = await Assert.ThrowsAsync<HabitException>(() => operation switch
        {
            "GetAll" => service.GetAllAsync(),
            "Add" => service.AddAsync(ValidRequest),
            "Mark" => service.MarkCompletedTodayAsync(1),
            "Delete" => service.DeleteAsync(1),
            _ => service.GetStatisticsAsync()
        });

        Assert.Equal(HabitErrorKind.Storage, exception.Kind);
        Assert.Same(inner, exception.InnerException);
    }

    [Fact]
    public async Task TypedHabitException_FromRepository_PassesThroughUnchanged()
    {
        var typed = new HabitException(HabitErrorKind.NotFound, "Уже типизированная ошибка.");
        var service = new HabitService(
            new FailingHabitRepository(typed),
            new FakeClock(TestData.Day("2027-03-10")));

        var exception = await Assert.ThrowsAsync<HabitException>(() => service.MarkCompletedTodayAsync(1));

        Assert.Same(typed, exception); // не оборачивается повторно в Storage
    }
}