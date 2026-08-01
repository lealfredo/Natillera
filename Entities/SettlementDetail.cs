using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rifa.Entities
{
    public class SettlementDetail
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int SettlementId { get; set; }
        [Indexed]
        public int PersonId { get; set; }

        public decimal TotalContributed { get; set; }

        public decimal ParticipationPercentage { get; set; }

        public decimal ProfitEarned { get; set; }
    }
}
