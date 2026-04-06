using Natillera.Data;
using Natillera.Models;
using Rifa.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Natillera.ViewModels
{
    public class LoanPaymentViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public int LoanId { get; set; }
        public decimal PendingPrincipal { get; set; }

        public ObservableCollection<LoanMonthItem> Months { get; set; } = new();

        public bool PayFullCapital { get; set; }
        public string CapitalAmount { get; set; }

        public ICommand ToggleMonthCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        public bool HasPendingPrincipal => PendingPrincipal > 0;

        public LoanPaymentViewModel(INatilleraDatabase database)
        {
            _database = database;

            ToggleMonthCommand = new Command<LoanMonthItem>(m =>
            {
                if (m.IsPaid) return;
                m.IsSelected = !m.IsSelected;
            });

            ConfirmCommand = new Command(async () => await Confirm());
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task Load(int loanId)
        {
            LoanId = loanId;

            var loan = (await _database.GetLoansAsync()).First(x => x.Id == loanId);
            var payments = await _database.GetPaymentsAsync(loanId);

            var monthlyInterest = loan.PrincipalAmount * (loan.InterestRate / 100);

            Months.Clear();

            // calcular meses transcurridos
            var start = new DateTime(loan.StartDate.Year, loan.StartDate.Month, 1);
            var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            int totalMonths = ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;

            for (int i = 0; i < totalMonths; i++)
            {
                var date = start.AddMonths(i);

                var paid = payments.Any(x =>
                    x.IsInterest &&
                    x.Month == date.Month &&
                    x.Year == date.Year);

                Months.Add(new LoanMonthItem
                {
                    Month = date.Month,
                    Year = date.Year,
                    Name = date.ToString("MMM yyyy"), // mejor UX
                    InterestAmount = monthlyInterest,
                    IsPaid = paid
                });
            }

            // capital pendiente
            var principalPaid = payments
                .Where(x => !x.IsInterest)
                .Sum(x => x.Amount);

            PendingPrincipal = loan.PrincipalAmount - principalPaid;
        }

        private async Task Confirm()
        {
            var loan = (await _database.GetLoansAsync())
                .First(x => x.Id == LoanId);

            var payments = await _database.GetPaymentsAsync(LoanId);

            var selectedMonths = Months
                .Where(x => x.IsSelected && !x.IsPaid)
                .ToList();

            // 1. PAGAR INTERESES
            foreach (var m in selectedMonths)
            {
                await _database.AddPaymentAsync(new LoanPayment
                {
                    LoanId = LoanId,
                    Amount = m.InterestAmount,
                    IsInterest = true,
                    Month = m.Month,
                    Year = m.Year,
                    Date = DateTime.Now
                });
            }

            // 2. CALCULAR CAPITAL PENDIENTE REAL
            var principalPaid = payments
                .Where(x => !x.IsInterest)
                .Sum(x => x.Amount);

            var pendingPrincipal = loan.PrincipalAmount - principalPaid;
            if (pendingPrincipal < 0) pendingPrincipal = 0;

            // 3. DEFINIR MONTO A PAGAR
            decimal capitalToPay = 0;

            if (PayFullCapital)
            {
                capitalToPay = pendingPrincipal;
            }
            else
            {
                if (!decimal.TryParse(CapitalAmount, out capitalToPay) || capitalToPay <= 0)
                    capitalToPay = 0;
            }

            // evitar sobrepago
            capitalToPay = Math.Min(capitalToPay, pendingPrincipal);

            // 4. PAGAR CAPITAL
            if (capitalToPay > 0)
            {
                var totalPrincipal = loan.PrincipalAmount;

                if (totalPrincipal > 0)
                {
                    // proporciones originales
                    var ratioInterest = loan.PrincipalFromInterest / totalPrincipal;
                    var ratioContributions = loan.PrincipalFromContributions / totalPrincipal;

                    // distribución (para dashboard)
                    var toInterestPool = capitalToPay * ratioInterest;
                    var toContributionsPool = capitalToPay * ratioContributions;

                    // aquí podrías guardar estos valores si luego quieres auditoría fina

                    await _database.AddPaymentAsync(new LoanPayment
                    {
                        LoanId = LoanId,
                        Amount = capitalToPay,
                        IsInterest = false,
                        Month = DateTime.Now.Month,
                        Year = DateTime.Now.Year,
                        Date = DateTime.Now
                    });
                }
            }

            // 5. MARCAR PRÉSTAMO COMO PAGADO
            if (pendingPrincipal - capitalToPay <= 0)
            {
                loan.IsPaid = true;
                await _database.UpdateLoanAsync(loan);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
