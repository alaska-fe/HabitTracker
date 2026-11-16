namespace HabitTracker.Core.Abstractions;

public interface IClock
{
   
    DateTime Today { get; }
}