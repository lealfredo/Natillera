using SQLite;

namespace Natillera.Entities
{
    public class Bet
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int RaffleWeekId { get; set; }

        [Indexed]
        public int ParticipantId { get; set; }

        // Start | Middle | End
        [Indexed(Name = "IX_Number_Type", Order = 2, Unique = true)]
        public BetType Type { get; set; }

        [Indexed(Name = "IX_Number_Type", Order = 1, Unique = true)]
        public string Number { get; set; }

        // Se marca cuando se cierra la rifa
        public bool IsWinner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
