using Natillera.Data;
using Natillera.Entities;
using Rifa.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public DashboardViewModel(INatilleraDatabase database)
        {
            _database = database;

            LoadCommand = new Command(async () => await Load());
        }

        public ICommand LoadCommand { get; }

        // PROPIEDADES

        private decimal _totalContributions;
        public decimal TotalContributions
        {
            get => _totalContributions;
            set { _totalContributions = value; OnPropertyChanged(); }
        }

        private decimal _totalLoaned;
        public decimal TotalLoaned
        {
            get => _totalLoaned;
            set { _totalLoaned = value; OnPropertyChanged(); }
        }

        private decimal _totalRecovered;
        public decimal TotalRecovered
        {
            get => _totalRecovered;
            set { _totalRecovered = value; OnPropertyChanged(); }
        }

        private decimal _totalInterest;
        public decimal TotalInterest
        {
            get => _totalInterest;
            set { _totalInterest = value; OnPropertyChanged(); }
        }

        private decimal _availableMoney;
        public decimal AvailableMoney
        {
            get => _availableMoney;
            set { _availableMoney = value; OnPropertyChanged(); }
        }

        private decimal _totalRaffles;
        public decimal TotalRaffles
        {
            get => _totalRaffles;
            set { _totalRaffles = value; OnPropertyChanged(); }
        }

        private decimal _totalRaffleCollected;
        public decimal TotalRaffleCollected
        {
            get => _totalRaffleCollected;
            set { _totalRaffleCollected = value; OnPropertyChanged(); }
        }

        private decimal _totalRafflePrizes;
        public decimal TotalRafflePrizes
        {
            get => _totalRafflePrizes;
            set { _totalRafflePrizes = value; OnPropertyChanged(); }
        }

        private decimal _raffleProfit;
        public decimal RaffleProfit
        {
            get => _raffleProfit;
            set { _raffleProfit = value; OnPropertyChanged(); }
        }

        // LOAD

        private async Task Load()
        {
            var contributions = await _database.GetAllContributionsAsync();
            var loans = await _database.GetLoansAsync();

            var allPayments = new List<LoanPayment>();

            foreach (var loan in loans)
            {
                var payments = await _database.GetPaymentsAsync(loan.Id);
                allPayments.AddRange(payments);
            }

            // APORTES
            TotalContributions = contributions.Sum(x => x.Amount);

            // PRESTADO
            TotalLoaned = loans.Sum(x => x.Amount);

            // CAPITAL RECUPERADO
            TotalRecovered = allPayments.Sum(x => x.PrincipalPaid);

            // INTERESES GANADOS
            TotalInterest = allPayments.Sum(x => x.InterestPaid);

            var raffles = await _database.GetAllNoPersonalRaffleWeek();
            var bets = await _database.GetAllBet();
            var winners = await _database.GetAllRaffleWinner();

            TotalRaffles = raffles.Count;

            var takenNumbersByRaffle = bets
                .Where(b => b.IsTaken)
                .GroupBy(b => new { b.RaffleWeekId, b.Number })
                .Select(g => g.Key)
                .GroupBy(x => x.RaffleWeekId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Recaudado
            TotalRaffleCollected = raffles.Sum(r =>
            {
                takenNumbersByRaffle.TryGetValue(r.Id, out var count);
                return r.BetPrize * count;
            });

            // Premios
            TotalRafflePrizes = 0;

            foreach (var raffle in raffles)
            {
                var raffleWinners = winners.Where(w => w.RaffleDrawId == raffle.Id);

                foreach (var w in raffleWinners)
                {
                    switch (w.BetType)
                    {
                        case BetType.Start:
                            TotalRafflePrizes += raffle.FirstTwoPrize;
                            break;

                        case BetType.Middle:
                            TotalRafflePrizes += raffle.MiddleTwoPrize;
                            break;

                        case BetType.End:
                            TotalRafflePrizes += raffle.LastTwoPrize;
                            break;
                    }
                }
            }

            // Ganancia
            RaffleProfit = TotalRaffleCollected - TotalRafflePrizes;

            // DISPONIBLE (IMPORTANTE)
            AvailableMoney =
                TotalContributions
                + TotalInterest
                + RaffleProfit //Pendiente hasta diferenciar rifas de la natillera y rifas personales
                - (TotalLoaned - TotalRecovered);
        }
    }
}
