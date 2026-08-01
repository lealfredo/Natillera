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

        private Participant _model;
        public Participant Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(Id));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Phone));
                OnPropertyChanged(nameof(MonthlyContribution));
                OnPropertyChanged(nameof(Initials)); // 🔥 IMPORTANTE
            }
        }

        public int Id => Model?.Id ?? 0;
        public string Name => Model?.Name ?? "";
        public string Phone => Model?.Phone ?? "";
        public decimal MonthlyContribution => Model?.MonthlyContribution ?? 0;

        // 🔥 NUEVO: INITIALS
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return "";

                var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Si solo tiene un nombre
                if (parts.Length == 1)
                    return parts[0].Substring(0, 1).ToUpper();

                // Tomar primeras letras de los dos primeros nombres
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
        }

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

        public string Status { get; set; } // "Paid", "Partial", "Pending"

        public Color StatusColor
        {
            get
            {
                return Status switch
                {
                    "Paid" => Color.FromArgb("#4CAF50"),     //  verde
                    "Partial" => Color.FromArgb("#FFC107"),  //  amarillo
                    "Pending" => Color.FromArgb("#F44336"),  //  rojo
                    _ => Color.FromArgb("#9E9E9E")           // gris default
                };
            }
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
