using Natillera.Entities;
using Natillera.Models;
using Rifa.Entities;
using SQLite;
using SQLitePCL;

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
            _database.CreateTableAsync<Setting>().Wait();
            _database.CreateTableAsync<Contribution>().Wait();
            _database.CreateTableAsync<Loan>().Wait();
            _database.CreateTableAsync<LoanPayment>().Wait();
            _database.CreateTableAsync<Settlement>().Wait();
            _database.CreateTableAsync<SettlementDetail>().Wait();
        }

        // ---------------- RIFA ----------------

        public async Task<int> SaveRaffleWeekAsync(RaffleWeek raffle)
        {
            if (raffle.Id != 0)
            {
                raffle.WinningNumber = raffle.WinningNumber == string.Empty ? null : raffle.WinningNumber;
                raffle.IsClosed = raffle.IsClosed;

                return await _database.UpdateAsync(raffle);
            }
            else
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

        public async Task<bool> ExistsBetForNumberAsync(string number, int id)
        {
            var count = await _database
                .Table<Bet>()
                .Where(b => b.Number == number && b.RaffleWeekId == id)
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

        public Task<int> DeleteParticipantAsync(int participantId)
        {
            return _database.Table<Participant>()
                .Where(b => b.Id == participantId)
                .DeleteAsync();
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

        public async Task<int> GetTotalNumbersSoldAsync(int raffleWeekId)
        {
            var bets = await _database
                .Table<Bet>()
                .Where(b => b.RaffleWeekId == raffleWeekId)
                .ToListAsync();

            return bets
                .Select(b => b.Number)
                .Distinct()
                .Count();
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

        public async Task<int> MarkNumberAsPaidAsync(int raffleWeekId, string number)
        {
            var bets = await _database.Table<Bet>()
                .Where(x => x.RaffleWeekId == raffleWeekId && x.Number == number)
                .ToListAsync();

            foreach (var bet in bets)
            {
                bet.IsTaken = true;
            }

            return await _database.UpdateAllAsync(bets);
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
                    IsPay = bet == null ? false : bet.IsTaken,
                    Number = number,
                    IsTaken = bet != null,
                    ParticipantName = bet == null
                        ? null : bet.ParticipantId == null ? bet.Bettor
                        : participants.First(p => p.Id == bet.ParticipantId).Name,
                    ParticipantId = bet == null
                        ? 0 : bet.ParticipantId == null ? 0
                        : participants.First(p => p.Id == bet.ParticipantId).Id,
                    RaflleWeekId = bet == null
                        ? 0
                        : bet.RaffleWeekId
                });
            }

            return numbers;
        }

        public Task<List<Bet>> GetBetsByNumberAndTypeAsync(string number, BetType type, int id)
        {
            return _database.Table<Bet>()
                .Where(b => b.Number == number && b.Type == type && b.RaffleWeekId == id)
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

        public Task<List<RaffleWeek>> GetOpenRafflesAsync()
        {
            return _database.Table<RaffleWeek>()
                      .Where(r => !r.IsClosed)
                      .OrderBy(r => r.DrawDate)
                      .ToListAsync();
        }

        public async Task<List<RaffleWeek>> GetAllNoPersonalRaffleWeek()
        {
            return await _database.Table<RaffleWeek>().Where(r => !r.IsPersonal).ToListAsync();
        }

        public async Task<List<RaffleWeek>> GetAllRaffleWeek()
        {
            return await _database.Table<RaffleWeek>().ToListAsync();
        }

        public async Task<List<Participant>> GetAllParticipant()
        {
            return await _database.Table<Participant>().ToListAsync();
        }

        public async Task<List<Bet>> GetAllBet()
        {
            return await _database.Table<Bet>().ToListAsync();
        }

        public async Task<List<RaffleWinner>> GetAllRaffleWinner()
        {
            return await _database.Table<RaffleWinner>().ToListAsync();
        }

        public async Task<List<RaffleWinner>> GetAllRaffleWinnerByParticipantAsync(int participantId)
        {
            return await _database.Table<RaffleWinner>().Where(p => p.ParticipantId == participantId).ToListAsync();
        }

        public async Task SaveRaffleWeekRangeAsync(List<RaffleWeek> raffleWeeks)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var raffle in raffleWeeks)
                {
                    conn.InsertOrReplace(raffle);
                }
            });
        }

        public async Task<int> DeleteRaffleAsync(int id)
        {
            return await _database.Table<RaffleWeek>()
                .Where(r => r.Id == id)
                .DeleteAsync();
        }

        public async Task SaveParticipantRangeAsync(List<Participant> participants)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var participant in participants)
                {
                    conn.InsertOrReplace(participant);
                }
            });
        }

        public async Task SaveBetRangeAsync(List<Bet> bets)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var bet in bets)
                {
                    conn.InsertOrReplace(bet);
                }
            });
        }

        public async Task SaveRaffleWinnerRangeAsync(List<RaffleWinner> raffleWinners)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var rafleWinner in raffleWinners)
                {
                    conn.InsertOrReplace(rafleWinner);
                }
            });
        }

        public async Task SaveContributionRangeAsync(List<Contribution> contributions)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var contribution in contributions)
                {
                    conn.InsertOrReplace(contribution);
                }
            });
        }

        public async Task SaveLoanRangeAsync(List<Loan> loans)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var loan in loans)
                {
                    conn.InsertOrReplace(loan);
                }
            });
        }

        public async Task SaveLoanPaymentRangeAsync(List<LoanPayment> loanPayments)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var loanPayment in loanPayments)
                {
                    conn.InsertOrReplace(loanPayment);
                }
            });
        }

        public async Task SaveSettlementRangeAsync(List<Settlement> settlements)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var settlement in settlements)
                {
                    conn.InsertOrReplace(settlement);
                }
            });
        }

        public async Task SaveSettlementDetailRangeAsync(List<SettlementDetail> settlementDetails)
        {
            await _database.RunInTransactionAsync(conn =>
            {
                foreach (var settlementDetail in settlementDetails)
                {
                    conn.InsertOrReplace(settlementDetail);
                }
            });
        }

        public async Task ClearAllAsync()
        {
            await _database.ExecuteAsync("DELETE FROM RaffleWinner");
            await _database.ExecuteAsync("DELETE FROM Bet");
            await _database.ExecuteAsync("DELETE FROM Participant");
            await _database.ExecuteAsync("DELETE FROM RaffleWeek");
            await _database.ExecuteAsync("DELETE FROM Setting");
            await _database.ExecuteAsync("DELETE FROM Contribution");
            await _database.ExecuteAsync("DELETE FROM Loan");
            await _database.ExecuteAsync("DELETE FROM LoanPayment");
            await _database.ExecuteAsync("DELETE FROM Settlement");
            await _database.ExecuteAsync("DELETE FROM SettlementDetail");
        }

        public async Task<int> SaveSettingAsync(Setting setting) => 
            await _database.InsertOrReplaceAsync(setting);

        public async Task<Setting> GetSettingAsync()
        {
            return await _database.Table<Setting>().FirstOrDefaultAsync();
        }

        // CONTRIBUTION
        public Task<List<Contribution>> GetAllContributionsAsync()
        => _database.Table<Contribution>()
              .OrderByDescending(x => x.Date)
              .ToListAsync();

        public Task<List<Contribution>> GetContributionsByParticipant(int participantId)
        => _database.Table<Contribution>()
                .Where(p => p.PersonId == participantId)
              .OrderByDescending(x => x.Date)
              .ToListAsync();

        public Task<int> AddContributionAsync(Contribution contribution)
            => _database.InsertAsync(contribution);

        public Task<int> DeleteContributionAsync(Contribution contribution)
            => _database.DeleteAsync(contribution);

        public async Task<bool> ExistsContribution(int participantId, int year, int month)
             => await _database.Table<Contribution>().CountAsync(c => c.PersonId == participantId && c.Year == year && c.Month == month) > 0;

        //--------------LOAN-----------
        public Task<List<Loan>> GetLoansAsync()
        => _database.Table<Loan>().OrderByDescending(x => x.StartDate).ToListAsync();

        public Task<List<LoanPayment>> GetPaymentsAsync(int loanId)
            => _database.Table<LoanPayment>().Where(x => x.LoanId == loanId).ToListAsync();

        public Task<List<LoanPayment>> GetAllPaymentsAsync()
            => _database.Table<LoanPayment>().ToListAsync();

        public Task<int> AddLoanAsync(Loan loan)
            => _database.InsertAsync(loan);

        public Task<int> AddPaymentAsync(LoanPayment payment)
            => _database.InsertAsync(payment);

        public Task<int> UpdateLoanAsync(Loan loan)
            => _database.UpdateAsync(loan);

        //----------- Settlement -----------
        public async Task<List<Settlement>> GetSettlementAsync() 
            => await _database.Table<Settlement>().ToListAsync();

        public Task<int> AddSettlementAsync(Settlement s)
            => _database.InsertAsync(s);

        public async Task<List<SettlementDetail>> GetSettlementDetailAsync()
            => await _database.Table<SettlementDetail>().ToListAsync();

        public Task<int> AddDetailAsync(SettlementDetail d)
            => _database.InsertAsync(d);

        public SQLiteAsyncConnection GetConnection() => _database;
    }
}
