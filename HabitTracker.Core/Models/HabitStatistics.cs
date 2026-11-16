namespace HabitTracker.Core.Models;


public sealed record HabitStatistics(
    int TotalHabits,
    int DoneToday,                
    int GoalsReached,              
    double AverageProgressPercent, 
    int LongestCurrentStreak,       
    int LongestEverStreak,          
    IReadOnlyList<HabitProgressRow> Rows);

public sealed record HabitProgressRow(
    int Id, string Name, string Glyph, string ColorHex,
    int EffectiveStreak, int GoalDays, double Progress, bool IsDoneToday, bool IsGoalReached);