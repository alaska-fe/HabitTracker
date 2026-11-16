using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Services;
using HabitTracker.Pages;
using HabitTracker.Services;
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

        // Страницы (ТЗ 4.4): Transient. ViewModel добавим на шагах 7–9.
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<AddHabitPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

#if DEBUG
        // ВРЕМЕННЫЙ зонд (шаг 5): проверяем DI и SQLite. Удалим на шаге 7.
        _ = ProbeDatabaseAsync(app.Services);
#endif

        return app;
    }

#if DEBUG
    private static async Task ProbeDatabaseAsync(IServiceProvider services)
    {
        try
        {
            var service = services.GetRequiredService<IHabitService>();
            var habits = await service.GetAllAsync();
            Console.WriteLine($"[HabitTrackerProbe] OK: habits.db3 создан, привычек: {habits.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HabitTrackerProbe] FAIL: {ex}");
        }
    }
#endif
}