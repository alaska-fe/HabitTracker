using HabitTracker.Core.Models;

namespace HabitTracker.Core.Abstractions;

public interface IHabitRepository
{
    Task<IReadOnlyList<Habit>> GetAllAsync();

    Task<Habit?> GetByIdAsync(int id);
    
    Task<int> InsertAsync(Habit habit);

    Task UpdateAsync(Habit habit);

    Task DeleteAsync(int id);
}