using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Natillera.Models
{
    public class MonthItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Month { get; set; }
        public int MonthNumber { get; set; } // 1 = Enero, etc
        public int Year { get; set; } = DateTime.Now.Year;

        public bool IsEnabled { get; set; }
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        private string _state;
        public string State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        private decimal paidAmount;
        public decimal PaidAmount
        {
            get => paidAmount;
            set
            {
                paidAmount = value;
                OnPropertyChanged(nameof(PaidAmount));
            }
        }

        private double progress;
        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
