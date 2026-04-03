using Natillera.ViewModels;

namespace Natillera.Views;

public partial class ParticipantsPage : ContentPage
{
    public ParticipantsPage(ParticipantsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ParticipantsViewModel)?.LoadCommand.Execute(null);
    }
}