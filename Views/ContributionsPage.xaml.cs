using Natillera.Entities;
using Natillera.ViewModels;
using Rifa.ViewModels;

namespace Natillera.Views;

public partial class ContributionsPage : ContentPage
{
    public ContributionsPage(ContributionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ContributionsViewModel vm)
        {
            vm.LoadCommand.Execute(null);
            //await vm.LoadParticipants();
        }
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Participant selected)
        {
            var vm = BindingContext as ContributionsViewModel;
            vm.SelectedParticipant = selected;
            vm.LoadCommand.Execute(null);
        }
    }
}