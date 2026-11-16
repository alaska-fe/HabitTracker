using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using HabitTracker.Core.Rules;
using HabitTracker.Core.Statistics;
using HabitTracker.Core.Validation;

namespace HabitTracker.Core.Services;

/// <summary>
/// Бизнес-слой (ТЗ 4.2, 6): валидация, лимиты, правила серии, статистика.
/// Не знает про UI и платформу; дата — только через IClock.
/// Непредвиденные технические сбои оборачивает в HabitException(Storage),
/// типизированные HabitException пропускает без изменений.
/// </summary>
public sealed class HabitService : IHabitService
{
    /// <summary>Максимум привычек (ТЗ 6.4).</summary>
    public const int MaxHabits = 30;

    private readonly IHabitRepository _repository;
    private readonly IClock _clock;

    public HabitService(IHabitRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task<IReadOnlyList<Habit>> GetAllAsync() =>
        GuardAsync(() => _repository.GetAllAsync());

    public Task<Habit> AddAsync(NewHabit request) => GuardAsync(async () =>
    {
        var validated = HabitValidator.Validate(request);
        var existing = await _repository.GetAllAsync();

        if (existing.Count >= MaxHabits)
        {
            throw new HabitException(HabitErrorKind.LimitReached,
                $"Достигнут лимит привычек: {MaxHabits}.");
        }

        if (existing.Any(habit => string.Equals(habit.Name, validated.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new HabitException(HabitErrorKind.DuplicateName,
                $"Привычка с названием «{validated.Name}» уже существует.");
        }

        var habit = new Habit
        {
            Name = validated.Name,
            Description = validated.Description,
            IconKey = validated.IconKey,
            ColorKey = validated.ColorKey,
            GoalDays = validated.GoalDays,
            CurrentStreak = 0,
            BestStreak = 0,
            LastCompletedDate = null,
            CreatedAt = _clock.Today
        };

        habit.Id = await _repository.InsertAsync(habit);
        return habit;
    });

    public Task<MarkResult> MarkCompletedTodayAsync(int habitId) => GuardAsync(async () =>
    {
        var habit = await GetByIdOrThrowAsync(habitId);
        var result = HabitRules.MarkCompletedToday(habit, _clock.Today);

        // R2: сегодня уже отмечено — в БД ничего не пишем.
        if (result.Outcome != MarkOutcome.AlreadyDoneToday)
        {
            await _repository.UpdateAsync(habit);
        }

        return result;
    });

    public Task DeleteAsync(int habitId) => GuardAsync(async () =>
    {
        _ = await GetByIdOrThrowAsync(habitId);
        await _repository.DeleteAsync(habitId);
    });

    public Task<HabitStatistics> GetStatisticsAsync() => GuardAsync(async () =>
    {
        var habits = await _repository.GetAllAsync();
        return HabitStatisticsCalculator.Calculate(habits, _clock.Today);
    });

    private async Task<Habit> GetByIdOrThrowAsync(int habitId) =>
        await _repository.GetByIdAsync(habitId)
        ?? throw new HabitException(HabitErrorKind.NotFound, $"Привычка Id={habitId} не найдена.");

    private static async Task<T> GuardAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (ex is not HabitException)
        {
            throw new HabitException(HabitErrorKind.Storage, "Сбой хранилища привычек.", inner: ex);
        }
    }

    private static async Task GuardAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex) when (ex is not HabitException)
        {
            throw new HabitException(HabitErrorKind.Storage, "Сбой хранилища привычек.", inner: ex);
        }
    }
}