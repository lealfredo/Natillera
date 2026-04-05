using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;
using System.Collections.ObjectModel;

namespace Natillera.ViewModels
{
    [QueryProperty(nameof(SelectedNumber), "number")]
    [QueryProperty(nameof(RaffleId), "raffleId")]
    public partial class BetViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public BetViewModel(INatilleraDatabase database)
        {
            _database = database;
            Title = "Registrar Apuesta";
        }
        public Participant SelectedParticipant { get; set; }
        public ObservableCollection<Participant> Participants { get; set; } = new();

        [ObservableProperty]
        private string bettor;

        [ObservableProperty]
        private string participantPhone;

        [ObservableProperty]
        private BetType selectedBetType;

        [ObservableProperty]
        private string selectedNumber;

        [ObservableProperty]
        private int raffleId;

        private bool _isNewParticipant = true;
        public bool IsNewParticipant
        {
            get => _isNewParticipant;
            set => SetProperty(ref _isNewParticipant, value);
        }

        [RelayCommand]
        public async Task Load()
        {
            Participants.Clear();

            var participants = await _database.GetParticipantsAsync();
            foreach (var p in participants)
                Participants.Add(p);
        }

        [RelayCommand]
        public async Task SaveBetAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrEmpty(Bettor) && SelectedParticipant == null) return;

            IsBusy = true;

            var raffle = await _database.GetRaffleByIdAsync(RaffleId);
            if (raffle == null)
                return;

            //var exists = await _database.ExistsBetForNumberAsync(SelectedNumber, raffle.Id);
            //if (exists)
            //{
            //    await Shell.Current.DisplayAlert("Número ocupado", "Este número ya fue tomado", "OK");
            //    return;
            //}

            var betTypes = new[] { BetType.Start, BetType.Middle, BetType.End };

            foreach (var type in betTypes)
            {
                var bet = new Bet
                {
                    ParticipantId = SelectedParticipant?.Id,
                    Bettor = SelectedParticipant?.Name ?? Bettor,
                    Number = SelectedNumber,
                    Type = type,
                    CreatedAt = DateTime.Now,
                    RaffleWeekId = raffle.Id
                };

                await _database.SaveBetAsync(bet);
            }

            IsBusy = false;

            await Shell.Current.GoToAsync("..");
        }

        //public async Task CheckParticipantByPhoneAsync()
        //{
        //    if (string.IsNullOrWhiteSpace(ParticipantPhone))
        //        return;

        //    var participant = await _database.GetParticipantByPhoneAsync(ParticipantPhone);

        //    if (participant != null)
        //    {
        //        ParticipantName = participant.Name;
        //        IsNewParticipant = false;
        //        OnPropertyChanged(nameof(ParticipantName));
        //    }
        //    else
        //    {
        //        ParticipantName = string.Empty;
        //        IsNewParticipant = true;
        //        OnPropertyChanged(nameof(ParticipantName));
        //    }
        //}
    }
}
