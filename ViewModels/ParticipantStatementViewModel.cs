using CommunityToolkit.Mvvm.ComponentModel;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
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
        public ObservableCollection<LoanItem> Loans { get; set; } = new();
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

            // PRÉSTAMOS
            var loans = (await _database.GetLoansAsync())
                .Where(x => x.PersonId == ParticipantId);

            foreach (var loan in loans)
            {
                var payments = await _database.GetPaymentsAsync(loan.Id);

                var interestPaid = payments
                    .Where(x => x.IsInterest)
                    .Sum(x => x.Amount);

                var principalPaid = payments
                    .Where(x => !x.IsInterest)
                    .Sum(x => x.Amount);

                var pending = loan.PrincipalAmount - principalPaid;

                TotalLoaned += loan.PrincipalAmount;
                TotalInterestPaid += interestPaid;

                Loans.Add(new LoanItem
                {
                    Amount = loan.PrincipalAmount,
                    Pending = pending,
                    StartDate = loan.StartDate
                });
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

            foreach (var group in groupedBets)
            {
                var raffle = raffles.FirstOrDefault(r => r.Id == group.Key.RaffleWeekId);

                var wins = winners
                    .Where(w =>
                        w.ParticipantId == ParticipantId &&
                        w.RaffleDrawId == group.Key.RaffleWeekId &&
                        w.BetNumber == group.Key.Number)
                    .ToList();

                var totalWonPerNumber = 0m;

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

                Raffles.Add(new RaffleItem
                {
                    Description = totalWonPerNumber > 0
                        ? $"🏆 {raffle?.WeekCode} - {group.Key.Number}"
                        : $"{raffle?.WeekCode} - {group.Key.Number}",

                    Amount = raffle?.BetPrize ?? 0,

                    // si quieres agregar esto al modelo
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
