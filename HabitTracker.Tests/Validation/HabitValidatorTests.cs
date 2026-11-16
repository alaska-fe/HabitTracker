using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Texts;
using HabitTracker.Core.Validation;
using Xunit;

namespace HabitTracker.Tests.Validation;

public class HabitValidatorTests
{
    private static NewHabit Request(
        string name = "Пить воду",
        string description = "",
        string iconKey = "water",
        string colorKey = "blue",
        int goalDays = 21) => new(name, description, iconKey, colorKey, goalDays);

    [Theory]
    [InlineData("  Пить   воду  ", "Пить воду")]
    [InlineData("Пить воду", "Пить воду")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  А  Б  В  ", "А Б В")]
    public void NormalizeName_TrimsAndCollapsesSpaces(string input, string expected)
    {
        Assert.Equal(expected, HabitValidator.NormalizeName(input));
    }

    [Theory]
    [InlineData(2, ValidationTexts.NameTooShort)]
    [InlineData(3, null)]
    [InlineData(50, null)]
    [InlineData(51, ValidationTexts.NameTooLong)]
    public void Validate_NameLengthBoundaries(int length, string? expectedError)
    {
        var request = Request(name: new string('а', length));

        if (expectedError is null)
        {
            var validated = HabitValidator.Validate(request);
            Assert.Equal(length, validated.Name.Length);
        }
        else
        {
            var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(request));
            Assert.Equal(HabitErrorKind.ValidationFailed, exception.Kind);
            Assert.Equal(expectedError, exception.FieldErrors[nameof(NewHabit.Name)]);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyOrWhitespaceName_Required(string name)
    {
        var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(Request(name: name)));

        Assert.Equal(HabitErrorKind.ValidationFailed, exception.Kind);
        Assert.Equal(ValidationTexts.NameRequired, exception.FieldErrors[nameof(NewHabit.Name)]);
    }

    [Theory]
    [InlineData(200, null)]
    [InlineData(201, ValidationTexts.DescriptionTooLong)]
    public void Validate_DescriptionLengthBoundaries(int length, string? expectedError)
    {
        var request = Request(description: new string('о', length));

        if (expectedError is null)
        {
            _ = HabitValidator.Validate(request);
        }
        else
        {
            var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(request));
            Assert.Equal(expectedError, exception.FieldErrors[nameof(NewHabit.Description)]);
        }
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(14, true)]
    [InlineData(21, true)]
    [InlineData(30, true)]
    [InlineData(66, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(5, false)] // как в S12: правила цель не валидируют, а сервис — да
    [InlineData(8, false)]
    [InlineData(22, false)]
    [InlineData(101, false)]
    public void Validate_GoalDays_FromCatalogOnly(int goalDays, bool isValid)
    {
        var request = Request(goalDays: goalDays);

        if (isValid)
        {
            var validated = HabitValidator.Validate(request);
            Assert.Equal(goalDays, validated.GoalDays);
        }
        else
        {
            var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(request));
            Assert.Equal(ValidationTexts.GoalDaysInvalid, exception.FieldErrors[nameof(NewHabit.GoalDays)]);
        }
    }

    [Fact]
    public void Validate_UnknownIconKey_Throws()
    {
        var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(Request(iconKey: "unknown")));

        Assert.Equal(HabitErrorKind.ValidationFailed, exception.Kind);
        Assert.Equal(ValidationTexts.IconKeyInvalid, exception.FieldErrors[nameof(NewHabit.IconKey)]);
    }

    [Fact]
    public void Validate_UnknownColorKey_Throws()
    {
        var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(Request(colorKey: "unknown")));

        Assert.Equal(ValidationTexts.ColorKeyInvalid, exception.FieldErrors[nameof(NewHabit.ColorKey)]);
    }

    [Fact]
    public void Validate_CollectsAllFieldErrors_AtOnce()
    {
        var request = Request(name: "", iconKey: "unknown", goalDays: 5);

        var exception = Assert.Throws<HabitException>(() => HabitValidator.Validate(request));

        Assert.Equal(3, exception.FieldErrors.Count);
        Assert.Contains(nameof(NewHabit.Name), exception.FieldErrors.Keys);
        Assert.Contains(nameof(NewHabit.IconKey), exception.FieldErrors.Keys);
        Assert.Contains(nameof(NewHabit.GoalDays), exception.FieldErrors.Keys);
    }

    [Fact]
    public void Validate_Success_ReturnsNormalizedRequest()
    {
        var request = Request(name: "  Пить   воду  ", description: "  2 литра в день  ");

        var validated = HabitValidator.Validate(request);

        Assert.Equal("Пить воду", validated.Name);
        Assert.Equal("2 литра в день", validated.Description);
        Assert.Equal("water", validated.IconKey);
        Assert.Equal("blue", validated.ColorKey);
        Assert.Equal(21, validated.GoalDays);
    }
}