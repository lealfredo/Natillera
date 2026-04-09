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

        private decimal _availableFromContributions;
        public decimal AvailableFromContributions
        {
            get => _availableFromContributions;
            set { _availableFromContributions = value; OnPropertyChanged(); }
        }

        private decimal _availableFromInterest;
        public decimal AvailableFromInterest
        {
            get => _availableFromInterest;
            set { _availableFromInterest = value; OnPropertyChanged(); }
        }

        private decimal _loanFromContributions;
        public decimal LoanFromContributions
        {
            get => _loanFromContributions;
            set { _loanFromContributions = value; OnPropertyChanged(); }
        }

        private decimal _loanFromInterest;
        public decimal LoanFromInterest
        {
            get => _loanFromInterest;
            set { _loanFromInterest = value; OnPropertyChanged(); }
        }

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

        private decimal _loanFromRaffles;
        public decimal LoanFromRaffles
        {
            get => _loanFromRaffles;
            set { _loanFromRaffles = value; OnPropertyChanged(); }
        }

        private decimal _availableFromRaffles;
        public decimal AvailableFromRaffles
        {
            get => _availableFromRaffles;
            set { _availableFromRaffles = value; OnPropertyChanged(); }
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

        public decimal _totalOutstandingLoans;
        public decimal TotalOutstandingLoans
        {
            get => _totalOutstandingLoans;
            set { _totalOutstandingLoans = value; OnPropertyChanged(); }
        }

        // LOAD

        private async Task Load()
        {
            // 🔥 DATA BASE
            var contributions = await _database.GetAllContributionsAsync();
            var loans = await _database.GetLoansAsync();
            var allPayments = await _database.GetAllPaymentsAsync(); // 🔥 optimizado

            var natilleraLoans = loans.Where(x => !x.IsPersonal).ToList();

            // =========================
            // 🔹 APORTES
            // =========================
            TotalContributions = contributions.Sum(x => x.Amount);

            // =========================
            // 🔹 PRÉSTAMOS
            // =========================
            TotalLoaned = natilleraLoans.Sum(x => x.PrincipalAmount);

            TotalRecovered = allPayments
                .Where(x => !x.IsInterest && natilleraLoans.Any(l => l.Id == x.LoanId))
                .Sum(x => x.Amount);

            TotalInterest = allPayments
                .Where(x => x.IsInterest && natilleraLoans.Any(l => l.Id == x.LoanId) && !x.IsFromPersonalLoan)
                .Sum(x => x.Amount);

            TotalOutstandingLoans = TotalLoaned - TotalRecovered;

            // =========================
            // 🔹 RIFAS
            // =========================
            var raffles = await _database.GetAllRafflesNatilleraAsync();
            var bets = await _database.GetAllBet();
            var winners = await _database.GetAllRaffleWinner();

            TotalRaffles = raffles.Count;

            var takenNumbersByRaffle = bets
                .Where(b => b.IsTaken)
                .GroupBy(b => new { b.RaffleWeekId, b.Number })
                .Select(g => g.Key)
                .GroupBy(x => x.RaffleWeekId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 🔥 RECAUDADO
            TotalRaffleCollected = raffles.Sum(r =>
            {
                takenNumbersByRaffle.TryGetValue(r.Id, out var count);
                return r.BetPrize * count;
            });

            // 🔥 PREMIOS
            TotalRafflePrizes = 0;

            foreach (var raffle in raffles)
            {
                var raffleWinners = winners.Where(w => w.RaffleDrawId == raffle.Id);

                foreach (var w in raffleWinners)
                {
                    TotalRafflePrizes += w.BetType switch
                    {
                        BetType.Start => raffle.FirstTwoPrize,
                        BetType.Middle => raffle.MiddleTwoPrize,
                        BetType.End => raffle.LastTwoPrize,
                        _ => 0
                    };
                }
            }

            // 🔥 BALANCE REAL RIFAS
            RaffleProfit = TotalRaffleCollected - TotalRafflePrizes;
            if (RaffleProfit < 0) RaffleProfit = 0;

            // =========================
            // 🔹 RECUPERACIÓN POR FUENTE
            // =========================
            decimal recoveredFromInterest = 0;
            decimal recoveredFromContributions = 0;
            decimal recoveredFromRaffles = 0;

            foreach (var loan in natilleraLoans)
            {
                var payments = allPayments.Where(x => x.LoanId == loan.Id && !x.IsInterest);

                var totalPrincipal = loan.PrincipalAmount;
                if (totalPrincipal == 0) continue;

                var ratioInterest = loan.PrincipalFromInterest / totalPrincipal;
                var ratioContributions = loan.PrincipalFromContributions / totalPrincipal;
                var ratioRaffles = loan.PrincipalFromRaffles / totalPrincipal;

                var totalPaid = payments.Sum(x => x.Amount);

                recoveredFromInterest += totalPaid * ratioInterest;
                recoveredFromContributions += totalPaid * ratioContributions;
                recoveredFromRaffles += totalPaid * ratioRaffles;
            }

            // =========================
            // 🔹 PRESTADO POR FUENTE
            // =========================
            LoanFromContributions = natilleraLoans.Sum(x => x.PrincipalFromContributions);
            LoanFromInterest = natilleraLoans.Sum(x => x.PrincipalFromInterest);
            LoanFromRaffles = natilleraLoans.Sum(x => x.PrincipalFromRaffles);

            // =========================
            // 🔹 DISPONIBLE REAL
            // =========================
            AvailableFromContributions =
                TotalContributions
                - LoanFromContributions
                + recoveredFromContributions;

            AvailableFromInterest =
                TotalInterest
                - LoanFromInterest
                + recoveredFromInterest;

            AvailableFromRaffles =
                RaffleProfit
                - LoanFromRaffles
                + recoveredFromRaffles;

            // 🔥 SEGURIDAD
            if (AvailableFromContributions < 0) AvailableFromContributions = 0;
            if (AvailableFromInterest < 0) AvailableFromInterest = 0;
            if (AvailableFromRaffles < 0) AvailableFromRaffles = 0;

            // =========================
            // 🔹 TOTAL FINAL
            // =========================
            AvailableMoney =
                AvailableFromContributions +
                AvailableFromInterest +
                AvailableFromRaffles;
        }
    }
}
