using Natillera.Views;

namespace Natillera
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("BetPage", typeof(BetPage));
            Routing.RegisterRoute("RafflePage", typeof(RafflePage));
            Routing.RegisterRoute("RaffleWinnersPage", typeof(RaffleWinnersPage));
            Routing.RegisterRoute("CreateRafflePage", typeof(CreateRafflePage));
            Routing.RegisterRoute("RaffleHistoryPage", typeof(RaffleHistoryPage));
            Routing.RegisterRoute("BackupPage", typeof(BackupPage));
        }
    }
}
