using Natillera.Data;
using Rifa.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public partial class SettlementViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public ICommand CloseYearCommand { get; }

        public SettlementViewModel(INatilleraDatabase database)
        {
            _database = database;

            CloseYearCommand = new Command(async () => await CloseYear());
        }

        private async Task CloseYear()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Cerrar año",
                "¿Seguro que deseas liquidar?",
                "Sí",
                "No");

            if (!confirm) return;

            var contributions = await _database.GetAllContributionsAsync();
            var loans = await _database.GetLoansAsync();
            var participants = await _database.GetParticipantsAsync();

            var allPayments = new List<LoanPayment>();

            foreach (var loan in loans)
            {
                var payments = await _database.GetPaymentsAsync(loan.Id);
                allPayments.AddRange(payments);
            }

            var setting = await _database.GetSettingAsync();

            // CAPITAL INICIAL (aportes)
            var initialCapital = contributions.Sum(x => x.Amount);

            // INTERESES GANADOS
            var totalInterest = allPayments
                .Where(x => x.IsInterest)
                .Sum(x => x.Amount);

            // TOTAL PRESTADO
            var totalLoaned = loans.Sum(x => x.PrincipalAmount);

            // CAPITAL RECUPERADO
            var totalRecovered = allPayments
                .Where(x => !x.IsInterest)
                .Sum(x => x.Amount);

            // CAPITAL FINAL
            var finalCapital =
                initialCapital
                + totalInterest
                - (totalLoaned - totalRecovered);

            // UTILIDAD
            var profit = finalCapital - initialCapital;

            // TOPE CONFIGURADO
            var participantShare = initialCapital * setting.MaxReturnPercentage;

            // VALIDACIÓN (IMPORTANTE)
            if (participantShare > profit)
                participantShare = profit;

            var adminShare = profit - participantShare;

            // GUARDAR LIQUIDACIÓN
            var settlement = new Settlement
            {
                EndDate = DateTime.Now,
                InitialCapital = initialCapital,
                FinalCapital = finalCapital,
                Profit = profit,
                ParticipantShare = participantShare,
                AdminShare = adminShare
            };

            await _database.AddSettlementAsync(settlement);

            // REPARTO POR PERSONA
            foreach (var p in participants)
            {
                var totalContributed = contributions
                    .Where(x => x.PersonId == p.Id)
                    .Sum(x => x.Amount);

                if (totalContributed == 0) continue;

                var percentage = totalContributed / initialCapital;

                var profitEarned = percentage * participantShare;

                await _database.AddDetailAsync(new SettlementDetail
                {
                    SettlementId = settlement.Id,
                    PersonId = p.Id,
                    TotalContributed = totalContributed,
                    ParticipationPercentage = percentage,
                    ProfitEarned = profitEarned
                });
            }

            await App.Current.MainPage.DisplayAlert(
                "Listo",
                "Liquidación realizada correctamente",
                "OK");
        }
    }
}
