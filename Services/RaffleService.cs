using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;

namespace Natillera.Services
{
    public class RaffleService : IRaffleService
    {
        private readonly INatilleraDatabase _database;

        public RaffleService(INatilleraDatabase db)
        {
            _database = db;
        }

        public async Task ProcessDrawAsync(RaffleWeek raffleWeek)
        {
            await _database.SaveRaffleWeekAsync(raffleWeek);

            string start = raffleWeek.WinningNumber[..2];
            string middle = raffleWeek.WinningNumber.Substring(1, 2);
            string end = raffleWeek.WinningNumber.Substring(2, 2);

            await CreateWinners(raffleWeek, start, BetType.Start);
            await CreateWinners(raffleWeek, middle, BetType.Middle);
            await CreateWinners(raffleWeek, end, BetType.End);
        }

        private async Task CreateWinners(
            RaffleWeek draw,
            string number,
            BetType type)
        {
            var bets = await _database.GetBetsByNumberAndTypeAsync(number, type);

            foreach (var bet in bets)
            {
                await _database.SaveRaffleWinnerAsync(new RaffleWinner
                {
                    RaffleDrawId = draw.Id,
                    ParticipantId = bet.ParticipantId,
                    BetNumber = number,
                    BetType = type,
                });
            }
        }

        public async Task<List<RaffleWeek>> GetClosedRafflesAsync()
        {
            return await _database.GetClosedRafflesAsync();
        }

        public async Task<RaffleEconomicSummary> GetEconomicSummaryAsync(int raffleId)
        {
            var raffle = await _database.GetRaffleByIdAsync(raffleId);
            var bets = await _database.GetTotalNumbersSoldAsync(raffleId);
            var winners = await _database.GetWinnersByDrawAsync(raffleId);

            var totalSold = bets;
            var totalCollected = totalSold * raffle.BetPrize;

            decimal totalPrizes = 0;

            foreach (var winner in winners)
            {
                totalPrizes += winner.BetType switch
                {
                    BetType.Start => raffle.FirstTwoPrize,
                    BetType.Middle => raffle.MiddleTwoPrize,
                    BetType.End => raffle.LastTwoPrize,
                    _ => 0
                };
            }

            return new RaffleEconomicSummary
            {
                TotalSold = totalSold,
                TotalCollected = totalCollected,
                TotalPrizes = totalPrizes,
                NetProfit = totalCollected - totalPrizes
            };
        }
    }
}
