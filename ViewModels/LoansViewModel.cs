using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Natillera.Views;
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
        public DateTime StartDate { get; set; }
        public string Amount { get; set; }
        public string InterestRate { get; set; }

        public ICommand LoadCommand { get; }
        public ICommand AddLoanCommand { get; }
        //public ICommand AddPaymentCommand { get; }
        public ICommand OpenPaymentCommand { get; }

        public LoansViewModel(INatilleraDatabase database)
        {
            _database = database;
            StartDate = DateTime.Now;

            LoadCommand = new Command(async () => await Load());
            AddLoanCommand = new Command(async () => await AddLoan());
            //AddPaymentCommand = new Command<LoanItem>(async (l) => await AddPayment(l));
            OpenPaymentCommand = new Command<LoanItem>(async (l) => await OpenPayment(l));
        }

        private async Task OpenPayment(LoanItem item)
        {
            if (item == null) return;

            var vm = new LoanPaymentViewModel(_database);
            await vm.Load(item.Id);

            if (vm.PendingPrincipal <= 0 && !vm.Months.Any(x => !x.IsPaid))
            {
                await Shell.Current.DisplayAlert(
                    "Información",
                    "No hay pagos pendientes para este préstamo",
                    "OK");

                return;
            }

            var page = new LoanPaymentPage(vm);

            await Shell.Current.Navigation.PushModalAsync(page);
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

                // Interés mensual (%)
                var monthlyInterest = loan.PrincipalAmount * (loan.InterestRate / 100);

                // Meses transcurridos
                var months =
                            (DateTime.Now.Year - loan.StartDate.Year) * 12 +
                            (DateTime.Now.Month - loan.StartDate.Month) + 1;

                if (months < 1)
                    months = 1;

                // Interés total generado
                var totalInterestGenerated = monthlyInterest * months;

                // Interés pagado
                var interestPaid = payments
                    .Where(x => x.IsInterest)
                    .Sum(x => x.Amount);

                var pendingInterest = totalInterestGenerated - interestPaid;
                if (pendingInterest < 0) pendingInterest = 0;

                // Capital pagado
                var principalPaid = payments
                    .Where(x => !x.IsInterest)
                    .Sum(x => x.Amount);

                var pendingPrincipal = loan.PrincipalAmount - principalPaid;
                if (pendingPrincipal < 0) pendingPrincipal = 0;

                var totalPaid = interestPaid + principalPaid;
                var totalBalance = pendingInterest + pendingPrincipal;

                Loans.Add(new LoanItem
                {
                    Id = loan.Id,
                    Name = Participants.FirstOrDefault(x => x.Id == loan.PersonId)?.Name ?? loan.BorrowerName,

                    Amount = loan.PrincipalAmount,
                    InterestRate = loan.InterestRate,

                    MonthlyInterest = monthlyInterest,

                    TotalInterestGenerated = totalInterestGenerated,
                    InterestPaid = interestPaid,
                    PendingInterest = pendingInterest,

                    PrincipalPaid = principalPaid,
                    PendingPrincipal = pendingPrincipal,

                    TotalPaid = totalPaid,
                    Balance = totalBalance,

                    IsPaid = pendingPrincipal <= 0,
                    StartDate = loan.StartDate
                });
            }
        }

        private async Task AddLoan()
        {
            if (!decimal.TryParse(Amount, out var amount)) return;
            if (!decimal.TryParse(InterestRate, out var rate)) return;

            var (availableInterest, availableContributions) =
                await _database.GetAvailableMoney();

            var fromInterest = Math.Min(amount, availableInterest);
            var fromContributions = amount - fromInterest;

            var loan = new Loan
            {
                PersonId = SelectedParticipant?.Id,
                BorrowerName = SelectedParticipant?.Name ?? BorrowerName,
                PrincipalAmount = amount,
                PrincipalFromInterest = fromInterest,
                PrincipalFromContributions = fromContributions,
                InterestRate = rate,
                StartDate = StartDate
            };

            await _database.AddLoanAsync(loan);

            Amount = string.Empty;
            InterestRate = string.Empty;
            SelectedParticipant = null;

            await Load();
        }
    }
}
