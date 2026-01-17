using Natillera.Entities;
using Natillera.Models;

namespace Natillera.Services
{
    public interface IRaffleService
    {
        Task ProcessDrawAsync(RaffleWeek raffleWeek);
        Task<RaffleEconomicSummary> GetEconomicSummaryAsync(int raffleId);
        Task<List<RaffleWeek>> GetClosedRafflesAsync();
    }
}
