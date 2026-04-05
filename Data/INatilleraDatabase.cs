using Natillera.Entities;
using Natillera.Models;
using Rifa.Entities;
using SQLite;

namespace Natillera.Data
{
    public interface INatilleraDatabase
    {
        // Rifa
        Task<int> SaveRaffleWeekAsync(RaffleWeek raffle);
        Task<RaffleWeek?> GetCurrentRaffleAsync();
        Task<RaffleWeek?> GetRaffleByIdAsync(int id);

        // Participantes
        Task<List<Participant>> GetParticipantsAsync();
        Task<int> SaveParticipantAsync(Participant participant);
        Task<Participant> GetParticipantByPhoneAsync(string phoneNumber);
        Task<Participant> GetParticipantByIdAsync(int id);
        Task<int> DeleteParticipantAsync(int participantId);

        // Apuestas
        Task<int> SaveBetAsync(Bet bet);
        Task<List<Bet>> GetBetsByRaffleAsync(int raffleWeekId);
        Task<List<Bet>> GetBetsByParticipantAsync(int participantId);
        Task<bool> ExistsBetForNumberAsync(string number, int id);
        Task<int> DeleteBetAsync(int participantId, int raffleWeekId, string number);
        Task<Bet?> GetBetByNumberAsync(string number);
        Task<List<RaffleWeek>> GetClosedRafflesAsync();
        Task<List<RaffleWeek>> GetOpenRafflesAsync();
        Task<int> DeleteRaffleAsync(int id);
        Task<List<RaffleWeek>> GetAllNoPersonalRaffleWeek();

        // Números apostados
        Task<List<string>> GetTakenNumbersAsync();
        Task<List<BetNumber>> GetBetNumbersAsync(int raflleId);
        Task<int> GetTotalNumbersSoldAsync(int raffleWeekId);
        Task<int> MarkNumberAsPaidAsync(int raffleWeekId, string number);

        Task<List<Bet>> GetBetsByNumberAndTypeAsync(string number, BetType type, int id);
        Task SaveRaffleWinnerAsync(RaffleWinner winner);
        Task<List<T>> GetTableAsync<T>() where T : new();
        Task<List<RaffleWinner>> GetWinnersByDrawAsync(int drawId);
        Task<List<RaffleWinner>> GetAllRaffleWinnerByParticipantAsync(int participantId);

        Task<List<RaffleWeek>> GetAllRaffleWeek();

        Task<List<Participant>> GetAllParticipant();

        Task<List<Bet>> GetAllBet();

        Task<List<RaffleWinner>> GetAllRaffleWinner();

        Task SaveRaffleWeekRangeAsync(List<RaffleWeek> raffleWeeks);

        Task SaveParticipantRangeAsync(List<Participant> participants);

        Task SaveBetRangeAsync(List<Bet> bets);

        Task SaveRaffleWinnerRangeAsync(List<RaffleWinner> raffleWinners);
        Task SaveContributionRangeAsync(List<Contribution> contributions);
        Task SaveLoanRangeAsync(List<Loan> loans);
        Task SaveLoanPaymentRangeAsync(List<LoanPayment> loanPayments);
        Task SaveSettlementRangeAsync(List<Settlement> settlements);
        Task SaveSettlementDetailRangeAsync(List<SettlementDetail> settlementDetails);
        Task ClearAllAsync();

        //Settings
        Task<int> SaveSettingAsync(Setting setting);
        Task<Setting> GetSettingAsync();

        // CONTRIBUTION
        Task<List<Contribution>> GetAllContributionsAsync();
        Task<List<Contribution>> GetContributionsByParticipant(int participantId);
        Task<int> AddContributionAsync(Contribution contribution);
        Task<int> DeleteContributionAsync(Contribution contribution);
        Task<bool> ExistsContribution(int participantId, int year, int month);

        //--------------LOAN-----------
        Task<List<Loan>> GetLoansAsync();
        Task<List<LoanPayment>> GetPaymentsAsync(int loanId);
        Task<List<LoanPayment>> GetAllPaymentsAsync();
        Task<int> AddLoanAsync(Loan loan);
        Task<int> AddPaymentAsync(LoanPayment payment);
        Task<int> UpdateLoanAsync(Loan loan);

        //----------- Settlement -----------
        Task<List<Settlement>> GetSettlementAsync();
        Task<int> AddSettlementAsync(Settlement s);
        Task<List<SettlementDetail>> GetSettlementDetailAsync();
        Task<int> AddDetailAsync(SettlementDetail d);

        SQLiteAsyncConnection GetConnection();
    }
}
