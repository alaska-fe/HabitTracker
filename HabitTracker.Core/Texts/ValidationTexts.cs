namespace HabitTracker.Core.Texts;

/// <summary>
/// Тексты ошибок валидации (ТЗ 9.3) — единое место хранения, попадают в FieldErrors.
/// </summary>
public static class ValidationTexts
{
    public const string NameRequired = "Введите название привычки";
    public const string NameTooShort = "Название должно содержать не менее 3 символов";
    public const string NameTooLong = "Название не должно превышать 50 символов";
    public const string DescriptionTooLong = "Описание не должно превышать 200 символов";

    // ТЗ 9.3 не задаёт тексты для этих случаев: в UI они невозможны (Picker всегда с выбором),
    // но сервис обязан проверять справочники (ТЗ 6.4) — формулируем нейтрально.
    public const string IconKeyInvalid = "Выберите иконку из списка";
    public const string ColorKeyInvalid = "Выберите цвет из списка";
    public const string GoalDaysInvalid = "Выберите цель из списка";
}