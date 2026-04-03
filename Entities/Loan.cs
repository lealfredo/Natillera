using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rifa.Entities
{
    public class Loan
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int? PersonId { get; set; } // null = externo
        public string BorrowerName { get; set; }

        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal PrincipalPaid { get; set; } // cuánto capital ya pagó

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }
    }
}
