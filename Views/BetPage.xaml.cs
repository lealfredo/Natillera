using Natillera.Entities;
using Natillera.ViewModels;
using Rifa.ViewModels;

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
        { 
            //await vm.CheckParticipantByPhoneAsync();
            await vm.LoadParticipants();
        }
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BetViewModel vm)
            await vm.Load();
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Participant selected)
        {
            if (BindingContext is BetViewModel vm)
            {
                vm.SelectedParticipant = selected;
            }
        }
    }
}