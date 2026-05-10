using CommunityToolkit.Mvvm.ComponentModel;
using Natillera.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Natillera.ViewModels
{
    [QueryProperty(nameof(Receipt), "Receipt")]
    public partial class ContributionReceiptViewModel : BaseViewModel
    {
        public ContributionReceipt Receipt
        {
            set => Load(value);
        }

        [ObservableProperty]
        private string participantName;

        [ObservableProperty]
        private DateTime date;

        public ObservableCollection<ContributionDetail> Details { get; set; } = new();

        public decimal Total => Details.Sum(x => x.Amount);

        public ContributionReceiptViewModel()
        {
        }

        public void Load(ContributionReceipt receipt)
        {
            ParticipantName = receipt.ParticipantName;
            Date = receipt.Date;

            Details.Clear();

            foreach (var d in receipt.Details)
            {
                Details.Add(new ContributionDetail
                {
                    MonthName = d.MonthName,
                    Year = d.Year,
                    Amount = d.Amount
                });
            }

            OnPropertyChanged(nameof(Total));
        }
    }
}
