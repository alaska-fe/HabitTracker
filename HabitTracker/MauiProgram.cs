using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Models;
using HabitTracker.Core.Services;
using HabitTracker.Pages;
using HabitTracker.Services;
using HabitTracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HabitTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Службы (ТЗ 4.4): Singleton
        builder.Services.AddSingleton<IClock, SystemClock>();
        builder.Services.AddSingleton<IHabitRepository, SqliteHabitRepository>();
        builder.Services.AddSingleton<IHabitService, HabitService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // ViewModel (ТЗ 4.4): Transient
        builder.Services.AddTransient<HabitsViewModel>();

        // Страницы (ТЗ 4.4): Transient
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<AddHabitPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

#if DEBUG
        // ВРЕМЕННЫЙ посев тестовых данных: проверить главный экран до формы добавления.
        // Будет удалён на шаге 8.
        _ = SeedAsync(app.Services);
#endif

        return app;
    }

#if DEBUG
    private static async Task SeedAsync(IServiceProvider services)
    {
        try
        {
            var repository = services.GetRequiredService<IHabitRepository>();
            if ((await repository.GetAllAsync()).Count > 0)
            {
                return;
            }

            var today = DateTime.Today;
            await repository.InsertAsync(new Habit
            {
                Name = "Пить воду", IconKey = "water", ColorKey = "blue", GoalDays = 21,
                CurrentStreak = 7, BestStreak = 7, LastCompletedDate = today, CreatedAt = today
            });
            await repository.InsertAsync(new Habit
            {
                Name = "Читать", IconKey = "book", ColorKey = "green", GoalDays = 21,
                CurrentStreak = 5, BestStreak = 9, LastCompletedDate = today.AddDays(-2), CreatedAt = today
            });
            await repository.InsertAsync(new Habit
            {
                Name = "Медитация", IconKey = "yoga", ColorKey = "purple", GoalDays = 21,
                CurrentStreak = 3, BestStreak = 3, LastCompletedDate = today.AddDays(-1), CreatedAt = today
            });
            Console.WriteLine("[HabitTrackerSeed] Добавлены 3 тестовые привычки");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HabitTrackerSeed] FAIL: {ex}");
        }
    }
#endif
}