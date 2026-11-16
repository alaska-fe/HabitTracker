using System.Diagnostics.CodeAnalysis;

namespace HabitTracker.Core.Models;

/// <summary>Запрос на создание привычки (ТЗ 5.3). Иммутабельный DTO: сервис валидирует и нормализует его.</summary>
/// <remarks>DTO без логики: сгенерированные record-члены исключены из замера покрытия.</remarks>
[ExcludeFromCodeCoverage]
public sealed record NewHabit(
    string Name,
    string Description,
    string IconKey,
    string ColorKey,
    int GoalDays);