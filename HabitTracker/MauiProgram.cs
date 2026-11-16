using HabitTracker.Core.Abstractions;
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

        builder.Services.AddSingleton<IClock, SystemClock>();
        builder.Services.AddSingleton<IHabitRepository, SqliteHabitRepository>();
        builder.Services.AddSingleton<IHabitService, HabitService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddTransient<HabitsViewModel>();
        builder.Services.AddTransient<AddHabitViewModel>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<HabitDetailsViewModel>();

        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<AddHabitPage>();
        builder.Services.AddTransient<HabitDetailsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}