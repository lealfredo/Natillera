using Natillera.ViewModels;

namespace Natillera.Views;

public partial class CreateRafflePage : ContentPage
{
    public CreateRafflePage(CreateRaffleViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}