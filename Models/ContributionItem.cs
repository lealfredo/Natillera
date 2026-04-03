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
    }
}
