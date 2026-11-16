using HabitTracker.Core.Abstractions;

namespace HabitTracker.Services;

/// <summary>Реальные часы устройства (ТЗ 4.3): локальная дата без времени.</summary>
public sealed class SystemClock : IClock
{
    public DateTime Today => DateTime.Today;
}