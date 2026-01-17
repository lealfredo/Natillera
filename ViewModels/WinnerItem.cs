using CommunityToolkit.Mvvm.ComponentModel;

namespace Natillera.ViewModels
{
    public partial class WinnerItem : ObservableObject
    {
        public string ParticipantName { get; set; }
        public string Phone { get; set; }
        public string Number { get; set; }
        public string BetType { get; set; }
        public decimal PrizeAmount { get; set; }
    }
}
