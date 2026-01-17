using Natillera.Entities;
using Natillera.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Data
{
    public class NatilleraDatabase : INatilleraDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public NatilleraDatabase(string databasePath)
        {
            _database = new SQLiteAsyncConnection(databasePath);

            _database.CreateTableAsync<RaffleWeek>().Wait();
            _database.CreateTableAsync<Participant>().Wait();
            _database.CreateTableAsync<Bet>().Wait();
            _database.CreateTableAsync<RaffleWinner>().Wait();
        }

        // ---------------- RIFA ----------------

        public async Task<int> SaveRaffleWeekAsync(RaffleWeek raffle)
        {
            if (raffle.Id != 0)
            {
                raffle.WinningNumber = raffle.WinningNumber == string.Empty ? null : raffle.WinningNumber;
                raffle.IsClosed = raffle.IsClosed;

                return await _database.UpdateAsync(raffle);
            }else
                return await _database.InsertAsync(raffle);
        }

        public async Task<RaffleWeek?> GetCurrentRaffleAsync()
        {
            return await _database.Table<RaffleWeek>()
                //.Where(r => !r.IsClosed)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<RaffleWeek?> GetRaffleByIdAsync(int id)
        {
            return await _database.FindAsync<RaffleWeek>(id);
        }

        public async Task<bool> ExistsBetForNumberAsync(string number)
        {
            var count = await _database
                .Table<Bet>()
                .Where(b => b.Number == number)
                .CountAsync();

            return count > 0;
        }

        // ------------- PARTICIPANTES -------------

        public async Task<Participant> GetParticipantByPhoneAsync(string phoneNumber)
        {
            return await _database
                .Table<Participant>()
                .Where(p => p.Phone == phoneNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Participant>> GetParticipantsAsync()
        {
            return await _database
                .Table<Participant>()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<int> SaveParticipantAsync(Participant participant)
        {
            if (participant.Id != 0)
                return await _database.UpdateAsync(participant);

            return await _database.InsertAsync(participant);
        }

        public Task<Participant> GetParticipantByIdAsync(int id)
        {
            return _database.Table<Participant>()
                .Where(p => p.Id == id)
                .FirstAsync();
        }

        // ---------------- APUESTAS ----------------

        public async Task<int> SaveBetAsync(Bet bet)
        {
            if (bet.Id != 0)
                return await _database.UpdateAsync(bet);

            return await _database.InsertAsync(bet);
        }

        public async Task<List<Bet>> GetBetsByRaffleAsync(int raffleWeekId)
        {
            return await _database
                .Table<Bet>()
                .Where(b => b.RaffleWeekId == raffleWeekId)
                .ToListAsync();
        }

        public async Task<List<Bet>> GetBetsByParticipantAsync(int participantId)
        {
            return await _database
                .Table<Bet>()
                .Where(b => b.ParticipantId == participantId)
                .ToListAsync();
        }

        public Task<int> DeleteBetAsync(int participantId, int raffleWeekId, string number)
        {
            return _database.Table<Bet>()
                .Where(b => b.RaffleWeekId == raffleWeekId && b.ParticipantId == participantId && b.Number == number)
                .DeleteAsync();
        }

        public Task<Bet?> GetBetByNumberAsync(string number)
        {
            return _database.Table<Bet>()
                .Where(b => b.Number == number)
                .FirstOrDefaultAsync();
        }

        // -------- Numero seleccionado -------------
        public async Task<List<string>> GetTakenNumbersAsync()
        {
            var bets = await _database.Table<Bet>().ToListAsync();

            return bets
                .Select(b => b.Number)
                .ToList();
        }

        public async Task<List<BetNumber>> GetBetNumbersAsync(int raflleId)
        {
            var bets = await _database.Table<Bet>()
                .Where(r => r.RaffleWeekId == raflleId)
                .ToListAsync();

            var participants = await _database.Table<Participant>()
                .ToListAsync();

            var numbers = new List<BetNumber>();

            for (int i = 0; i < 100; i++)
            {
                var number = i.ToString("D2");

                var bet = bets.FirstOrDefault(b => b.Number == number);

                numbers.Add(new BetNumber
                {
                    Number = number,
                    IsTaken = bet != null,
                    ParticipantName = bet == null
                        ? null
                        : participants.First(p => p.Id == bet.ParticipantId).Name,
                    ParticipantId = bet == null
                        ? 0
                        : participants.First(p => p.Id == bet.ParticipantId).Id,
                    RaflleWeekId = bet == null
                        ? 0
                        : bet.RaffleWeekId
                });
            }

            return numbers;
        }

        public Task<List<Bet>> GetBetsByNumberAndTypeAsync(string number, BetType type)
        {
            return _database.Table<Bet>()
                .Where(b => b.Number == number && b.Type == type)
                .ToListAsync();
        }

        public Task SaveRaffleWinnerAsync(RaffleWinner winner)
        {
            return _database.InsertAsync(winner);
        }

        // GENERIC TABLE ACCESS
        public Task<List<T>> GetTableAsync<T>() where T : new()
            => _database.Table<T>().ToListAsync();

        public Task<List<RaffleWinner>> GetWinnersByDrawAsync(int drawId)
        => _database.Table<RaffleWinner>()
              .Where(w => w.RaffleDrawId == drawId)
              .ToListAsync();

        public Task<List<RaffleWeek>> GetClosedRafflesAsync()
        {
            return _database.Table<RaffleWeek>()
                      .Where(r => r.IsClosed)
                      .OrderByDescending(r => r.DrawDate)
                      .ToListAsync();
        }
    }
}
