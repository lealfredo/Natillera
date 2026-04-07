using Natillera.Entities;
using Natillera.ViewModels;
using Rifa.ViewModels;

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

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Participant selected)
        {
            var vm = BindingContext as LoansViewModel;
            vm.SelectedParticipant = selected;
            vm.LoadCommand.Execute(null);
        }
    }
}