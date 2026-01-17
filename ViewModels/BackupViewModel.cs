using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natillera.Data;
using Natillera.Entities;
using Natillera.Models;
using Natillera.Utilities;
using System.Text.Json;

namespace Natillera.ViewModels
{
    public partial class BackupViewModel : BaseViewModel
    {
        private readonly INatilleraDatabase _database;

        public BackupViewModel(INatilleraDatabase database)
        {
            _database = database;
        }


        [ObservableProperty]
        private Setting setting;

        public async Task GetSetting()
        {
            Setting = await _database.GetSettingAsync() ?? new Setting();
        }

        [RelayCommand]
        private async Task SaveSettingAsync()
        {
            if (Setting == null || string.IsNullOrEmpty(Setting.WhatsAppNumber) || string.IsNullOrEmpty(Setting.ContactName))
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "Ingrese los datos",
                    "OK");

                return;
            }

            var result = await _database.SaveSettingAsync(Setting);

            await Shell.Current.DisplayAlert("Éxito", "Configuiración guardada", "OK");
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            var backup = new NatilleraBackup
            {
                Raffles = await _database.GetAllRaffleWeek(),
                Participants = await _database.GetAllParticipant(),
                Bets = await _database.GetAllBet(),
                Winners = await _database.GetAllRaffleWinner(),
                Setting = await _database.GetSettingAsync(),
            };

            var json = JsonSerializer.Serialize(
                backup,
                new JsonSerializerOptions { WriteIndented = true });

            var filePath = Path.Combine(
                FileSystem.CacheDirectory,
                $"natillera_backup_{DateTime.Now:yyyyMMddHHmmss}.json");

            File.WriteAllText(filePath, json);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Respaldo Natillera",
                File = new ShareFile(filePath)
            });
        }

        [RelayCommand]
        private async Task ImportAsync(string filePath)
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Importar respaldo",
                "Esto reemplazará todos los datos actuales. ¿Deseas continuar?",
                "Sí", "Cancelar");

            if (!confirm) return;

            try
            {
                var result = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Selecciona el respaldo",
                        FileTypes = CustomFileTypes.Json
                    });

                if (result == null)
                    return;

                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var backup = JsonSerializer.Deserialize<NatilleraBackup>(json);

                if (backup == null)
                    throw new Exception("Backup no compatible");

                if (backup.Version != 1)
                    throw new Exception("Versión no compatible");

                if (backup == null) return;

                //Recomendado: limpiar antes
                await _database.ClearAllAsync();

                await _database.SaveRaffleWeekRangeAsync(backup.Raffles);
                await _database.SaveParticipantRangeAsync(backup.Participants);
                await _database.SaveBetRangeAsync(backup.Bets);
                await _database.SaveRaffleWinnerRangeAsync(backup.Winners);
                await _database.SaveSettingAsync(backup.Setting);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "No se pudo importar el archivo",
                    "OK");
            }
        }
    }
}
