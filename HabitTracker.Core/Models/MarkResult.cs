namespace HabitTracker.Core.Models;


public enum MarkOutcome
{
    
    Completed,
    
    GoalReached,
    
    AlreadyDoneToday
}

public sealed record MarkResult(MarkOutcome Outcome, Habit Habit);