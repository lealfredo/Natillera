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

            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        NumbersGrid.Children.Clear();
        NumbersGrid.RowDefinitions.Clear();
        NumbersGrid.ColumnDefinitions.Clear();

        int columns = 10;
        var numbers = (BindingContext as RaffleViewModel)?.Numbers;

        if (numbers == null) return;

        for (int i = 0; i < numbers.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            if (NumbersGrid.RowDefinitions.Count <= row)
                NumbersGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            if (NumbersGrid.ColumnDefinitions.Count < columns)
            {
                for (int c = NumbersGrid.ColumnDefinitions.Count; c < columns; c++)
                    NumbersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            var item = numbers[i];

            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

            // 10 columnas
            double itemSize = (screenWidth / 10) - 10; // margen incluido

            var btn = new Button
            {
                Text = item.Number.ToString(),
                WidthRequest = itemSize,
                HeightRequest = itemSize,
                FontSize = 12,
                Margin = 2,
                TextColor = Colors.White,
                BackgroundColor = item.IsTaken ? Colors.Red : Colors.Green,
                Command = new Command(() =>
                {
                    (BindingContext as RaffleViewModel)?.SelectNumberCommand.Execute(item);
                })
            };

            Grid.SetRow(btn, row);
            Grid.SetColumn(btn, col);

            NumbersGrid.Children.Add(btn);
        }
    }

    private async void OnExportNumbersRequested(object? sender, EventArgs e)
    {
        try
        {
            var vm = BindingContext as RaffleViewModel;
            if (vm == null || vm.Numbers == null || vm.Numbers.Count == 0)
                return;

            var numbers = vm.Numbers.ToList();

            // Tamaño de pantalla en unidades reales
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

            int columns = 10;
            int rows = (int)Math.Ceiling((double)numbers.Count / columns);

            double itemWidth = screenWidth / columns;
            double itemHeight = (screenHeight / 2) / rows;

            // Contenedor principal
            var exportView = new Grid
            {
                BackgroundColor = Colors.White,
                WidthRequest = screenWidth,
                HeightRequest = screenHeight
            };

            // Grid interno
            var grid = new Grid();

            for (int i = 0; i < rows; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = itemHeight });

            for (int i = 0; i < columns; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = itemWidth });

            // Pintar números
            for (int i = 0; i < numbers.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                var item = numbers[i];

                var border = new Border
                {
                    Stroke = Colors.Black, // borde
                    StrokeThickness = 0.5,
                    BackgroundColor = item.IsTaken ? Colors.Red : Color.FromArgb("#E8F5E9"),
                    Padding = 2
                };

                var label = new Label
                {
                    Text = item.Number,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 10,
                    TextColor = item.IsTaken ? Colors.White : Colors.Black
                };

                border.Content = label;

                grid.Add(border, col, row);
            }

            exportView.Add(grid);

            // IMPORTANTE: agregar temporalmente a la UI
            MainLayout.Children.Add(exportView);

            // Esperar renderizado
            await Task.Delay(150);

            // Capturar
            var image = await exportView.CaptureAsync();
            var stream = await image.OpenReadAsync();

            // Nombre dinámico
            var fileName = $"numeros-rifa-{DateTime.Now:yyyyMMdd-HHmmss}.png";
            var path = Path.Combine(FileSystem.CacheDirectory, fileName);

            using var file = File.OpenWrite(path);
            await stream.CopyToAsync(file);

            // Limpiar
            MainLayout.Children.Remove(exportView);

            // Compartir
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Números de la rifa",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnRegisterBetClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(App.Services.GetService<CreateRafflePage>());
    }
}