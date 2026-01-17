using Natillera.ViewModels;

namespace Natillera.Views;

public partial class RaffleWinnersPage : ContentPage
{
    private readonly RaffleWinnerViewModel _viewModel;
    public RaffleWinnersPage(RaffleWinnerViewModel vm)
	{
		InitializeComponent();

		BindingContext = _viewModel = vm;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RaffleWinnerViewModel vm)
            await _viewModel.LoadWinnersAsync();
    }
}