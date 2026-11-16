using HabitTracker.Core.Catalogs;
using SQLite;

namespace HabitTracker.Core.Models;

[Table("Habits")]
public class Habit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string IconKey { get; set; } = HabitIcons.Default;
    
    public string ColorKey { get; set; } = HabitColors.Default;
    
    public int GoalDays { get; set; } = 21;
    
    public int CurrentStreak { get; set; }
    
    public int BestStreak { get; set; }
    
    public DateTime? LastCompletedDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
}