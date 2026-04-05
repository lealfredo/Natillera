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
    [QueryProperty(nameof(RaffleId), "RaffleId")]
    public partial class RaffleViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;
        private readonly IWhatsAppService _whatsAppService;
        private Setting _setting;

        [ObservableProperty]
        private int raffleId;
        public ICommand SelectNumberCommand { get; }
        public ObservableCollection<BetNumber> Numbers { get; } = new();

        [ObservableProperty]
        private RaffleWeek currentRaffle;

        [ObservableProperty]
        private bool isClosed;

        [ObservableProperty]
        private string buttonText;

        public bool IsOpen => !IsClosed;

        public ICommand MarkAsPaidCommand { get; }

        public RaffleViewModel(INatilleraDatabase database, IWhatsAppService whatsAppService)
        {
            _database = database;
            Title = "Rifa Semanal";

            SelectNumberCommand = new Command<BetNumber>(OnSelectNumber);
            _whatsAppService = whatsAppService;
            MarkAsPaidCommand = new Command<BetNumber>(async (bet) => await MarkAsPaid(bet));
        }

        private async Task MarkAsPaid(BetNumber bet)
        {
            if (bet == null) return;

            if (bet.IsPay)
                return;

            var confirm = await Shell.Current.DisplayAlert(
                "Confirmar",
                $"¿Marcar TODAS las apuestas del número {bet.Number} como pagadas?",
                "Sí",
                "No");

            if (!confirm) return;

            await _database.MarkNumberAsPaidAsync(bet.RaflleWeekId, bet.Number);

            await LoadNumbersAsync();
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

        public Func<BetNumber, Task<string>> ShowOptionsAction { get; set; }

        private async void OnSelectNumber(BetNumber bet)
        {
            if (bet == null)
                return;

            if (bet.IsPay) return;

            if (bet.IsTaken)
            {
                if (ShowOptionsAction == null)
                    return;

                var action = await ShowOptionsAction.Invoke(bet);

                switch (action)
                {
                    case "Marcar como pagado":
                        await MarkAsPaid(bet);
                        bet.IsPay = true;
                        break;

                    case "Eliminar apuesta":
                        await DeleteBetGroup(bet);
                        bet.IsTaken = false;
                        break;
                }

                return;
            }

            await Shell.Current.GoToAsync(
                $"{nameof(Views.BetPage)}?number={bet.Number}&raffleId={RaffleId}");
        }

        private async Task DeleteBetGroup(BetNumber bet)
        {
            await _database.DeleteBetAsync(
                bet.ParticipantId,
                bet.RaflleWeekId,
                bet.Number);

            await LoadNumbersAsync();
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
            if (RaffleId == 0) return;

            if (IsBusy) return;
            IsBusy = true;

            CurrentRaffle = await _database.GetRaffleByIdAsync(RaffleId);

            if (CurrentRaffle == null)
            { 
                //IsClosed = true;
                //ButtonText = "Nueva rifa semanal";
                //CurrentRaffle = new();
            }
            else
            {

                ButtonText = "Editar rifa semanal";
                await LoadNumbersAsync();

                IsClosed = CurrentRaffle.IsClosed;
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
