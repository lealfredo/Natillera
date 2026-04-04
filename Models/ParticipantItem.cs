using Natillera.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Natillera.Models
{
    public class ParticipantItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Participant Model { get; set; }

        public int Id => Model.Id;
        public string Name => Model.Name;
        public string Phone => Model.Phone;
        public decimal MonthlyContribution => Model.MonthlyContribution;

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

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
