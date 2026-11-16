using HabitTracker.Core.Texts;
using Xunit;

namespace HabitTracker.Tests.Texts;

public class HabitTextsTests
{
    // Полный список значений из ТЗ 11.1.
    [Theory]
    [InlineData(0,   "дней")]
    [InlineData(1,   "день")]
    [InlineData(2,   "дня")]
    [InlineData(4,   "дня")]
    [InlineData(5,   "дней")]
    [InlineData(11,  "дней")]
    [InlineData(12,  "дней")]
    [InlineData(14,  "дней")]
    [InlineData(21,  "день")]
    [InlineData(22,  "дня")]
    [InlineData(25,  "дней")]
    [InlineData(101, "день")]
    [InlineData(111, "дней")]
    public void DaysText_RussianPluralization(int n, string expected)
    {
        Assert.Equal(expected, HabitTexts.DaysText(n));
    }

    [Theory]
    [InlineData(0,  "Серия: 0 дней")]
    [InlineData(1,  "Серия: 1 день")]
    [InlineData(2,  "Серия: 2 дня")]
    [InlineData(5,  "Серия: 5 дней")]
    [InlineData(21, "Серия: 21 день")]
    public void StreakText_UsesPluralization(int streak, string expected)
    {
        Assert.Equal(expected, HabitTexts.StreakText(streak));
    }

    [Theory]
    [InlineData(7,  21, "7 из 21 дн.")]
    [InlineData(25, 21, "25 из 21 дн.")] // серия выше цели — реальное число (ТЗ 6.3)
    [InlineData(0,  7,  "0 из 7 дн.")]
    public void ProgressText_ShowsRealStreak(int streak, int goal, string expected)
    {
        Assert.Equal(expected, HabitTexts.ProgressText(streak, goal));
    }
}