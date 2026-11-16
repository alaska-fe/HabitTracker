using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Errors;
using HabitTracker.Core.Models;
using SQLite;

namespace HabitTracker.Services;

/// <summary>
/// Репозиторий на SQLite (ТЗ 4.2, 5.7): только CRUD, без бизнес-правил.
/// Файл habits.db3 в FileSystem.AppDataDirectory. Инициализация ленивая и потокобезопасная
/// (SemaphoreSlim + двойная проверка): соединение и таблица создаются при первом обращении.
/// Технические исключения оборачиваются в HabitException(Storage) на границе слоя (ТЗ 10.1).
/// </summary>
public sealed class SqliteHabitRepository : IHabitRepository
{
    private readonly SemaphoreSlim _initGate = new(1, 1);
    private volatile SQLiteAsyncConnection? _connection;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        var existing = _connection;
        if (existing is not null)
        {
            return existing;
        }

        await _initGate.WaitAsync();
        try
        {
            if (_connection is not null)
            {
                return _connection;
            }

            var path = Path.Combine(FileSystem.AppDataDirectory, "habits.db3");
            var connection = new SQLiteAsyncConnection(path);
            await connection.CreateTableAsync<Habit>();
            _connection = connection; // публикуем только после успешного создания таблицы
            return connection;
        }
        finally
        {
            _initGate.Release();
        }
    }

    public async Task<IReadOnlyList<Habit>> GetAllAsync() =>
        await GuardAsync(async () =>
        {
            var db = await GetConnectionAsync();
            var habits = await db.Table<Habit>().ToListAsync();
            return (IReadOnlyList<Habit>)habits;
        });

    public async Task<Habit?> GetByIdAsync(int id) =>
        await GuardAsync<Habit?>(async () =>
        {
            var db = await GetConnectionAsync();
            return await db.FindAsync<Habit>(id);
        });

    public async Task<int> InsertAsync(Habit habit) =>
        await GuardAsync(async () =>
        {
            var db = await GetConnectionAsync();
            await db.InsertAsync(habit);
            return habit.Id;
        });

    public async Task UpdateAsync(Habit habit) =>
        await GuardAsync(async () =>
        {
            var db = await GetConnectionAsync();
            await db.UpdateAsync(habit);
        });

    public async Task DeleteAsync(int id) =>
        await GuardAsync(async () =>
        {
            var db = await GetConnectionAsync();
            await db.DeleteAsync<Habit>(id);
        });

    private static async Task<T> GuardAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (ex is not HabitException)
        {
            throw new HabitException(HabitErrorKind.Storage, "Сбой SQLite-хранилища привычек.", inner: ex);
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
            throw new HabitException(HabitErrorKind.Storage, "Сбой SQLite-хранилища привычек.", inner: ex);
        }
    }
}