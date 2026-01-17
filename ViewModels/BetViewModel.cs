using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Natillera.ViewModels
{
    [QueryProperty(nameof(SelectedNumber), "number")]
    public partial class BetViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public BetViewModel(INatilleraDatabase database)
        {
            _database = database;
            Title = "Registrar Apuesta";
        }

        [ObservableProperty]
        private string participantName;

        [ObservableProperty]
        private string participantPhone;

        [ObservableProperty]
        private BetType selectedBetType;

        [ObservableProperty]
        private string selectedNumber;

        private bool _isNewParticipant = true;
        public bool IsNewParticipant
        {
            get => _isNewParticipant;
            set => SetProperty(ref _isNewParticipant, value);
        }

        [RelayCommand]
        public async Task SaveBetAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(ParticipantName) ||
                string.IsNullOrWhiteSpace(SelectedNumber) ||
                SelectedNumber.Length != 2)
                return;

            IsBusy = true;

            var exists = await _database.ExistsBetForNumberAsync(SelectedNumber);
            if (exists)
            {
                await Shell.Current.DisplayAlert(
                    "Número ocupado",
                    "Este número ya fue tomado",
                    "OK");
                return;
            }

            var participant = await _database.GetParticipantByPhoneAsync(ParticipantPhone);

            if (participant == null)
            {
                participant = new Participant
                {
                    Name = ParticipantName,
                    Phone = ParticipantPhone
                };

                await _database.SaveParticipantAsync(participant);
            }

            var raffle = await _database.GetCurrentRaffleAsync();
            if (raffle == null)
                return;

            var betTypes = new[] { BetType.Start, BetType.Middle, BetType.End };

            foreach (var type in betTypes)
            {
                var bet = new Bet
                {
                    ParticipantId = participant.Id,
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

        public async Task CheckParticipantByPhoneAsync()
        {
            if (string.IsNullOrWhiteSpace(ParticipantPhone))
                return;

            var participant = await _database.GetParticipantByPhoneAsync(ParticipantPhone);

            if (participant != null)
            {
                ParticipantName = participant.Name;
                IsNewParticipant = false;
                OnPropertyChanged(nameof(ParticipantName));
            }
            else
            {
                ParticipantName = string.Empty;
                IsNewParticipant = true;
                OnPropertyChanged(nameof(ParticipantName));
            }
        }
    }
}
