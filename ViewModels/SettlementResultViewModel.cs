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
    public partial class SettlementResultViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public ObservableCollection<SettlementResultItem> Results { get; set; } = new();

        public ICommand LoadCommand { get; }
        public ICommand ShareCommand { get; }

        public SettlementResultViewModel(INatilleraDatabase database)
        {
            _database = database;

            LoadCommand = new Command(async () => await Load());
            ShareCommand = new Command(async () => await Shared());
        }

        private async Task Load()
        {
            Results.Clear();

            var participants = await _database.GetParticipantsAsync();

            var db = _database.GetConnection(); // agrega esto en el repo
            var settlement = await db.Table<Settlement>()
                                     .OrderByDescending(x => x.Id)
                                     .FirstOrDefaultAsync();

            if (settlement == null) return;

            var details = await db.Table<SettlementDetail>()
                                  .Where(x => x.SettlementId == settlement.Id)
                                  .ToListAsync();

            foreach (var d in details)
            {
                var participant = participants.FirstOrDefault(x => x.Id == d.PersonId);

                Results.Add(new SettlementResultItem
                {
                    Name = participant?.Name ?? "N/A",
                    Contributed = d.TotalContributed,
                    Profit = d.ProfitEarned
                });
            }
        }

        // EXPORTAR A IMAGEN (igual que rifas)
        public VisualElement ExportView { get; set; }

        private async Task Shared()
        {
            if (ExportView == null) return;

            var image = await ExportView.CaptureAsync();
            var stream = await image.OpenReadAsync();

            var fileName = $"liquidacion-{DateTime.Now:yyyyMMddHHmmss}.png";
            var path = Path.Combine(FileSystem.CacheDirectory, fileName);

            using var file = File.OpenWrite(path);
            await stream.CopyToAsync(file);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Liquidación",
                File = new ShareFile(path)
            });
        }
    }
}
