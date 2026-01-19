using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;

namespace Natillera.ViewModels
{
    [QueryProperty(nameof(Id), "id")]
    public partial class CreateRaffleViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public CreateRaffleViewModel(INatilleraDatabase database)
        {
            _database = database;
        }

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string weekCode;

        [ObservableProperty]
        private DateTime drawDate;

        [ObservableProperty]
        private RaffleWeek raffleWeek = new();

        partial void OnDrawDateChanged(DateTime value)
        {
            WeekCode = GenerateWeekCode(value);

            RaffleWeek.WeekCode = WeekCode;
            RaffleWeek.DrawDate = value;
        }

        public async Task LoadRaffleAsync()
        {
            if (Id > 0)
            {
                Title = "Modificar Rifa Semanal";
                RaffleWeek = await _database.GetRaffleByIdAsync(Id); 
            }
            else
            { 
                RaffleWeek = new();

                Title = "Crear Rifa Semanal";

                // valores por defecto
                RaffleWeek.LotteryName = "Lotería de Medellín";
                DrawDate = GetNextFriday();
            }
        }

        [RelayCommand]
        private async Task CreateRaffleAsync()
        {
            if (string.IsNullOrWhiteSpace(RaffleWeek.PrizeDescription))
            {
                await Shell.Current.DisplayAlert("Error", "Describe qué se está rifando", "OK");
                return;
            }

            await _database.SaveRaffleWeekAsync(RaffleWeek);

            await Shell.Current.DisplayAlert("Éxito", "Rifa creada correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }

        private static string GenerateWeekCode(DateTime date)
        {
            var week = System.Globalization.ISOWeek.GetWeekOfYear(Utilities.Helpers.GetNextFriday(DateTime.Today));
            return $"{DateTime.Today.Year}-S{week}";
        }

        private static DateTime GetNextFriday()
        {
            var date = DateTime.Today;
            while (date.DayOfWeek != DayOfWeek.Friday)
                date = date.AddDays(1);

            return date;
        }
    }
}
