using HabitTracker.Core.Catalogs;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Texts;

namespace HabitTracker.Core.Validation;

/// <summary>
/// Валидация и нормализация запроса на создание (ТЗ 6.4, 9.3) — чистая статическая логика.
/// Собирает все ошибки сразу и бросает HabitException(ValidationFailed) со словарём «поле → сообщение».
/// </summary>
public static class HabitValidator
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 50;
    public const int DescriptionMaxLength = 200;

    /// <summary>Обрезает пробелы по краям и схлопывает повторные пробелы внутри (ТЗ 6.4).</summary>
    public static string NormalizeName(string name)
    {
        // Split(null) разбивает по любым пробельным символам, RemoveEmptyEntries убирает повторы.
        var words = name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words);
    }

    /// <summary>Проверяет все поля; возвращает нормализованный запрос или бросает ValidationFailed.</summary>
    public static NewHabit Validate(NewHabit request)
    {
        var name = NormalizeName(request.Name);
        var description = request.Description.Trim();
        var errors = new Dictionary<string, string>();

        if (name.Length == 0)
        {
            errors[nameof(NewHabit.Name)] = ValidationTexts.NameRequired;
        }
        else if (name.Length < NameMinLength)
        {
            errors[nameof(NewHabit.Name)] = ValidationTexts.NameTooShort;
        }
        else if (name.Length > NameMaxLength)
        {
            errors[nameof(NewHabit.Name)] = ValidationTexts.NameTooLong;
        }

        if (description.Length > DescriptionMaxLength)
        {
            errors[nameof(NewHabit.Description)] = ValidationTexts.DescriptionTooLong;
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