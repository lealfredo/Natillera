using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Rifa.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public partial class LoansViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public ObservableCollection<LoanItem> Loans { get; set; } = new();
        public ObservableCollection<Participant> Participants { get; set; } = new();

        public Participant SelectedParticipant { get; set; }
        public string BorrowerName { get; set; }
        public string Amount { get; set; }
        public string InterestRate { get; set; }

        public ICommand LoadCommand { get; }
        public ICommand AddLoanCommand { get; }
        public ICommand AddPaymentCommand { get; }

        public LoansViewModel(INatilleraDatabase database)
        {
            _database = database;

            LoadCommand = new Command(async () => await Load());
            AddLoanCommand = new Command(async () => await AddLoan());
            AddPaymentCommand = new Command<LoanItem>(async (l) => await AddPayment(l));
        }

        private async Task Load()
        {
            Loans.Clear();
            Participants.Clear();

            var participants = await _database.GetParticipantsAsync();
            foreach (var p in participants)
                Participants.Add(p);

            var loans = await _database.GetLoansAsync();

            foreach (var loan in loans)
            {
                var payments = await _database.GetPaymentsAsync(loan.Id);

                // 1. Interés mensual
                var monthlyInterest = loan.Amount * loan.InterestRate;

                // 2. Meses transcurridos
                //var months = ((DateTime.Now.Year - loan.StartDate.Year) * 12)
                //           + DateTime.Now.Month - loan.StartDate.Month;

                var days = (DateTime.Now - loan.StartDate).TotalDays;

                // convierte a meses (aprox)
                var months = (int)Math.Ceiling(days / 30.0);

                // mínimo 1 mes
                if (months < 1)
                    months = 1;

                // 3. Interés total generado
                var totalInterestGenerated = monthlyInterest * months;

                // 4. Interés pagado
                var interestPaid = payments.Sum(x => x.InterestPaid);

                // 5. Interés pendiente
                var pendingInterest = totalInterestGenerated - interestPaid;
                if (pendingInterest < 0) pendingInterest = 0;

                // 6. Capital pagado
                var principalPaid = payments.Sum(x => x.PrincipalPaid);

                // 7. Capital pendiente
                var pendingPrincipal = loan.Amount - principalPaid;
                if (pendingPrincipal < 0) pendingPrincipal = 0;

                // 8. Total pagado (informativo)
                var totalPaid = interestPaid + principalPaid;

                // 9. Balance real
                var totalBalance = pendingInterest + pendingPrincipal;

                Loans.Add(new LoanItem
                {
                    Id = loan.Id,
                    Name = loan.BorrowerName,

                    Amount = loan.Amount,
                    InterestRate = loan.InterestRate,

                    MonthlyInterest = monthlyInterest,

                    TotalInterestGenerated = totalInterestGenerated,
                    InterestPaid = interestPaid,
                    PendingInterest = pendingInterest,

                    PrincipalPaid = principalPaid,
                    PendingPrincipal = pendingPrincipal,

                    TotalPaid = totalPaid,
                    Balance = totalBalance,

                    IsPaid = loan.IsPaid
                });
            }
        }

        private async Task AddLoan()
        {
            if (!decimal.TryParse(Amount, out var amount)) return;
            if (!decimal.TryParse(InterestRate, out var rate)) return;

            var loan = new Loan
            {
                PersonId = SelectedParticipant?.Id,
                BorrowerName = SelectedParticipant?.Name ?? BorrowerName,
                Amount = amount,
                InterestRate = rate,
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddMonths(1),
                IsPaid = false
            };

            await _database.AddLoanAsync(loan);

            Amount = string.Empty;
            InterestRate = string.Empty;
            BorrowerName = string.Empty;
            SelectedParticipant = null;

            await Load();
        }

        private async Task AddPayment(LoanItem item)
        {
            if (item == null || item.IsPaid) return;

            string result = await App.Current.MainPage.DisplayPromptAsync(
                "Abono",
                "Ingrese el valor:");

            if (!decimal.TryParse(result, out var value) || value <= 0)
                return;

            // Obtener préstamo real
            var loans = await _database.GetLoansAsync();
            var loan = loans.First(x => x.Id == item.Id);

            var payments = await _database.GetPaymentsAsync(loan.Id);

            // Recalcular interés actual (igual que en Load)
            var monthlyInterest = loan.Amount * loan.InterestRate;

            var days = (DateTime.Now - loan.StartDate).TotalDays;

            // convierte a meses (aprox)
            var months = (int)Math.Ceiling(days / 30.0);

            // mínimo 1 mes
            if (months < 1)
                months = 1;

            var totalInterestGenerated = monthlyInterest * months;

            var interestPaid = payments.Sum(x => x.InterestPaid);
            var pendingInterest = totalInterestGenerated - interestPaid;
            if (pendingInterest < 0) pendingInterest = 0;

            var principalPaid = payments.Sum(x => x.PrincipalPaid);
            var pendingPrincipal = loan.Amount - principalPaid;
            if (pendingPrincipal < 0) pendingPrincipal = 0;

            // DISTRIBUCIÓN DEL PAGO
            decimal payment = value;

            // Primero paga interés
            decimal interestToPay = Math.Min(payment, pendingInterest);
            payment -= interestToPay;

            // Luego capital
            decimal principalToPay = Math.Min(payment, pendingPrincipal);
            payment -= principalToPay;

            // Guardar pago
            await _database.AddPaymentAsync(new LoanPayment
            {
                LoanId = loan.Id,
                Amount = value,
                InterestPaid = interestToPay,
                PrincipalPaid = principalToPay,
                Date = DateTime.Now
            });

            // Actualizar estado del préstamo
            if ((principalPaid + principalToPay) >= loan.Amount)
            {
                loan.IsPaid = true;
                await _database.UpdateLoanAsync(loan);
            }

            await Load();
        }
    }
}
