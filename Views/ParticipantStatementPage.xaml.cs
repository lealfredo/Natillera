using Natillera.ViewModels;

namespace Natillera.Views;

public partial class ParticipantStatementPage : ContentPage
{
	public ParticipantStatementPage(ParticipantStatementViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (BindingContext is ParticipantStatementViewModel vm)
                await vm.Load();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR OnAppearing: {ex.Message}");
        }
    }
}