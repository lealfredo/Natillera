using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Models
{
    public class RaffleEconomicSummary
    {
        public int TotalSold { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalPrizes { get; set; }
        public decimal NetProfit { get; set; }
    }
}
