using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Entities
{
    public class RaffleWinner
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int RaffleDrawId { get; set; }

        public int ParticipantId { get; set; }

        public string BetNumber { get; set; } // 2 dígitos apostados

        public BetType BetType { get; set; }  // Start / Middle / End
    }
}
