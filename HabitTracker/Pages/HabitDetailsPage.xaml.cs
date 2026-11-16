using HabitTracker.ViewModels;

namespace HabitTracker.Pages;

public partial class HabitDetailsPage : ContentPage
{
    private readonly HabitDetailsViewModel _viewModel;

    public HabitDetailsPage(HabitDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}