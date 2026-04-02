using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Entities;
using Natillera.Services;
using Natillera.ViewModels;
using Natillera.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public partial class RafflesViewModel : BaseViewModel
    {
        private readonly IRaffleService _raffleService;
        public RafflesViewModel(IRaffleService raffleService)
        {

            Title = "Rifa Semanal";
            ButtonText = "Nueva rifa";
            _raffleService = raffleService;
        }

        [ObservableProperty]
        private string buttonText;

        [ObservableProperty]
        private ObservableCollection<RaffleWeek> raffles = new();

        [RelayCommand]
        public async Task GoToCreateRaffleAsync()
        {
            await Shell.Current.GoToAsync(
                $"{nameof(CreateRafflePage)}?id={0}");
        }

        public async Task LoadHistoryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            Raffles.Clear();

            var list = await _raffleService.GetOpenRafflesAsync();

            foreach (var raffle in list)
                Raffles.Add(raffle);

            if (Raffles.Count > 0)
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
                nameof(RafflePage),
                new Dictionary<string, object>
                {
                { "RaffleId", raffle.Id }
                });
        }

        [RelayCommand]
        private async Task OpenRaffleWinnerAsync(RaffleWeek raffle)
        {
            if (raffle == null) return;

            await Shell.Current.GoToAsync(
                nameof(RaffleWinnersPage),
                new Dictionary<string, object>
                {
                { "RaffleId", raffle.Id }
                });
        }

        public ICommand DeleteRaffleCommand => new Command<RaffleWeek>(async (raffle) =>
        {
            if (raffle == null) return;

            // Opcional: confirmación
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Eliminar",
                "¿Seguro que deseas eliminar este sorteo?",
                "Sí",
                "No");

            if (!confirm) return;

            var resul = await _raffleService.DeleteRaffleWeek(raffle.Id);

            if (resul > 0)
                Raffles.Remove(raffle);
        });

        public ICommand ShowOptionsCommand => new Command<RaffleWeek>(async (raffle) =>
        {
            if (raffle == null) return;

            string action = await App.Current.MainPage.DisplayActionSheet(
                "Opciones",
                "Cancelar",
                null,
                "Ver detalles",
                "Ganadores"
            );

            switch (action)
            {
                case "Ver detalles":
                    await OpenRaffleAsync(raffle);
                    break;
                case "Ganadores":
                    await OpenRaffleWinnerAsync(raffle);
                    break;
            }
        });

        [ObservableProperty]
        private bool exist;
        public bool NoExist => !Exist;

        partial void OnExistChanged(bool value)
        {
            OnPropertyChanged(nameof(NoExist));
        }
    }
}
