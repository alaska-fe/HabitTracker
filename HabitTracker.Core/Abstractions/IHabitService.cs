using HabitTracker.Core.Models;

namespace HabitTracker.Core.Abstractions;

public interface IHabitService
{
    Task<IReadOnlyList<Habit>> GetAllAsync();

  
    Task<Habit> AddAsync(NewHabit request);

   
    Task<MarkResult> MarkCompletedTodayAsync(int habitId);

  
    Task DeleteAsync(int habitId);

    Task<HabitStatistics> GetStatisticsAsync();
}