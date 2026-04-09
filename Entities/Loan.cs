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

        public decimal PrincipalAmount { get; set; } // monto prestado
        public decimal PrincipalFromContributions { get; set; }
        public decimal PrincipalFromInterest { get; set; }
        public decimal PrincipalFromRaffles { get; set; }
        public decimal InterestRate { get; set; } // % mensual (ej: 5)
        public bool IsPaid { get; set; }

        public int TotalMonths { get; set; }

        public DateTime StartDate { get; set; }
        public bool IsPersonal { get; set; }
    }
}
