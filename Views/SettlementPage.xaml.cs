using Natillera.ViewModels;

namespace Natillera.Views;

public partial class SettlementPage : ContentPage
{
	public SettlementPage(SettlementViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}