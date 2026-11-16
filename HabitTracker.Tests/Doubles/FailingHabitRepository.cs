using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Models;

namespace HabitTracker.Tests.Doubles;

/// <summary>
/// Репозиторий, у которого падает любая операция (ТЗ 11.2):
/// проверка преобразования технических сбоев в HabitException(Storage).
/// </summary>
public sealed class FailingHabitRepository : IHabitRepository
{
    private readonly Exception _exception;

    public FailingHabitRepository(Exception exception)
    {
        _exception = exception;
    }

    public Task<IReadOnlyList<Habit>> GetAllAsync() => Task.FromException<IReadOnlyList<Habit>>(_exception);

    public Task<Habit?> GetByIdAsync(int id) => Task.FromException<Habit?>(_exception);

    public Task<int> InsertAsync(Habit habit) => Task.FromException<int>(_exception);

    public Task UpdateAsync(Habit habit) => Task.FromException(_exception);

    public Task DeleteAsync(int id) => Task.FromException(_exception);
}