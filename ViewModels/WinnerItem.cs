using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

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
