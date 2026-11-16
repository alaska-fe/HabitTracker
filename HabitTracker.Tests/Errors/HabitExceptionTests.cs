using HabitTracker.Core.Errors;
using Xunit;

namespace HabitTracker.Tests.Errors;

public class HabitExceptionTests
{
    [Fact]
    public void Constructor_PreservesKindAndMessage()
    {
        var exception = new HabitException(HabitErrorKind.DuplicateName, "служебное сообщение для журнала");

        Assert.Equal(HabitErrorKind.DuplicateName, exception.Kind);
        Assert.Equal("служебное сообщение для журнала", exception.Message);
    }

    [Fact]
    public void Constructor_WithoutFieldErrors_ExposesEmptyDictionary()
    {
        var exception = new HabitException(HabitErrorKind.NotFound, "не найдено");

        Assert.NotNull(exception.FieldErrors);
        Assert.Empty(exception.FieldErrors);
    }

    [Fact]
    public void Constructor_WithFieldErrors_PreservesThem()
    {
        var fieldErrors = new Dictionary<string, string>
        {
            ["Name"] = "Введите название привычки"
        };

        var exception = new HabitException(HabitErrorKind.ValidationFailed, "валидация", fieldErrors);

        Assert.Same(fieldErrors, exception.FieldErrors);
        Assert.Equal("Введите название привычки", exception.FieldErrors["Name"]);
    }

    [Fact]
    public void Constructor_PreservesInnerException()
    {
        var inner = new InvalidOperationException("сбой БД");

        var exception = new HabitException(HabitErrorKind.Storage, "сбой хранилища", inner: inner);

        Assert.Same(inner, exception.InnerException);
    }
}