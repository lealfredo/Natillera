using Natillera.ViewModels;

namespace Natillera.Views;

public partial class ParticipantStatementPage : ContentPage
{
	public ParticipantStatementPage(ParticipantStatementViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ParticipantStatementViewModel)?.Load();
    }
}