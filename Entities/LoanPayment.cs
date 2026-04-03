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

        public decimal Amount { get; set; }

        public decimal InterestPaid { get; set; }
        public decimal PrincipalPaid { get; set; }
        public DateTime Date { get; set; }
    }
}
