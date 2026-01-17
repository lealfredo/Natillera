using SQLite;

namespace Natillera.Entities
{
    public class RaffleWeek
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Ej: 2025-02
        [Indexed]
        public string WeekCode { get; set; }

        public string PrizeDescription { get; set; }
        public decimal FirstTwoPrize { get; set; }
        public decimal MiddleTwoPrize { get; set; }
        public decimal LastTwoPrize { get; set; }

        public decimal BetPrize { get; set; }

        public string LotteryName { get; set; } = "Lotería de Medellín";

        // Viernes del sorteo
        public DateTime DrawDate { get; set; }

        // Número ganador de 4 dígitos (nullable hasta que salga)
        public string? WinningNumber { get; set; }

        // Abierta / Cerrada
        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
