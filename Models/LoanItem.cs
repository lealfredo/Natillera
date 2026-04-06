using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Models
{
    public class LoanItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }

        public DateTime StartDate { get; set; }

        // NUEVO
        public decimal MonthlyInterest { get; set; }

        // INTERESES
        public decimal TotalInterestGenerated { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PendingInterest { get; set; }

        // CAPITAL
        public decimal PrincipalPaid { get; set; }
        public decimal PendingPrincipal { get; set; }

        // TOTALES
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }

        public decimal Pending { get; set; }

        public bool IsPaid { get; set; }

        // UI
        public string Status =>
            IsPaid ? "Pagado"
            : $"Interés: {PendingInterest:C0} | Capital: {PendingPrincipal:C0}";
    }
}
