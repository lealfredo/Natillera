using Natillera.ViewModels;

namespace Natillera.Views;

public partial class BetPage : ContentPage
{
    public BetPage(BetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnPhoneUnfocused(object sender, FocusEventArgs e)
    {
        if (BindingContext is BetViewModel vm)
            await vm.CheckParticipantByPhoneAsync();
    }
}