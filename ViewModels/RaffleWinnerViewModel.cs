using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Natillera.ViewModels
{
    [QueryProperty(nameof(RaffleId), "RaffleId")]
    public partial class RaffleWinnerViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;
        private readonly IRaffleService _raffleService;
        private readonly IWhatsAppService _whatsAppService;

        public ObservableCollection<WinnerItem> Winners { get; } = new();

        [ObservableProperty]
        private int raffleId;

        [ObservableProperty]
        private string winningNumber;

        [ObservableProperty]
        private RaffleWeek currentRaffle;

        [ObservableProperty]
        private bool isClose;
        public bool IsOpen => !IsClose;

        [ObservableProperty]
        private bool exist;
        public bool NoExist => !Exist;

        [ObservableProperty]
        private int totalSold;

        [ObservableProperty]
        private decimal totalCollected;

        [ObservableProperty]
        private decimal totalPrizes;

        [ObservableProperty]
        private decimal netProfit;

        public RaffleWinnerViewModel(INatilleraDatabase db, IRaffleService raffleService, IWhatsAppService whatsAppService)
        {
            _database = db;
            _raffleService = raffleService;
            Title = "Ganadores de la Rifa";
            _whatsAppService = whatsAppService;
        }

        private async Task LoadEconomicSummaryAsync()
        {
            var summary = await _raffleService.GetEconomicSummaryAsync(CurrentRaffle.Id);

            TotalSold = summary.TotalSold;
            TotalCollected = summary.TotalCollected;
            TotalPrizes = summary.TotalPrizes;
            NetProfit = summary.NetProfit;
        }

        public async Task LoadWinnersAsync()
        {
            Winners.Clear();

            // Último sorteo
            var lastDraw = new RaffleWeek();
            if (RaffleId > 0)
                lastDraw = await _database.GetRaffleByIdAsync(RaffleId);
            else
                lastDraw = await _database.GetCurrentRaffleAsync();

            if (lastDraw == null)
            {
                Exist = false;
                return; 
            }

            Exist = true;

            IsClose = lastDraw.IsClosed;

            CurrentRaffle = lastDraw;

            WinningNumber = lastDraw.IsClosed ? lastDraw.WinningNumber : string.Empty;

            if (!lastDraw.IsClosed) return;

            await LoadEconomicSummaryAsync();

            var winners = await _database.GetWinnersByDrawAsync(lastDraw.Id);

            foreach (var winner in winners)
            {
                var participant = await _database
                    .GetParticipantByIdAsync(winner.ParticipantId);

                Winners.Add(new WinnerItem
                {
                    ParticipantName = participant.Name,
                    Phone = participant.Phone,
                    Number = winner.BetNumber,
                    BetType = winner.BetType == BetType.Start ? "Primeros 2" :
                                winner.BetType == BetType.Middle ? "Dos del medio" :
                                "Últimos 2",
                    PrizeAmount = winner.BetType == BetType.Start ? lastDraw.FirstTwoPrize :
                                  winner.BetType == BetType.Middle ? lastDraw.MiddleTwoPrize :
                                  lastDraw.LastTwoPrize
                });
            }
        }

        partial void OnIsCloseChanged(bool value)
        {
            OnPropertyChanged(nameof(IsOpen));
        }

        partial void OnExistChanged(bool value)
        {
            OnPropertyChanged(nameof(NoExist));
        }

        [RelayCommand]
        private async Task SaveWinningNumberAsync()
        {
            if (IsBusy) return;

            IsBusy = true;

            if (string.IsNullOrWhiteSpace(WinningNumber) ||
                WinningNumber.Length != 4)
                return;

            var lastDraw = await _database.GetCurrentRaffleAsync();

            if (lastDraw == null) return;

            lastDraw.WinningNumber = WinningNumber;
            lastDraw.IsClosed = true;

            await _raffleService.ProcessDrawAsync(lastDraw);

            await LoadWinnersAsync();

            IsBusy = false;
        }

        [RelayCommand]
        private async Task SendSummaryAsync()
        {
            var message = $"""
                🎟️ RIFA SEMANAL
                Semana: {CurrentRaffle.WeekCode}
                Lotería: {CurrentRaffle.LotteryName}
                Fecha: {CurrentRaffle.DrawDate:dd/MM/yyyy}

                📊 Resumen:
                • Números vendidos: {TotalSold}
                • Total recaudado: ${TotalCollected:N0}
                • Total premios: ${TotalPrizes:N0}
                • Ganancia neta: ${NetProfit:N0}
                """;

            await _whatsAppService.SendAsync(message, "3163236534");
        }

        [RelayCommand]
        public async Task BuildRaffleResultMessageAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine("🎉 RESULTADO RIFA 🎉\n");
            sb.AppendLine($"🏛️ Lotería: {CurrentRaffle.LotteryName}");
            sb.AppendLine($"📅 {CurrentRaffle.DrawDate:dd/MM/yyyy}");
            sb.AppendLine($"🔢 Número ganador: {CurrentRaffle.WinningNumber}\n");

            sb.AppendLine("🏆 GANADORES 🏆\n");

            foreach (var winner in Winners)
            {
                var emoji = winner.BetType == "Primeros 2" ? "🥉" :
                            winner.BetType == "Dos del medio" ? "🥈" :
                            "🥇";
                sb.AppendLine($"{emoji} {winner.BetType} ({winner.Number}):");

                sb.AppendLine($"• {winner.ParticipantName} – ${winner.PrizeAmount:N0}");

                sb.AppendLine();
            }

            sb.AppendLine("¡Felicitaciones! 🎉");


            await _whatsAppService.SendAsync(sb.ToString(), "3163236534");
        }

    }
}
