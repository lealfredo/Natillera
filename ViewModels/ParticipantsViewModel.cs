using Natillera;
using Natillera.Data;
using Natillera.Entities;
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
        public ObservableCollection<Participant> Participants { get; set; } = new();

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

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public ParticipantsViewModel(INatilleraDatabase database)
        {
            _database = database;

            LoadCommand = new Command(async () => await Load());
            AddCommand = new Command(async () => await Add());
            //DeleteCommand = new Command<Participant>(async (p) => await Delete(p));
        }

        private async Task Load()
        {
            Participants.Clear();
            var list = await _database.GetParticipantsAsync();

            foreach (var item in list)
                Participants.Add(item);
        }

        private async Task Add()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;

            var participant = new Participant
            {
                Name = Name,
                Phone = Phone
            };

            await _database.SaveParticipantAsync(participant);

            Name = string.Empty;
            Phone = string.Empty;

            await Load();
        }

        //private async Task Delete(Participant participant)
        //{
        //    if (participant == null) return;

        //    bool confirm = await App.Current.MainPage.DisplayAlert(
        //        "Eliminar",
        //        $"¿Eliminar a {participant.Name}?",
        //        "Sí",
        //        "No");

        //    if (!confirm) return;

        //    await _database.DeleteParticipantAsync(participant.Id);
        //    Participants.Remove(participant);
        //}
    }
}
