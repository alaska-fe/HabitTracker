using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Texts;

namespace HabitTracker.Core.Validation;

public static class HabitValidator
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 50;
    public const int DescriptionMaxLength = 200;

    public static string NormalizeName(string name)
    {
        var words = name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words);
    }

    public static string? GetNameError(string name)
    {
        var normalized = NormalizeName(name);

        if (normalized.Length == 0)
        {
            return ValidationTexts.NameRequired;
        }

        if (normalized.Length < NameMinLength)
        {
            return ValidationTexts.NameTooShort;
        }

        if (normalized.Length > NameMaxLength)
        {
            return ValidationTexts.NameTooLong;
        }

        return null;
    }

    public static string? GetDescriptionError(string description) =>
        description.Trim().Length > DescriptionMaxLength ? ValidationTexts.DescriptionTooLong : null;

    public static NewHabit Validate(NewHabit request)
    {
        var name = NormalizeName(request.Name);
        var description = request.Description.Trim();
        var errors = new Dictionary<string, string>();

        var nameError = GetNameError(request.Name);
        if (nameError is not null)
        {
            errors[nameof(NewHabit.Name)] = nameError;
        }

        var descriptionError = GetDescriptionError(request.Description);
        if (descriptionError is not null)
        {
            errors[nameof(NewHabit.Description)] = descriptionError;
        }

        if (!HabitIcons.All.Any(option => option.Key == request.IconKey))
        {
            errors[nameof(NewHabit.IconKey)] = ValidationTexts.IconKeyInvalid;
        }

        if (!HabitColors.All.Any(option => option.Key == request.ColorKey))
        {
            errors[nameof(NewHabit.ColorKey)] = ValidationTexts.ColorKeyInvalid;
        }

        if (!HabitGoals.All.Contains(request.GoalDays))
        {
            errors[nameof(NewHabit.GoalDays)] = ValidationTexts.GoalDaysInvalid;
        }

        if (errors.Count > 0)
        {
            throw new HabitException(HabitErrorKind.ValidationFailed, "Нарушены правила ввода привычки.", errors);
        }

        return request with { Name = name, Description = description };
    }
}