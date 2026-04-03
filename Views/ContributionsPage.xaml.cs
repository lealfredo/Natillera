using Rifa.ViewModels;

namespace Natillera.Views;

public partial class ContributionsPage : ContentPage
{
    public ContributionsPage(ContributionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ContributionsViewModel)?.LoadCommand.Execute(null);
    }
}