using Natillera.Data;
using Natillera.Services;
using Natillera.ViewModels;
using Natillera.Views;

namespace Natillera.Extensions
{
    public static class MauiAppBuilderExtension
    {
        public static void ConfigureNatillera(this MauiAppBuilder builder)
        {
            // Configure services, fonts, etc. here
            var databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "natillera.db3");

            //if (File.Exists(databasePath))
            //{
            //    File.Delete(databasePath);
            //}

            builder.Services.AddSingleton<INatilleraDatabase>(
                _ => new NatilleraDatabase(databasePath));

            builder.Services.AddSingleton<IRaffleService, RaffleService>();
            builder.Services.AddSingleton<IWhatsAppService, WhatsAppService>();

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<RafflePage>();
            builder.Services.AddTransient<BetPage>();
            builder.Services.AddTransient<RaffleWinnersPage>();
            builder.Services.AddTransient<CreateRafflePage>();
            builder.Services.AddTransient<RaffleHistoryPage>();
            builder.Services.AddTransient<BackupPage>();

            builder.Services.AddTransient<RaffleViewModel>();
            builder.Services.AddTransient<BetViewModel>();
            builder.Services.AddTransient<RaffleWinnerViewModel>();
            builder.Services.AddTransient<CreateRaffleViewModel>();
            builder.Services.AddTransient<RaffleHistoryViewModel>();
            builder.Services.AddTransient<BackupViewModel>();
        }
    }
}
