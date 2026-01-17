namespace Natillera
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        public App(IServiceProvider serviceProvider, AppShell appShell)
        {
            InitializeComponent();

            MainPage = appShell;

            Services = serviceProvider;
        }
    }
}