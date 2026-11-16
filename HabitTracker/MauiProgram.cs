using HabitTracker.Core.Abstractions;
using HabitTracker.Core.Services;
using HabitTracker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Handlers;

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

        // ViewModel и страницы (Transient) регистрируем на шагах 7–10 по мере появления.

        ConfigureFieldPaddingHandlers();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

#if DEBUG
        // ВРЕМЕННЫЙ диагностический зонд: проверяем DI и нативный SQLite на устройстве
        // до появления экранов. Будет удалён на шаге 7 (загрузку возьмёт HabitsViewModel).
        _ = ProbeDatabaseAsync(app.Services);
#endif

        return app;
    }

    // У Entry/Editor в .NET MAUI нет XAML-свойства Padding.
    // Добавляем внутренний отступ через нативные обработчики (Android).
    private static void ConfigureFieldPaddingHandlers()
    {
#if ANDROID
        EntryHandler.Mapper.AppendToMapping("FieldPadding", (handler, _) =>
            ApplyAndroidPadding(handler.PlatformView));

        EditorHandler.Mapper.AppendToMapping("FieldPadding", (handler, _) =>
            ApplyAndroidPadding(handler.PlatformView));
#endif
    }

#if ANDROID
    private static void ApplyAndroidPadding(Android.Widget.EditText? native)
    {
        if (native is null) return;

        var p = GetFieldPaddingOrDefault();
        var density = (float)DeviceDisplay.MainDisplayInfo.Density;

        native.SetPadding(
            (int)Math.Round(p.Left * density),
            (int)Math.Round(p.Top * density),
            (int)Math.Round(p.Right * density),
            (int)Math.Round(p.Bottom * density));
    }
#endif

    private static Thickness GetFieldPaddingOrDefault()
    {
        // Пытаемся прочитать значение из ресурсов (Dimens.xaml).
        if (Application.Current?.Resources.TryGetValue("FieldPadding", out var value) == true
            && value is Thickness t)
        {
            return t;
        }

        // Fallback на случай, если ресурс ещё не загружен (первый рендер).
        return new Thickness(12, 8);
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