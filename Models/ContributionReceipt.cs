using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Models
{
    public class ContributionReceipt
    {
        public string ParticipantName { get; set; }
        public DateTime Date { get; set; }

        public List<ContributionDetail> Details { get; set; } = new();

        public decimal Total => Details.Sum(x => x.Amount);
    }

    public class ContributionDetail
    {
        public string MonthName { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}
