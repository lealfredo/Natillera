namespace Natillera.Models
{
    public class BetNumber
    {
        public int RaflleWeekId { get; set; }
        public string Number { get; set; } // "00" a "99"
        public bool IsTaken { get; set; }
        public string? ParticipantName { get; set; }
        public int ParticipantId { get; set; }
    }
}
