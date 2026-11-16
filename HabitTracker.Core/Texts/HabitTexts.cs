namespace HabitTracker.Core.Texts;

/// <summary>
/// Тексты карточки (ТЗ 5.5): склонение и форматирование — чистые методы Core, покрыты тестами.
/// </summary>
public static class HabitTexts
{
    /// <summary>
    /// Русское склонение: 1 день, 2–4 дня, 5+ дней;
    /// исключения 11–14 (и 111–114 и т.п.) — всегда «дней».
    /// </summary>
    public static string DaysText(int n)
    {
        var lastDigit = n % 10;
        var lastTwoDigits = n % 100;

        if (lastDigit == 1 && lastTwoDigits != 11)
        {
            return "день";
        }

        if (lastDigit is >= 2 and <= 4 && lastTwoDigits is < 12 or > 14)
        {
            return "дня";
        }

        return "дней";
    }

    /// <summary>«Серия: N день/дня/дней» — со склонением.</summary>
    public static string StreakText(int effectiveStreak) =>
        $"Серия: {effectiveStreak} {DaysText(effectiveStreak)}";

    /// <summary>«N из M дн.» — сокращение без склонения; при серии выше цели показываем реальную серию (ТЗ 6.3).</summary>
    public static string ProgressText(int effectiveStreak, int goalDays) =>
        $"{effectiveStreak} из {goalDays} дн.";
}