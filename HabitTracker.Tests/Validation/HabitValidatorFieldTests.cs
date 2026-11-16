using HabitTracker.Core.Texts;
using HabitTracker.Core.Validation;
using Xunit;

namespace HabitTracker.Tests.Validation;

public class HabitValidatorFieldTests
{
    [Theory]
    [InlineData("", ValidationTexts.NameRequired)]
    [InlineData("   ", ValidationTexts.NameRequired)]
    [InlineData("ab", ValidationTexts.NameTooShort)]
    [InlineData("abc", null)]
    [InlineData("  Пить   воду  ", null)]
    public void GetNameError_ReturnsCatalogMessage(string name, string? expected)
    {
        Assert.Equal(expected, HabitValidator.GetNameError(name));
    }

    [Fact]
    public void GetNameError_TooLong_ReturnsMessage()
    {
        Assert.Equal(ValidationTexts.NameTooLong, HabitValidator.GetNameError(new string('а', 51)));
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("  2 литра  ", null)]
    public void GetDescriptionError_ShortText_ReturnsNull(string description, string? expected)
    {
        Assert.Equal(expected, HabitValidator.GetDescriptionError(description));
    }

    [Fact]
    public void GetDescriptionError_TooLong_ReturnsMessage()
    {
        Assert.Equal(ValidationTexts.DescriptionTooLong, HabitValidator.GetDescriptionError(new string('о', 201)));
    }
}