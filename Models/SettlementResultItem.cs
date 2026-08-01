using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Models
{
    public class SettlementResultItem
    {
        public string Name { get; set; }

        public decimal Contributed { get; set; }

        public decimal Profit { get; set; }

        public string ContributedFormatted => $"Aportó: $ {Contributed:N0}";
        public string ProfitFormatted => $"Ganó: $ {Profit:N0}";
    }
}
