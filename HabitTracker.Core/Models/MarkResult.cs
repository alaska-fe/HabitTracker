using System.Diagnostics.CodeAnalysis;

namespace HabitTracker.Core.Models;

/// <summary>Исход отметки «выполнено сегодня» (ТЗ 5.4).</summary>
public enum MarkOutcome
{
    /// <summary>Серия увеличена.</summary>
    Completed,

    /// <summary>Серия увеличена и впервые достигла цели (ровно один раз, правило R7).</summary>
    GoalReached,

    /// <summary>Сегодня уже отмечено, ничего не изменилось. Не ошибка (правило R2).</summary>
    AlreadyDoneToday
}

/// <summary>
/// Результат отметки: исход + обновлённая сущность.
/// Habit возвращается, чтобы ViewModel обновил ОДНУ карточку без перезагрузки списка (ТЗ 16).
/// </summary>
/// <remarks>DTO без логики: сгенерированные record-члены исключены из замера покрытия.</remarks>
[ExcludeFromCodeCoverage]
public sealed record MarkResult(MarkOutcome Outcome, Habit Habit);