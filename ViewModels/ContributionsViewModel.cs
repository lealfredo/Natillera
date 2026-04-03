using Natillera;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Natillera.ViewModels;
using Rifa.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Rifa.ViewModels
{
    public partial class ContributionsViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public ObservableCollection<ContributionItem> Contributions { get; set; } = new();
        public ObservableCollection<Participant> Participants { get; set; } = new();

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set { _selectedParticipant = value; OnPropertyChanged(); }
        }

        private string _amount;
        public string Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public ContributionsViewModel(INatilleraDatabase database)
        {
            _database = database;

            LoadCommand = new Command(async () => await Load());
            AddCommand = new Command(async () => await Add());
            DeleteCommand = new Command<ContributionItem>(async (c) => await Delete(c));
        }

        private async Task Load()
        {
            Contributions.Clear();
            Participants.Clear();

            var participants = await _database.GetParticipantsAsync();
            foreach (var p in participants)
                Participants.Add(p);

            var list = await _database.GetAllContributionsAsync();

            foreach (var item in list)
            {
                var participant = participants.FirstOrDefault(x => x.Id == item.PersonId);

                Contributions.Add(new ContributionItem
                {
                    Id = item.Id,
                    ParticipantId = item.PersonId,
                    Name = participant?.Name ?? "N/A",
                    Amount = item.Amount,
                    Date = item.Date
                });
            }
        }

        private async Task Add()
        {
            if (SelectedParticipant == null)
                return;

            if (!decimal.TryParse(Amount, out decimal value) || value <= 0)
                return;

            var setting = await _database.GetSettingAsync();

            if (setting == null) return;

            if (setting.MinimumContribution > value) return;

            var contribution = new Contribution
            {
                PersonId = SelectedParticipant.Id,
                Amount = value,
                Date = DateTime.Now
            };

            await _database.AddContributionAsync(contribution);

            Amount = string.Empty;
            SelectedParticipant = null;

            await Load();
        }

        private async Task Delete(ContributionItem item)
        {
            if (item == null) return;

            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Eliminar",
                "¿Eliminar este aporte?",
                "Sí",
                "No");

            if (!confirm) return;

            await _database.DeleteContributionAsync(new Contribution { Id = item.Id });

            Contributions.Remove(item);
        }
    }
}
