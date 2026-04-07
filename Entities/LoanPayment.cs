using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rifa.Entities
{
    public class LoanPayment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int LoanId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal Amount { get; set; }

        public bool IsInterest { get; set; } // separa interés vs capital

        public DateTime Date { get; set; }
        public bool IsFromPersonalLoan { get; set; }
    }
}
