using Natillera.ViewModels;

namespace Natillera.Views;

public partial class LoanPaymentPage : ContentPage
{
    public LoanPaymentPage(LoanPaymentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}