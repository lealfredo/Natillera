using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Entities;
using Natillera.Services;
using Natillera.Views;
using System.Collections.ObjectModel;

namespace Natillera.ViewModels
{
    public partial class RaffleHistoryViewModel : BaseViewModel
    {
        private readonly IRaffleService _raffleService;

        public RaffleHistoryViewModel(IRaffleService raffleService)
        {
            Title = "Historial de Rifas";
            _raffleService = raffleService;
        }

        [ObservableProperty]
        private ObservableCollection<RaffleWeek> raffles = new();

        [ObservableProperty]
        private bool isBusy;

        public async Task LoadHistoryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            Raffles.Clear();

            var list = await _raffleService.GetClosedRafflesAsync();

            foreach (var raffle in list)
                Raffles.Add(raffle);

            if(Raffles.Count > 0)
                Exist = true;
            else
                Exist = false;

            IsBusy = false;
        }

        [RelayCommand]
        private async Task OpenRaffleAsync(RaffleWeek raffle)
        {
            if (raffle == null) return;

            await Shell.Current.GoToAsync(
                nameof(RaffleWinnersPage),
                new Dictionary<string, object>
                {
                { "RaffleId", raffle.Id }
                });
        }


        [ObservableProperty]
        private bool exist;
        public bool NoExist => !Exist;

        partial void OnExistChanged(bool value)
        {
            OnPropertyChanged(nameof(NoExist));
        }
    }
}
