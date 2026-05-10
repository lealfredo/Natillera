using Natillera.ViewModels;

namespace Natillera.Views;

public partial class ContributionReceiptPage : ContentPage
{
	public ContributionReceiptPage(ContributionReceiptViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}