using Natillera.ViewModels;

namespace Natillera.Views;

public partial class BackupPage : ContentPage
{
    private readonly BackupViewModel _viewModel;
    public BackupPage(BackupViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BackupViewModel vm)
            await _viewModel.GetSetting();
    }
}