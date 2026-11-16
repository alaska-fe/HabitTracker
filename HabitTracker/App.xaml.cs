namespace HabitTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // .NET 10: окно создаётся через CreateWindow (Application.MainPage устарел).
        return new Window(new AppShell());
    }
}