using Natillera.Views;

namespace Natillera
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("BetPage", typeof(BetPage));
            Routing.RegisterRoute("RafflesPage", typeof(RafflesPage));
            Routing.RegisterRoute("RafflePage", typeof(RafflePage));
            Routing.RegisterRoute("RaffleWinnersPage", typeof(RaffleWinnersPage));
            Routing.RegisterRoute("CreateRafflePage", typeof(CreateRafflePage));
            Routing.RegisterRoute("RaffleHistoryPage", typeof(RaffleHistoryPage));
            Routing.RegisterRoute("BackupPage", typeof(BackupPage));
            Routing.RegisterRoute("ParticipantsPage", typeof(ParticipantsPage));
            Routing.RegisterRoute("ContributionsPage", typeof(ContributionsPage));
            Routing.RegisterRoute("LoansPage", typeof(LoansPage));
            Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
            Routing.RegisterRoute("SettlementPage", typeof(SettlementPage));
            Routing.RegisterRoute("SettlementResultPage", typeof(SettlementResultPage));
        }
    }
}
