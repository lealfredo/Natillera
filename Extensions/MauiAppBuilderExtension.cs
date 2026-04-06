using Natillera.Data;
using Natillera.Services;
using Natillera.ViewModels;
using Natillera.Views;
using Rifa.ViewModels;

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
            builder.Services.AddTransient<RafflesPage>();
            builder.Services.AddTransient<BetPage>();
            builder.Services.AddTransient<RaffleWinnersPage>();
            builder.Services.AddTransient<CreateRafflePage>();
            builder.Services.AddTransient<RaffleHistoryPage>();
            builder.Services.AddTransient<BackupPage>();
            builder.Services.AddTransient<ParticipantsPage>();
            builder.Services.AddTransient<ContributionsPage>();
            builder.Services.AddTransient<LoansPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<SettlementPage>();
            builder.Services.AddTransient<SettlementResultPage>();
            builder.Services.AddTransient<LoanPaymentPage>();
            builder.Services.AddTransient<ParticipantStatementPage>();

            builder.Services.AddTransient<RaffleViewModel>();
            builder.Services.AddTransient<RafflesViewModel>();
            builder.Services.AddTransient<BetViewModel>();
            builder.Services.AddTransient<RaffleWinnerViewModel>();
            builder.Services.AddTransient<CreateRaffleViewModel>();
            builder.Services.AddTransient<RaffleHistoryViewModel>();
            builder.Services.AddTransient<BackupViewModel>();
            builder.Services.AddTransient<ParticipantsViewModel>();
            builder.Services.AddTransient<ContributionsViewModel>();
            builder.Services.AddTransient<LoansViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ParticipantStatementViewModel>();
            builder.Services.AddTransient<SettlementViewModel>();
            builder.Services.AddTransient<SettlementResultViewModel>();
            builder.Services.AddTransient<LoanPaymentViewModel>();
        }
    }
}
