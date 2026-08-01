using Natillera.ViewModels;

namespace Natillera.Views;

public partial class SettlementResultPage : ContentPage
{
    public SettlementResultPage(SettlementResultViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.ExportView = ExportLayout;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as SettlementResultViewModel)?.LoadCommand.Execute(null);
    }
}