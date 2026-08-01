using Natillera.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Models
{
    public class ParticipantFilter
    {
        public int? Id { get; set; } // null = externos
        public string Name { get; set; }
        public bool IsAll { get; set; }

        public Participant Participant { get; set; } // referencia real
    }
}
