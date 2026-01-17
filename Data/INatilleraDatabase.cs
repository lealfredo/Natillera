using Natillera.Entities;
using Natillera.Models;

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

        // Apuestas
        Task<int> SaveBetAsync(Bet bet);
        Task<List<Bet>> GetBetsByRaffleAsync(int raffleWeekId);
        Task<List<Bet>> GetBetsByParticipantAsync(int participantId);
        Task<bool> ExistsBetForNumberAsync(string number);
        Task<int> DeleteBetAsync(int participantId, int raffleWeekId, string number);
        Task<Bet?> GetBetByNumberAsync(string number);
        Task<List<RaffleWeek>> GetClosedRafflesAsync();

        // Números apostados
        Task<List<string>> GetTakenNumbersAsync();
        Task<List<BetNumber>> GetBetNumbersAsync(int raflleId);
        Task<int> GetTotalNumbersSoldAsync(int raffleWeekId);

        Task<List<Bet>> GetBetsByNumberAndTypeAsync(string number, BetType type);
        Task SaveRaffleWinnerAsync(RaffleWinner winner);
        Task<List<T>> GetTableAsync<T>() where T : new();
        Task<List<RaffleWinner>> GetWinnersByDrawAsync(int drawId);

        Task<List<RaffleWeek>> GetAllRaffleWeek();

        Task<List<Participant>> GetAllParticipant();

        Task<List<Bet>> GetAllBet();

        Task<List<RaffleWinner>> GetAllRaffleWinner();

        Task SaveRaffleWeekRangeAsync(List<RaffleWeek> raffleWeeks);

        Task SaveParticipantRangeAsync(List<Participant> participants);

        Task SaveBetRangeAsync(List<Bet> bets);

        Task SaveRaffleWinnerRangeAsync(List<RaffleWinner> raffleWinners);
        Task ClearAllAsync();

        //Settings
        Task<int> SaveSettingAsync(Setting setting);
        Task<Setting> GetSettingAsync();
    }
}
