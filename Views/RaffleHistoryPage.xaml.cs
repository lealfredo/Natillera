using Natillera.ViewModels;

namespace Natillera.Views;

public partial class RaffleHistoryPage : ContentPage
{
    private readonly RaffleHistoryViewModel _viewModel;
    public RaffleHistoryPage(RaffleHistoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = _viewModel = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RaffleHistoryViewModel vm)
            await _viewModel.LoadHistoryAsync();
    }
}