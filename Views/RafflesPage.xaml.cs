using Natillera.ViewModels;

namespace Natillera.Views;

public partial class RafflesPage : ContentPage
{
    private readonly RafflesViewModel _viewModel;
	public RafflesPage(RafflesViewModel _vm)
	{
		InitializeComponent();

		BindingContext = _viewModel = _vm;
	}



    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RafflesViewModel vm)
            await _viewModel.LoadHistoryAsync();
    }
}