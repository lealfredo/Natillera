using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;

namespace Natillera.ViewModels
{
    public partial class CreateRaffleViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public CreateRaffleViewModel(INatilleraDatabase database)
        {
            _database = database;

            Title = "Crear Rifa Semanal";

            // valores por defecto
            LotteryName = "Lotería de Medellín";
            DrawDate = GetNextFriday();
            WeekCode = GenerateWeekCode(DrawDate);
        }

        [ObservableProperty]
        private string weekCode;

        [ObservableProperty]
        private string lotteryName;

        [ObservableProperty]
        private DateTime drawDate;

        [ObservableProperty]
        private string prizeDescription;

        [ObservableProperty]
        private decimal firstTwoPrize;

        [ObservableProperty]
        private decimal betPrize;

        [ObservableProperty]
        private decimal middleTwoPrize;

        [ObservableProperty]
        private decimal lastTwoPrize;

        partial void OnDrawDateChanged(DateTime value)
        {
            WeekCode = GenerateWeekCode(value);
        }

        [RelayCommand]
        private async Task CreateRaffleAsync()
        {
            if (string.IsNullOrWhiteSpace(PrizeDescription))
            {
                await Shell.Current.DisplayAlert("Error", "Describe qué se está rifando", "OK");
                return;
            }

            var raffle = new RaffleWeek
            {
                WeekCode = WeekCode,
                DrawDate = DrawDate,
                PrizeDescription = PrizeDescription,
                FirstTwoPrize = FirstTwoPrize,
                MiddleTwoPrize = MiddleTwoPrize,
                LastTwoPrize = LastTwoPrize,
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
                LotteryName = LotteryName,
                BetPrize = BetPrize
            };

            await _database.SaveRaffleWeekAsync(raffle);

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
