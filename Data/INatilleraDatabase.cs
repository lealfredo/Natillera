using Natillera.Entities;
using Natillera.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

        Task<List<Bet>> GetBetsByNumberAndTypeAsync(string number, BetType type);
        Task SaveRaffleWinnerAsync(RaffleWinner winner);
        Task<List<T>> GetTableAsync<T>() where T : new();
        Task<List<RaffleWinner>> GetWinnersByDrawAsync(int drawId);
    }
}
