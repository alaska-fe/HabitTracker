namespace HabitTracker.Core.Models;


public sealed record NewHabit(
    string Name,
    string Description,
    string IconKey,
    string ColorKey,
    int GoalDays);