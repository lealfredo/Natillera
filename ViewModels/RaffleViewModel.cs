using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Natillera.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public partial class RaffleViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;
        private readonly IWhatsAppService _whatsAppService;
        private Setting _setting;
        public ICommand SelectNumberCommand { get; }
        public ObservableCollection<BetNumber> Numbers { get; } = new();

        [ObservableProperty]
        private RaffleWeek currentRaffle;

        [ObservableProperty]
        private bool isClosed;

        [ObservableProperty]
        private string buttonText;

        public bool IsOpen => !IsClosed;

        public RaffleViewModel(INatilleraDatabase database, IWhatsAppService whatsAppService)
        {
            _database = database;
            Title = "Rifa Semanal";

            SelectNumberCommand = new Command<BetNumber>(OnSelectNumber);
            _whatsAppService = whatsAppService;
        }
        public async Task LoadSettingAsync()
        {
            _setting = await _database.GetSettingAsync();
        }

        public event EventHandler? ExportNumbersRequested;

        [RelayCommand]
        public async Task ExportNumbers()
        {
            ExportNumbersRequested?.Invoke(this, EventArgs.Empty);
        }

        public async Task LoadNumbersAsync()
        {
            Numbers.Clear();

            // Números ya apostados desde SQLite
            var list = await _database.GetBetNumbersAsync(CurrentRaffle.Id);

            foreach (var n in list)
                Numbers.Add(n);
        }

        partial void OnIsClosedChanged(bool value)
        {
            OnPropertyChanged(nameof(IsOpen));
        }

        private async void OnSelectNumber(BetNumber bet)
        {
            if (bet == null)
                return;

            if (bet.IsTaken)
            {
                var delete = await Shell.Current.DisplayAlert(
                "Número ocupado",
                $"Este número pertenece a:\n\n{bet.ParticipantName}\n\n¿Deseas eliminar esta apuesta?",
                "Eliminar",
                "Cancelar");

                if (delete)
                {
                    await _database.DeleteBetAsync(bet.ParticipantId, bet.RaflleWeekId, bet.Number);

                    // Refrescar números bloqueados
                    await LoadNumbersAsync();
                }

                return;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(Views.BetPage)}?number={bet.Number}");
        }

        [RelayCommand]
        public async Task GoToCreateRaffleAsync(int raffleWeekId)
        {
            await Shell.Current.GoToAsync(
                $"{nameof(Views.CreateRafflePage)}?id={raffleWeekId}");
        }


        [RelayCommand]
        public async Task LoadCurrentRaffleAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            CurrentRaffle = await _database.GetCurrentRaffleAsync();

            if (CurrentRaffle == null)
            { 
                IsClosed = true;
                ButtonText = "Nueva rifa semanal";
            }
            else
            {
                IsClosed = CurrentRaffle.IsClosed;
                if (!CurrentRaffle.IsClosed)
                {
                    ButtonText = "Editar rifa semanal";
                    await LoadNumbersAsync(); 
                }
                else
                {
                    ButtonText = "Nueva rifa semanal";
                    CurrentRaffle = new RaffleWeek(); 
                }
            }

            IsBusy = false;
        }

        [RelayCommand]
        public async Task BuildRafflePromotionMessageAsync()
        {
            if (_setting == null)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "Primero debe realizar la configuración del número de WhatsApp",
                    "OK");

                return;
            }

            var message = $"""
                🎟️ RIFA SEMANAL

                🎁 Premio: {CurrentRaffle.PrizeDescription}
                🏛️ Lotería: {CurrentRaffle.LotteryName}
                📅 Fecha: {CurrentRaffle.DrawDate:dddd dd/MM/yyyy}
                💵 Valor de la boleta: {CurrentRaffle.BetPrize:N0}
                💰 Premios:
                • 2 primeras: ${CurrentRaffle.FirstTwoPrize:N0}
                • 2 del medio: ${CurrentRaffle.MiddleTwoPrize:N0}
                • 2 últimas: ${CurrentRaffle.LastTwoPrize:N0}

                📞 Contacto: {_setting.WhatsAppNumber}
                """;
            await _whatsAppService.SendAsync(message, _setting.WhatsAppNumber);
        }
    }
}
