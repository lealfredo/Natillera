using Natillera;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Natillera.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public partial class ParticipantsViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        private ObservableCollection<ParticipantItem> _participants;
        public ObservableCollection<ParticipantItem> Participants
        {
            get => _participants;
            set
            {
                _participants = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private decimal _monthlyContribution;
        public decimal MonthlyContribution
        {
            get => _monthlyContribution;
            set { _monthlyContribution = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;

                if (value != null)
                {
                    Name = value.Name;
                    Phone = value.Phone;
                    MonthlyContribution = value.MonthlyContribution;
                    IsEditing = true;
                }

                OnPropertyChanged();
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SelectParticipantCommand { get; }
        public ICommand CancelEditCommand { get; }

        public ParticipantsViewModel(INatilleraDatabase database)
        {
            _database = database;
            Participants = new();
            LoadCommand = new Command(async () => await Load());
            AddCommand = new Command(async () => await Add());
            DeleteCommand = new Command<ParticipantItem>(async (p) => await Delete(p));
            CancelEditCommand = new Command(CancelEdit);
            SelectParticipantCommand = new Command<ParticipantItem>(p =>
            {
                foreach (var item in Participants)
                    item.IsSelected = false;

                p.IsSelected = true;

                SelectedParticipant = p.Model; // IMPORTANTE

                Name = p.Name;
                Phone = p.Phone;
                MonthlyContribution = p.MonthlyContribution;

                IsEditing = true;
            });
        }

        private void CancelEdit()
        {
            foreach (var item in Participants)
                item.IsSelected = false;

            Name = string.Empty;
            Phone = string.Empty;
            MonthlyContribution = 0;

            SelectedParticipant = null;
            IsEditing = false;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(MonthlyContribution));
            OnPropertyChanged(nameof(IsEditing));
        }

        private async Task Load()
        {
            Participants.Clear();

            var data = await _database.GetParticipantsAsync();

            Participants = new ObservableCollection<ParticipantItem>(
                data.Select(x => new ParticipantItem
                {
                    Model = x
                }));
        }

        private async Task Add()
        {
            if (string.IsNullOrEmpty(Name))
                return;

            if (IsEditing && SelectedParticipant != null)
            {
                // EDITAR
                SelectedParticipant.Name = Name;
                SelectedParticipant.Phone = Phone;
                SelectedParticipant.MonthlyContribution = MonthlyContribution;

                await _database.SaveParticipantAsync(SelectedParticipant);
            }
            else
            {
                // CREAR
                var newItem = new Participant
                {
                    Name = Name,
                    Phone = Phone,
                    MonthlyContribution = MonthlyContribution
                };

                await _database.SaveParticipantAsync(newItem);
            }

            //var participant = new Participant
            //{
            //    Name = Name,
            //    Phone = Phone,
            //    MonthlyContribution = MonthlyContribution
            //};

            //await _database.SaveParticipantAsync(participant);

            ClearForm();

            await Load();
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Phone = string.Empty;
            MonthlyContribution = 0;

            SelectedParticipant = null;
            IsEditing = false;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(MonthlyContribution));
            OnPropertyChanged(nameof(IsEditing));
        }

        private async Task Delete(ParticipantItem participant)
        {
            if (participant == null) return;

            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Eliminar",
                $"¿Eliminar a {participant.Name}?",
                "Sí",
                "No");

            if (!confirm) return;

            await _database.DeleteParticipantAsync(participant.Id);
            Participants.Remove(participant);
        }
    }
}
