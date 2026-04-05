using SQLite;

namespace Natillera.Entities
{
    public class RaffleWinner
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int RaffleDrawId { get; set; }

        [Indexed]
        public int? ParticipantId { get; set; }
        public string Bettor { get; set; }

        public string BetNumber { get; set; } // 2 dígitos apostados

        public BetType BetType { get; set; }  // Start / Middle / End
    }
}
