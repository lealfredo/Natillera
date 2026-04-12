using CommunityToolkit.Mvvm.ComponentModel;
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

        private bool _hasLoaded;
        public bool HasLoaded
        {
            get => _hasLoaded;
            set
            {
                _hasLoaded = value;
                OnPropertyChanged();
            }
        }

        public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime ToDate { get; set; } = DateTime.Now;

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }
        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();
                if (SetProperty(ref _selectedParticipant, value))
                {
                    if (value != null)
                        LoadCommand.Execute(null);
                }
            }
        }
        private bool _isPersonal;
        public bool IsPersonal
        {
            get => _isPersonal;
            set
            {
                _isPersonal = value;
                OnPropertyChanged();
            }
        }

        private string _borrowerName;
        public string BorrowerName
        {
            get => _borrowerName;
            set
            {
                _borrowerName = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalLoaned;
        public decimal TotalLoaned
        {
            get => _totalLoaned;
            set
            {
                _totalLoaned = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalNatilleraLoaned;
        public decimal TotalNatilleraLoaned
        {
            get => _totalNatilleraLoaned;
            set
            {
                _totalNatilleraLoaned = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalNatilleraPaid;
        public decimal TotalNatilleraPaid
        {
            get => _totalNatilleraPaid;
            set
            {
                _totalNatilleraPaid = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalNatilleraBalance;
        public decimal TotalNatilleraBalance
        {
            get => _totalNatilleraBalance;
            set
            {
                _totalNatilleraBalance = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalPersonalLoaned;
        public decimal TotalPersonalLoaned
        {
            get => _totalPersonalLoaned;
            set
            {
                _totalPersonalLoaned = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalPersonalPaid;
        public decimal TotalPersonalPaid
        {
            get => _totalPersonalPaid;
            set
            {
                _totalPersonalPaid = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalPersonalBalance;
        public decimal TotalPersonalBalance
        {
            get => _totalPersonalBalance;
            set
            {
                _totalPersonalBalance = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalPaid;
        public decimal TotalPaid
        {
            get => _totalPaid;
            set
            {
                _totalPaid = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalBalance;
        public decimal TotalBalance
        {
            get => _totalBalance;
            set
            {
                _totalBalance = value;
                OnPropertyChanged();
            }
        }

        private string _totalLoans;
        public string TotalLoans
        {
            get => _totalLoans;
            set
            {
                _totalLoans = value;
                OnPropertyChanged();
            }
        }

        private string _amount;
        public string Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        private string _interestRate;
        public string InterestRate
        {
            get => _interestRate;
            set
            {
                _interestRate = value;
                OnPropertyChanged();
            }
        }


        [ObservableProperty]
        private int _totalLoansCount;
        [ObservableProperty]
        private int _totalPersonalLoansCount;
        [ObservableProperty]
        private int _totalNatilleraLoansCount;

        private ObservableCollection<Participant> _filteredParticipants;
        public ObservableCollection<Participant> FilteredParticipants
        {
            get => _filteredParticipants;
            set => SetProperty(ref _filteredParticipants, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterParticipants();
            }
        }

        public void FilterParticipants()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                FilteredParticipants = new ObservableCollection<Participant>(Participants);
            else
                FilteredParticipants = new ObservableCollection<Participant>(
                    Participants.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task LoadParticipants()
        {
            var list = await _database.GetParticipantsAsync();
            Participants = new ObservableCollection<Participant>(list);
            FilteredParticipants = new ObservableCollection<Participant>(list);
        }

        private List<LoanItem> _allLoans = new();

        private bool _showPaid;
        public bool ShowPaid
        {
            get => _showPaid;
            set
            {
                _showPaid = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand AddLoanCommand { get; }
        public ICommand DeleteLoanCommand => new Command<LoanItem>(async (l) => await DeleteLoan(l));
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

        private void ApplyFilter()
        {
            var filtered = ShowPaid
                ? _allLoans
                    .OrderBy(x => x.IsPaid)
                    .ThenBy(x => x.Name)        // ordenar por participante
                    .ThenBy(l => l.StartDate)
                    .ToList()
                : _allLoans
                    .Where(x => !x.IsPaid)
                    .OrderBy(x => x.Name)       // ordenar por participante
                    .ThenBy(l => l.StartDate)
                    .ToList();

            Loans.Clear();

            foreach (var l in filtered)
                Loans.Add(l);
        }

        private async Task DeleteLoan(LoanItem item)
        {
            if (item == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Eliminar",
                "¿Seguro que deseas eliminar este préstamo?",
                "Sí",
                "No");

            if (!confirm) return;

            var payments = await _database.GetPaymentsAsync(item.Id);

            if (payments.Any())
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "No puedes eliminar un préstamo con pagos registrados",
                    "OK");
                return;
            }

            await _database.DeleteLoanAsync(item.Id);

            await Load();
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

        public async Task Init()
        {
            await LoadParticipants();
        }

        private async Task Load()
        {
            try
            {
                IsLoading = true;

                Loans.Clear();
                _allLoans.Clear();

                if (SelectedParticipant == null)
                {
                    HasLoaded = false;
                    return;
                }

                var participantsDict = Participants.ToDictionary(x => x.Id, x => x.Name);

                var loans = await _database.GetAllLoansByParticipantAsync(SelectedParticipant.Id);

                TotalLoansCount = loans.Count;
                TotalNatilleraLoansCount = loans.Count(x => !x.IsPersonal);
                TotalPersonalLoansCount = loans.Count(x => x.IsPersonal);

                decimal totalLoaned = 0;
                decimal totalPaidSum = 0;
                decimal totalBalanceSum = 0;

                decimal totalNatilleraLoaned = 0;
                decimal totalNatilleraPaidSum = 0;
                decimal totalNatilleraBalanceSum = 0;

                decimal totalPersonalLoaned = 0;
                decimal totalPersonalPaidSum = 0;
                decimal totalPersonalBalanceSum = 0;

                foreach (var loan in loans)
                {
                    var payments = await _database.GetPaymentsAsync(loan.Id);

                    var monthlyInterest = loan.PrincipalAmount * (loan.InterestRate / 100);

                    var months =
                        (DateTime.Now.Year - loan.StartDate.Year) * 12 +
                        (DateTime.Now.Month - loan.StartDate.Month);

                    if (months < 1)
                        months = 1;

                    var totalInterestGenerated = monthlyInterest * months;

                    var interestPaid = payments
                        .Where(x => x.IsInterest)
                        .Sum(x => x.Amount);

                    var pendingInterest = totalInterestGenerated - interestPaid;
                    if (pendingInterest < 0) pendingInterest = 0;

                    var principalPaid = payments
                        .Where(x => !x.IsInterest)
                        .Sum(x => x.Amount);

                    var pendingPrincipal = loan.PrincipalAmount - principalPaid;
                    if (pendingPrincipal < 0) pendingPrincipal = 0;

                    var totalPaid = interestPaid + principalPaid;
                    var totalBalance = pendingInterest + pendingPrincipal;

                    participantsDict.TryGetValue((int)loan.PersonId, out var participantName);

                    string name = loan.IsPersonal
                        ? $"👤 {participantName ?? loan.BorrowerName}"
                        : participantName ?? loan.BorrowerName;

                    // 🔥 GLOBAL
                    totalLoaned += loan.PrincipalAmount;
                    totalPaidSum += totalPaid;
                    totalBalanceSum += totalBalance;

                    // 🔥 SEPARACIÓN
                    if (loan.IsPersonal)
                    {
                        totalPersonalLoaned += loan.PrincipalAmount;
                        totalPersonalPaidSum += totalPaid;
                        totalPersonalBalanceSum += totalBalance;
                    }
                    else
                    {
                        totalNatilleraLoaned += loan.PrincipalAmount;
                        totalNatilleraPaidSum += totalPaid;
                        totalNatilleraBalanceSum += totalBalance;
                    }

                    _allLoans.Add(new LoanItem
                    {
                        Id = loan.Id,
                        Name = name,

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
                        StartDate = loan.StartDate,
                        IsPersonal = loan.IsPersonal,
                    });
                }

                // TOTALES GENERALES
                TotalLoaned = totalLoaned;
                TotalPaid = totalPaidSum;
                TotalBalance = totalBalanceSum;

                // TOTALES NATILLERA
                TotalNatilleraLoaned = totalNatilleraLoaned;
                TotalNatilleraPaid = totalNatilleraPaidSum;
                TotalNatilleraBalance = totalNatilleraBalanceSum;

                // TOTALES PERSONALES
                TotalPersonalLoaned = totalPersonalLoaned;
                TotalPersonalPaid = totalPersonalPaidSum;
                TotalPersonalBalance = totalPersonalBalanceSum;

                HasLoaded = true;

                ApplyFilter();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddLoan()
        {
            if (SelectedParticipant == null && string.IsNullOrWhiteSpace(BorrowerName))
            {
                await Shell.Current.DisplayAlert("Error", "Seleccione participante o ingrese nombre", "OK");
                return;
            }

            if (!decimal.TryParse(Amount, out var amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Error", "Monto inválido", "OK");
                return;
            }

            if (!decimal.TryParse(InterestRate, out var rate) || rate <= 0)
            {
                await Shell.Current.DisplayAlert("Error", "Interés inválido", "OK");
                return;
            }

            decimal fromInterest = 0;
            decimal fromContributions = 0;
            decimal totalAvailable = 0;
            decimal fromRaffles = 0;

            if (!IsPersonal)
            {
                var (availableInterest, availableContributions, availableFromRaffles) = await _database.GetAvailableMoney();

                // TOTAL DISPONIBLE REAL
                totalAvailable =
                    availableInterest +
                    availableContributions +
                    availableFromRaffles;

                if (amount > totalAvailable)
                {
                    await Shell.Current.DisplayAlert("Error", "No hay suficiente dinero disponible", "OK");
                    return;
                }

                // DISTRIBUCIÓN CORRECTA (3 fuentes)
                var remaining = amount;

                fromInterest = Math.Min(remaining, availableInterest);
                remaining -= fromInterest;

                fromContributions = Math.Min(remaining, availableContributions);
                remaining -= fromContributions;

                fromRaffles = remaining; // lo que quede sale de rifas
            }

            //var (availableInterest, availableContributions) =
            //    await _database.GetAvailableMoney();

            //var totalAvailable = availableInterest + availableContributions;

            //if (amount > totalAvailable)
            //{
            //    await Shell.Current.DisplayAlert("Error", "No hay suficiente dinero disponible", "OK");
            //    return;
            //}

            //var fromInterest = Math.Min(amount, availableInterest);
            //var fromContributions = amount - fromInterest;

            var loan = new Loan
            {
                PersonId = SelectedParticipant?.Id,
                BorrowerName = SelectedParticipant?.Name ?? BorrowerName,
                PrincipalAmount = amount,
                PrincipalFromInterest = fromInterest,
                PrincipalFromContributions = fromContributions,
                InterestRate = rate,
                StartDate = StartDate,
                IsPersonal = IsPersonal,
                PrincipalFromRaffles = fromRaffles
            };

            await _database.AddLoanAsync(loan);

            // LIMPIAR FORMULARIO
            Amount = string.Empty;
            InterestRate = string.Empty;
            BorrowerName = string.Empty;
            SelectedParticipant = null;
            StartDate = DateTime.Now;

            await Load();
        }
    }
}
