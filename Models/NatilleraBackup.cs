using Natillera.Entities;
using Rifa.Entities;

namespace Natillera.Models
{
    public class NatilleraBackup
    {
        public int Version { get; set; } = 1;
        public List<RaffleWeek> Raffles { get; set; } = [];
        public List<Participant> Participants { get; set; } = [];
        public List<Bet> Bets { get; set; } = [];
        public List<RaffleWinner> Winners { get; set; } = [];
        public Setting Setting { get; set; }
        public List<Contribution> Contribution { get; set; } = [];
        public List<Loan> Loan { get; set; } = [];
        public List<LoanPayment> LoanPayment { get; set; } = [];
        public List<Settlement> Settlement { get; set; } = [];
        public List<SettlementDetail> SettlementDetail { get; set; } = [];
    }
}
