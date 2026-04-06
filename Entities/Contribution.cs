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

        [Indexed]
        public int PersonId { get; set; }

        public decimal Amount { get; set; }

        [Indexed]
        public int Year { get; set; }
        [Indexed]
        public int Month { get; set; }  

        public DateTime Date { get; set; }
    }
}
