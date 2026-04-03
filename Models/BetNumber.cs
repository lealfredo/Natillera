using System.ComponentModel;

namespace Natillera.Models
{
    public class BetNumber : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int RaflleWeekId { get; set; }
        public string Number { get; set; } // "00" a "99"
        public string? ParticipantName { get; set; }
        public int ParticipantId { get; set; }

        private bool _isTaken;
        public bool IsTaken
        {
            get => _isTaken;
            set
            {
                _isTaken = value;
                OnPropertyChanged(nameof(IsTaken));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        private bool _isPay;
        public bool IsPay
        {
            get => _isPay;
            set
            {
                _isPay = value;
                OnPropertyChanged(nameof(IsPay));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public Color BackgroundColor =>
            IsPay ? Colors.Gray :
            IsTaken ? Colors.Red :
            Colors.Green;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
