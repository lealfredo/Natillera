using Natillera.ViewModels;

namespace Natillera.Views;

public partial class CreateRafflePage : ContentPage
{
    private readonly CreateRaffleViewModel _vm;
    public CreateRafflePage(CreateRaffleViewModel vm)
    {
        InitializeComponent();

        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CreateRaffleViewModel vm)
            await _vm.LoadRaffleAsync();
    }
}