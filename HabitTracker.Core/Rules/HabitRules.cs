using System.Globalization;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;

namespace HabitTracker.Core.Rules;

/// <summary>
/// Чистые бизнес-правила привычек (ТЗ 6). Все методы детерминированы:
/// результат зависит только от переданных данных и даты; никаких обращений к БД, UI
/// и системному времени. Дата приходит извне: в приложении — через IClock, в тестах — фиксированная.
/// </summary>
public static class HabitRules
{
    /// <summary>
    /// Отметка «выполнено сегодня» (правила R1–R7, ТЗ 6.1).
    /// Изменяет переданную сущность (кроме R2 и R5 — там данные не меняются) и возвращает исход.
    /// Сохранение в БД — обязанность сервиса, не правил.
    /// </summary>
    public static MarkResult MarkCompletedToday(Habit habit, DateTime today)
    {
        var day = today.Date;
        var last = habit.LastCompletedDate?.Date;

        // R5: дата устройства раньше последней отметки — данные не меняем.
        if (last.HasValue && last.Value > day)
        {
            throw new HabitException(
                HabitErrorKind.ClockMovedBack,
                FormattableString.Invariant(
                    $"Дата устройства {day:yyyy-MM-dd} раньше последней отметки {last.Value:yyyy-MM-dd}."));
        }

        // R2: сегодня уже отмечено — не ошибка, ничего не меняем.
        if (last.HasValue && last.Value == day)
        {
            return new MarkResult(MarkOutcome.AlreadyDoneToday, habit);
        }

        // R3: отметка была вчера (календарно!) — серия продолжается.
        // R1 (отметок не было) и R4 (пропущен день) — серия начинается с 1.
        if (last.HasValue && last.Value == day.AddDays(-1))
        {
            habit.CurrentStreak += 1;
        }
        else
        {
            habit.CurrentStreak = 1;
        }

        habit.LastCompletedDate = day;

        // R6: рекорд за всё время.
        if (habit.CurrentStreak > habit.BestStreak)
        {
            habit.BestStreak = habit.CurrentStreak;
        }

        // R7: GoalReached — ровно в день, когда серия стала равна цели.
        var outcome = habit.CurrentStreak == habit.GoalDays
            ? MarkOutcome.GoalReached
            : MarkOutcome.Completed;

        return new MarkResult(outcome, habit);
    }

    /// <summary>
    /// Эффективная серия на сегодня (ТЗ 6.2). В БД серия не обнуляется при пропуске:
    /// приложение может не запускаться несколько дней; сброс произойдёт при следующей отметке (R4).
    /// </summary>
    public static int GetEffectiveStreak(Habit habit, DateTime today)
    {
        var day = today.Date;
        var last = habit.LastCompletedDate?.Date;

        if (last is null)
        {
            return 0;
        }

        // Дата устройства некорректна (раньше отметки) — показываем сохранённую серию.
        if (last.Value > day)
        {
            return habit.CurrentStreak;
        }

        // Отмечено сегодня или вчера — серия жива.
        if (last.Value >= day.AddDays(-1))
        {
            return habit.CurrentStreak;
        }

        // Пропущен хотя бы один день — на экране серия 0.
        return 0;
    }

    /// <summary>Серия была, но прервана (ТЗ 6.2) — состояние карточки N05.</summary>
    public static bool IsStreakBroken(Habit habit, DateTime today)
    {
        var last = habit.LastCompletedDate?.Date;
        return last.HasValue && last.Value < today.Date.AddDays(-1);
    }

    /// <summary>Отмечена ли привычка сегодня.</summary>
    public static bool IsDoneToday(Habit habit, DateTime today) =>
        habit.LastCompletedDate?.Date == today.Date;

    /// <summary>
    /// Прогресс до цели 0.0…1.0 для ProgressBar (ТЗ 6.3).
    /// Деление в double, сверху ограничено единицей.
    /// </summary>
    public static double GetProgress(Habit habit, DateTime today)
    {
        // Защита от повреждённых данных: цель должна быть положительной (ТЗ 10.3).
        if (habit.GoalDays <= 0)
        {
            return 0.0;
        }

        var effectiveStreak = GetEffectiveStreak(habit, today);
        return Math.Min((double)effectiveStreak / habit.GoalDays, 1.0);
    }

    /// <summary>Цель достигнута, если эффективная серия не меньше цели (ТЗ 6.3).</summary>
    public static bool IsGoalReached(Habit habit, DateTime today) =>
        habit.GoalDays > 0 && GetEffectiveStreak(habit, today) >= habit.GoalDays;

    /// <summary>Процент для текста: до целого, «школьное» округление (0.125 → 13 %).</summary>
    public static int GetProgressPercent(double progress) =>
        (int)Math.Round(progress * 100, MidpointRounding.AwayFromZero);
}