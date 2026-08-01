using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Models
{
    public class ContributionItem
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string DateFormatted => Date.ToString("dd/MM/yyyy HH:mm");
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName
        {
            get
            {
                if (Year <= 0 || Month <= 0) // valores inválidos
                    return "—";              // o "No definido"

                return new DateTime(Year, Month, 1).ToString("MMMM").ToUpper();
            }
        }
    }
}
