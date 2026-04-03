using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rifa.Entities
{
    public class Settlement
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal InitialCapital { get; set; }
        public decimal FinalCapital { get; set; }

        public decimal Profit { get; set; }

        public decimal ParticipantShare { get; set; }
        public decimal AdminShare { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
