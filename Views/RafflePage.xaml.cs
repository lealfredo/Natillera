using Natillera.ViewModels;

namespace Natillera.Views;

public partial class RafflePage : ContentPage
{
    private readonly RaffleViewModel _viewModel;

    public RafflePage(RaffleViewModel viewModel)
    {
        InitializeComponent();

        viewModel.ExportNumbersRequested += OnExportNumbersRequested;

        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RaffleViewModel vm)
        {
            await _viewModel.LoadSettingAsync();
            await _viewModel.LoadCurrentRaffleAsync();
        }
    }

    private async void OnExportNumbersRequested(object? sender, EventArgs e)
    {
        var image = await ExportLayout.CaptureAsync();
        var stream = await image.OpenReadAsync();

        var path = Path.Combine(FileSystem.CacheDirectory, "numeros-rifa.png");

        using var file = File.OpenWrite(path);
        await stream.CopyToAsync(file);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Números de la rifa",
            File = new ShareFile(path)
        });
    }

    private async void OnRegisterBetClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(App.Services.GetService<CreateRafflePage>());
    }
}