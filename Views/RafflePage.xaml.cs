using Natillera.ViewModels;

namespace Natillera.Views;

public partial class RafflePage : ContentPage
{
    private readonly RaffleViewModel _viewModel;

    public RafflePage(RaffleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RaffleViewModel vm)
            await _viewModel.LoadCurrentRaffleAsync();
    }

    private async void OnRegisterBetClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(App.Services.GetService<CreateRafflePage>());
    }
}