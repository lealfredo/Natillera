using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Natillera.Models
{
    public class LoanMonthItem : INotifyPropertyChanged
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Name { get; set; }

        public decimal InterestAmount { get; set; }

        private bool isPaid;
        public bool IsPaid
        {
            get => isPaid;
            set { isPaid = value; OnPropertyChanged(nameof(IsPaid)); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
