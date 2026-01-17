using Natillera.Entities;
using Natillera.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Services
{
    public interface IRaffleService
    {
        Task ProcessDrawAsync(RaffleWeek raffleWeek);
        Task<RaffleEconomicSummary> GetEconomicSummaryAsync(int raffleId);
        Task<List<RaffleWeek>> GetClosedRafflesAsync();
    }
}
