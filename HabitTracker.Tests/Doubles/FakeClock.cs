using HabitTracker.Core.Abstractions;

namespace HabitTracker.Tests.Doubles;

/// <summary>
/// Управляемые часы (ТЗ 11.2): дата задаётся и меняется тестом, время всегда обнулено.
/// </summary>
public sealed class FakeClock : IClock
{
    public FakeClock(DateTime today)
    {
        Today = today.Date;
    }

    public DateTime Today { get; private set; }

    public void SetToday(DateTime today) => Today = today.Date;

    public void AdvanceDays(int days) => Today = Today.AddDays(days);
}