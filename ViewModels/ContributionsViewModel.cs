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
        public ObservableCollection<MonthItem> Months { get; set; } = new();

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();

                if (value != null)
                    _ = LoadPaidMonths(value.Id); // aquí ocurre la magia
            }
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
        public ICommand ToggleMonthCommand { get; }

        public ContributionsViewModel(INatilleraDatabase database)
        {
            _database = database;

            LoadCommand = new Command(async () => await Load());
            AddCommand = new Command(async () => await Add());
            DeleteCommand = new Command<ContributionItem>(async (c) => await Delete(c));
            ToggleMonthCommand = new Command<MonthItem>(m =>
            {
                if (m.IsPaid) return;

                m.IsSelected = !m.IsSelected;
            });

            Months = new ObservableCollection<MonthItem>
            {
                new () { Month = 1, Name = "Enero", IsPaid = false, IsSelected = false },
                new () { Month = 2, Name = "Febrero", IsPaid = false, IsSelected = false },
                new() {Month = 3, Name = "Marzo", IsPaid = false, IsSelected = false},
                new() {Month = 4, Name = "Abril", IsPaid = false, IsSelected = false},
                new() {Month = 5, Name = "Mayo", IsPaid = false, IsSelected = false},
                new() {Month = 6, Name = "Junio", IsPaid = false, IsSelected = false},
                new() {Month = 7, Name = "Julio", IsPaid = false, IsSelected = false},
                new() {Month = 8, Name = "Agosto", IsPaid = false, IsSelected = false},
                new() {Month = 9, Name = "Septiembre", IsPaid = false, IsSelected = false},
                new() {Month = 10, Name = "Octubre", IsPaid = false, IsSelected = false},
                new() {Month = 11, Name = "Noviembre", IsPaid = false, IsSelected = false},
                new() {Month = 12, Name = "Diciembre", IsPaid = false, IsSelected = false},
            };
        }

        private async Task Load()
        {
            foreach (var m in Months)
            {
                m.IsSelected = false;
                m.IsPaid = false;
            }

            Contributions.Clear();
            Participants.Clear();

            var participants = await _database.GetParticipantsAsync();
            foreach (var p in participants)
                Participants.Add(p);

            //var list = await _database.GetAllContributionsAsync();

            //foreach (var item in list)
            //{
            //    var participant = participants.FirstOrDefault(x => x.Id == item.PersonId);

            //    Contributions.Add(new ContributionItem
            //    {
            //        Id = item.Id,
            //        ParticipantId = item.PersonId,
            //        Name = participant?.Name ?? "N/A",
            //        Amount = item.Amount,
            //        Date = item.Date,
            //        Month = item.Month,
            //        Year = item.Year,
            //    });
            //}
        }

        private async Task LoadPaidMonths(int participantId)
        {
            Contributions.Clear();
            var contributions = await _database.GetContributionsByParticipant(participantId);
            var participant = Participants.FirstOrDefault(x => x.Id == participantId);

            foreach (var item in contributions)
            {

                Contributions.Add(new ContributionItem
                {
                    Id = item.Id,
                    ParticipantId = item.PersonId,
                    Name = participant?.Name ?? "N/A",
                    Amount = item.Amount,
                    Date = item.Date,
                    Month = item.Month,
                    Year = item.Year,
                });
            }

            foreach (var month in Months)
            {
                month.IsPaid = contributions.Any(c => c.Month == month.Month);

                // marcar visualmente
                month.IsSelected = month.IsPaid;
            }
        }

        private async Task Add()
        {
            var selectedMonths = Months.Where(x => x.IsSelected).ToList();

            if (!selectedMonths.Any())
            {
                await Shell.Current.DisplayAlert("Error", "Seleccione al menos un mes", "OK");
                return;
            }

            if (SelectedParticipant == null)
            {
                await Shell.Current.DisplayAlert("Error", "Seleccione participante", "OK");
                return;
            }

            if (Amount == null && SelectedParticipant.MonthlyContribution == 0)
                return;

            Amount = Amount == null ? SelectedParticipant.MonthlyContribution.ToString() : Amount;

            if (!decimal.TryParse(Amount, out decimal value))
                return;

            var setting = await _database.GetSettingAsync();

            if (setting == null) return;

            if (setting.MinimumContribution > value)
            {
                await Shell.Current.DisplayAlert("Error", $"El monto debe ser mayo que {setting.MinimumContribution.ToString("C0")}", "OK");
                return;
            }


            foreach (var m in selectedMonths)
            {
                var exists = await _database.ExistsContribution(SelectedParticipant.Id, DateTime.Now.Year, m.Month);

                if (exists)
                    continue;

                var contribution = new Contribution
                {
                    PersonId = SelectedParticipant.Id,
                    Month = m.Month,
                    Year = DateTime.Now.Year,
                    Amount = value,
                    Date = DateTime.Now
                };

                await _database.AddContributionAsync(contribution);
            }

            Amount = null;
            SelectedParticipant = null;
            foreach (var item in Months)
                item.IsSelected = false;

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
