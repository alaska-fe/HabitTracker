using HabitTracker.ViewModels;

namespace HabitTracker.Pages;

public partial class HabitsPage : ContentPage
{
    private readonly HabitsViewModel _viewModel;
    private bool _resumedHooked;

    public HabitsPage(HabitsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Перезагрузка при каждом появлении: возврат с формы, смена суток (ТЗ 7.3).
        _viewModel.LoadCommand.Execute(null);

        if (!_resumedHooked && Application.Current?.Windows.FirstOrDefault() is { } window)
        {
            window.Resumed += OnWindowResumed;
            _resumedHooked = true;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_resumedHooked && Application.Current?.Windows.FirstOrDefault() is { } window)
        {
            window.Resumed -= OnWindowResumed;
            _resumedHooked = false;
        }
    }

    // Возврат из фона: пересчитать состояние «сегодня» (ТЗ 7.3, A8).
    private void OnWindowResumed(object? sender, EventArgs e) => _viewModel.LoadCommand.Execute(null);
}