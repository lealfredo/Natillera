using Natillera.ViewModels;

namespace Natillera.Views;

public partial class LoansPage : ContentPage
{
	public LoansPage(LoansViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as LoansViewModel)?.LoadCommand.Execute(null);
    }
}