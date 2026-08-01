using Natillera.Entities;
using Natillera.Models;
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

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LoansViewModel vm)
        { 
            vm.HasLoaded = false;
            await vm.Init();
            vm.LoadCommand.Execute(null);
        }
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ParticipantFilter selected)
        {
            var vm = BindingContext as LoansViewModel;

            vm.SelectedFilter = selected;

            // SOLO si es participante real
            if (selected.Participant != null)
                vm.SelectedParticipant = selected.Participant;
            else
                vm.SelectedParticipant = null;

            // 🔥 cerrar dropdown
            vm.FilteredParticipants.Clear();
        }
    }
}