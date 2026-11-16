using HabitTracker.ViewModels;

namespace HabitTracker.Pages;

public partial class AddHabitPage : ContentPage
{
    private readonly AddHabitViewModel _viewModel;
    private bool _navigatingHooked;

    public AddHabitPage(AddHabitViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_navigatingHooked && Shell.Current is { } shell)
        {
            shell.Navigating += OnShellNavigating;
            _navigatingHooked = true;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_navigatingHooked && Shell.Current is { } shell)
        {
            shell.Navigating -= OnShellNavigating;
            _navigatingHooked = false;
        }
    }

    private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (_viewModel.CanLeaveWithoutConfirm)
        {
            return;
        }

        if (e.Source is ShellNavigationSource.Pop or ShellNavigationSource.PopToRoot or ShellNavigationSource.Remove)
        {
            e.Cancel();
            _viewModel.CancelCommand.Execute(null);
        }
    }

    private void OnNameUnfocused(object? sender, FocusEventArgs e) => _viewModel.OnNameLostFocus();
}