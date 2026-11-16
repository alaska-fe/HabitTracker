using HabitTracker.Pages;

namespace HabitTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("add-habit", typeof(AddHabitPage));
        Routing.RegisterRoute("habit-details", typeof(HabitDetailsPage));
    }
}