using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rifa.Entities
{
    public class Contribution
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Name = "IX_Participant_Year_Month", Order = 1, Unique = true)]
        public int PersonId { get; set; }

        public decimal Amount { get; set; }

        [Indexed(Name = "IX_Participant_Year_Month", Order = 1, Unique = true)]
        public int Year { get; set; }
        [Indexed(Name = "IX_Participant_Year_Month", Order = 3, Unique = true)]
        public int Month { get; set; }  

        public DateTime Date { get; set; }
    }
}
