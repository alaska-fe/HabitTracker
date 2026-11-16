using HabitTracker.Pages;

namespace HabitTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Маршрут страницы добавления (ТЗ 8.3). Маршрут habit-details добавим на шаге 10.
        Routing.RegisterRoute("add-habit", typeof(AddHabitPage));
    }
}