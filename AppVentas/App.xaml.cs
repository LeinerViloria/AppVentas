
namespace AppVentas
{
    public partial class App : Application
    {
        internal static string? UserToken { get; set; }
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
