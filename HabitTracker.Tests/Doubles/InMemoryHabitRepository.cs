using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Models;

namespace HabitTracker.Tests.Doubles;

/// <summary>
/// Репозиторий в памяти (ТЗ 11.2). Клонирует сущности на входе и выходе — как настоящая БД:
/// изменение объекта без UpdateAsync не должно попадать в «хранилище».
/// Методы virtual, чтобы в тестах можно было имитировать сбой конкретной операции.
/// </summary>
public class InMemoryHabitRepository : IHabitRepository
{
    private readonly List<Habit> _habits = new();
    private int _nextId = 1;

    public int InsertCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    /// <summary>Наполнение хранилища тестовыми данными (arrange).</summary>
    public void Seed(Habit habit)
    {
        var copy = Clone(habit);
        if (copy.Id == 0)
        {
            copy.Id = _nextId++;
        }
        else
        {
            _nextId = Math.Max(_nextId, copy.Id + 1);
        }
        _habits.Add(copy);
    }

    public virtual Task<IReadOnlyList<Habit>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Habit>>(_habits.Select(Clone).ToList());

    public virtual Task<Habit?> GetByIdAsync(int id)
    {
        var found = _habits.FirstOrDefault(habit => habit.Id == id);
        Habit? result = found is null ? null : Clone(found);
        return Task.FromResult(result);
    }

    public virtual Task<int> InsertAsync(Habit habit)
    {
        InsertCalls++;
        var copy = Clone(habit);
        copy.Id = _nextId++;
        _habits.Add(copy);
        return Task.FromResult(copy.Id);
    }

    public virtual Task UpdateAsync(Habit habit)
    {
        UpdateCalls++;
        var index = _habits.FindIndex(stored => stored.Id == habit.Id);
        if (index < 0)
        {
            throw new InvalidOperationException($"Привычка Id={habit.Id} не найдена в хранилище.");
        }
        _habits[index] = Clone(habit);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(int id)
    {
        DeleteCalls++;
        _habits.RemoveAll(habit => habit.Id == id);
        return Task.CompletedTask;
    }

    private static Habit Clone(Habit habit) => new()
    {
        Id = habit.Id,
        Name = habit.Name,
        Description = habit.Description,
        IconKey = habit.IconKey,
        ColorKey = habit.ColorKey,
        GoalDays = habit.GoalDays,
        CurrentStreak = habit.CurrentStreak,
        BestStreak = habit.BestStreak,
        LastCompletedDate = habit.LastCompletedDate,
        CreatedAt = habit.CreatedAt
    };
}