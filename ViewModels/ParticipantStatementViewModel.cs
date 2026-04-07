using CommunityToolkit.Mvvm.ComponentModel;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Rifa.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Natillera.ViewModels
{
    [QueryProperty(nameof(ParticipantId), "ParticipantId")]
    public partial class ParticipantStatementViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        [ObservableProperty]
        private int participantId;
        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public decimal totalContributions;
        [ObservableProperty]
        public decimal totalLoaned;
        [ObservableProperty]
        public decimal totalInterestPaid;

        [ObservableProperty]
        public decimal totalRaffleSpent;
        [ObservableProperty]
        public decimal totalRaffleWon;

        [ObservableProperty]
        public decimal balance;


        public ObservableCollection<ContributionItem> Contributions { get; set; } = new();
        public ObservableCollection<LoanItem> NatilleraLoans { get; set; } = new();
        public ObservableCollection<LoanItem> PersonalLoans { get; set; } = new();
        public ObservableCollection<RaffleItem> Raffles { get; set; } = new();

        public ParticipantStatementViewModel(INatilleraDatabase database)
        {
            _database = database;
        }

        public async Task Load()
        {
            var participant = (await _database.GetParticipantsAsync())
                .First(x => x.Id == ParticipantId);

            Name = participant.Name;

            // APORTES
            var contributions = await _database.GetContributionsByParticipant(ParticipantId);

            TotalContributions = contributions.Sum(x => x.Amount);

            foreach (var c in contributions)
            {
                Contributions.Add(new ContributionItem
                {
                    Amount = c.Amount,
                    Name = $"{c.Month}/{c.Year}"
                });
            }

            var loans = (await _database.GetLoansAsync())
                            .Where(x => x.PersonId == ParticipantId)
                            .ToList();

            var natilleraLoans = loans.Where(x => !x.IsPersonal);
            var personalLoans = loans.Where(x => x.IsPersonal);

            foreach (var loan in natilleraLoans)
            {
                try
                {
                    var payments = await _database.GetPaymentsAsync(loan.Id) ?? new List<LoanPayment>();

                    var interestPaid = payments
                        .Where(x => x.IsInterest)
                        .Sum(x => x.Amount);

                    var principalPaid = payments
                        .Where(x => !x.IsInterest)
                        .Sum(x => x.Amount);

                    var pending = (loan?.PrincipalAmount ?? 0) - principalPaid;

                    TotalLoaned += loan?.PrincipalAmount ?? 0;
                    TotalInterestPaid += interestPaid;

                    var item = new LoanItem
                    {
                        Amount = loan?.PrincipalAmount ?? 0,
                        Pending = pending,
                        StartDate = loan.StartDate
                    };

                    // SIEMPRE EN UI THREAD
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        NatilleraLoans.Add(item);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR LOAN: {ex.Message}");
                }
            }

            foreach (var loan in personalLoans)
            {
                try
                {
                    var payments = await _database.GetPaymentsAsync(loan.Id) ?? new List<LoanPayment>();

                    var interestPaid = payments
                        .Where(x => x.IsInterest)
                        .Sum(x => x.Amount);

                    var principalPaid = payments
                        .Where(x => !x.IsInterest)
                        .Sum(x => x.Amount);

                    var pending = (loan?.PrincipalAmount ?? 0) - principalPaid;

                    TotalLoaned += loan?.PrincipalAmount ?? 0;
                    TotalInterestPaid += interestPaid;

                    var item = new LoanItem
                    {
                        Amount = loan?.PrincipalAmount ?? 0,
                        Pending = pending,
                        StartDate = loan.StartDate
                    };

                    // SIEMPRE EN UI THREAD
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        PersonalLoans.Add(item);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR LOAN: {ex.Message}");
                }
            }

            // RIFAS
            var bets = await _database.GetAllBet();
            var raffles = await _database.GetAllRaffleWeek();
            var winners = await _database.GetAllRaffleWinner();

            // apuestas del participante
            var groupedBets = bets
                .Where(x => x.ParticipantId == ParticipantId && x.IsTaken)
                .GroupBy(x => new { x.RaffleWeekId, x.Number })
                .ToList();

            // TOTAL JUGADO
            TotalRaffleSpent = groupedBets.Sum(g =>
            {
                var raffle = raffles.FirstOrDefault(r => r.Id == g.Key.RaffleWeekId);
                return raffle?.BetPrize ?? 0;
            });

            // GANANCIAS
            decimal totalWon = 0;

            foreach (var group in groupedBets)
            {
                var raffle = raffles.FirstOrDefault(r => r.Id == group.Key.RaffleWeekId);
                if (raffle == null) continue;

                var myWins = winners
                    .Where(w =>
                        w.ParticipantId == ParticipantId &&
                        w.RaffleDrawId == group.Key.RaffleWeekId &&
                        w.BetNumber == group.Key.Number)
                    .ToList();

                foreach (var win in myWins)
                {
                    switch (win.BetType)
                    {
                        case BetType.Start:
                            totalWon += raffle.FirstTwoPrize;
                            break;

                        case BetType.Middle:
                            totalWon += raffle.MiddleTwoPrize;
                            break;

                        case BetType.End:
                            totalWon += raffle.LastTwoPrize;
                            break;
                    }
                }
            }

            TotalRaffleWon = totalWon;

            Raffles.Clear();

            var rafflesDict = raffles.ToDictionary(r => r.Id);

            // agrupar winners UNA sola vez
            var winnersLookup = winners
                .Where(w => w.ParticipantId == ParticipantId)
                .GroupBy(w => new { w.RaffleDrawId, w.BetNumber })
                .ToDictionary(
                    g => (g.Key.RaffleDrawId, g.Key.BetNumber),
                    g => g.ToList()
                );


            foreach (var group in groupedBets)
            {
                // acceso rápido (sin recorrer lista)
                if (!rafflesDict.TryGetValue(group.Key.RaffleWeekId, out var raffle))
                    continue;

                // lookup directo (sin Where)
                winnersLookup.TryGetValue(
                    (group.Key.RaffleWeekId, group.Key.Number),
                    out var wins
                );

                var totalWonPerNumber = 0m;

                if (wins != null)
                {
                    foreach (var w in wins)
                    {
                        switch (w.BetType)
                        {
                            case BetType.Start:
                                totalWonPerNumber += raffle.FirstTwoPrize;
                                break;

                            case BetType.Middle:
                                totalWonPerNumber += raffle.MiddleTwoPrize;
                                break;

                            case BetType.End:
                                totalWonPerNumber += raffle.LastTwoPrize;
                                break;
                        }
                    }
                }

                Raffles.Add(new RaffleItem
                {
                    Description = totalWonPerNumber > 0
                        ? $"🏆 {raffle.WeekCode} - {group.Key.Number}"
                        : $"{raffle.WeekCode} - {group.Key.Number}",

                    Amount = raffle.BetPrize,
                    Won = totalWonPerNumber
                });
            }

            // BALANCE FINAL
            Balance =
                TotalContributions
                - TotalLoaned
                + TotalInterestPaid
                - TotalRaffleSpent
                + TotalRaffleWon;
        }
    }
}
