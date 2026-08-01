using Natillera;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Natillera.ViewModels;
using Natillera.Views;
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
                OnPropertyChanged(nameof(SelectedParticipant));

                IsParticipantSelected = _selectedParticipant != null;

                if (value != null)
                    _ = LoadPaidMonths(value.Id); // aquí ocurre la magia
            }
        }

        private bool isParticipantSelected;
        public bool IsParticipantSelected
        {
            get => isParticipantSelected;
            set
            {
                isParticipantSelected = value;
                OnPropertyChanged(nameof(IsParticipantSelected));
            }
        }

        private string _amount;
        public string Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

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
                // Si ya está completo, no hace nada
                if (!m.IsEnabled)
                {
                    Application.Current.MainPage.DisplayAlert("Info", "Mes ya completado", "OK");
                    return; 
                }

                m.IsSelected = !m.IsSelected;
            });

            Months = new ObservableCollection<MonthItem>
            {
                new () { MonthNumber = 1, Name = "Enero",   IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 2, Name = "Febrero", IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 3, Name = "Marzo",   IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 4, Name = "Abril",   IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 5, Name = "Mayo",    IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 6, Name = "Junio",   IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 7, Name = "Julio",   IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 8, Name = "Agosto",  IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 9, Name = "Septiembre", IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 10, Name = "Octubre", IsSelected = false, IsEnabled = true },
                new () { MonthNumber = 11, Name = "Noviembre", IsSelected = false, IsEnabled = true },
                //new () { MonthNumber = 12, Name = "Diciembre", IsSelected = false, IsEnabled = false },
            };
        }

        private async Task Load()
        {
            foreach (var m in Months)
            {
                m.IsSelected = false;
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
            var participant = FilteredParticipants.FirstOrDefault(x => x.Id == participantId);

            decimal cuota = participant?.MonthlyContribution ?? 0;

            // Llenas lista de movimientos
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

            // Lógica clave por mes
            foreach (var month in Months)
            {
                // Filtrar por el mes actual del loop
                var pagosPorMes = contributions
                    .GroupBy(x => new { x.Year, x.Month })
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

                var key = new { Year = month.Year, Month = month.MonthNumber };

                decimal totalPagadoMes = pagosPorMes.ContainsKey(key)
                    ? pagosPorMes[key]
                    : 0;

                string state;

                if (totalPagadoMes == 0)
                    state = "Pending";
                else if (totalPagadoMes < cuota)
                    state = "Partial";
                else
                    state = "Paid";

                month.State = state;

                // Solo marcar seleccionado si tiene algo pago
                month.IsSelected = totalPagadoMes > 0;

                // NUEVO: control del botón
                month.IsEnabled = totalPagadoMes < cuota;

                // BONUS: progreso (muy útil para UI)
                month.Progress = cuota == 0 ? 0 : (double)(totalPagadoMes / cuota);
                month.PaidAmount = totalPagadoMes;
            }
        }

        private async Task Add()
        {
            try
            {
                var selectedMonths = Months
                    .Where(x => x.IsSelected && x.IsEnabled)
                    .ToList();

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

                decimal cuota = SelectedParticipant?.MonthlyContribution ?? 0;

                if (cuota <= 0)
                {
                    await Shell.Current.DisplayAlert("Error", "El participante no tiene cuota definida", "OK");
                    return;
                }

                // Intentar leer monto
                decimal montoIngresado = 0;
                decimal.TryParse(Amount, out montoIngresado);

                var year = DateTime.Now.Year;

                var receipt = new ContributionReceipt
                {
                    ParticipantName = SelectedParticipant.Name,
                    Date = DateTime.Now
                };

                // CASO 1: SIN MONTO → pagar completo cada mes
                if (montoIngresado == 0)
                {
                    foreach (var m in selectedMonths)
                    {
                        var restante = cuota - m.PaidAmount;

                        if (restante <= 0)
                            continue;

                        await _database.AddContributionAsync(new Contribution
                        {
                            PersonId = SelectedParticipant.Id,
                            Month = m.MonthNumber,
                            Year = year,
                            Amount = restante,
                            Date = DateTime.Now
                        });

                        // AGREGAR AL RECIBO
                        receipt.Details.Add(new ContributionDetail
                        {
                            MonthName = m.Name,
                            Year = year,
                            Amount = restante
                        });
                    }
                }
                else
                {
                    // CASO 2: CON MONTO → distribuir
                    decimal montoRestante = montoIngresado;

                    foreach (var m in selectedMonths)
                    {
                        if (montoRestante <= 0)
                            break;

                        var restanteMes = cuota - m.PaidAmount;

                        if (restanteMes <= 0)
                            continue;

                        if (restanteMes < montoRestante)
                        {
                            await Shell.Current.DisplayAlert("Error", $"Esta pagando mas de lo que debe para el mes {m.Name}", "OK");
                            return;
                        }

                        var pago = Math.Min(montoRestante, restanteMes);

                        await _database.AddContributionAsync(new Contribution
                        {
                            PersonId = SelectedParticipant.Id,
                            Month = m.MonthNumber,
                            Year = year,
                            Amount = pago,
                            Date = DateTime.Now
                        });

                        // AGREGAR AL RECIBO
                        receipt.Details.Add(new ContributionDetail
                        {
                            MonthName = m.Name,
                            Year = year,
                            Amount = pago
                        });

                        montoRestante -= pago;
                    }
                }

                // refrescar antes de limpiar
                await LoadPaidMonths(SelectedParticipant.Id);

                // limpiar UI
                Amount = null;

                foreach (var item in Months)
                    item.IsSelected = false;

                if (receipt.Details.Any())
                {
                    await Shell.Current.GoToAsync(nameof(ContributionReceiptPage), new Dictionary<string, object>
                    {
                        { "Receipt", receipt }
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"{ex.Message}", "OK");
                return;
            }
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
