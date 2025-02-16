using Microsoft.Maui.Controls;

namespace DeadLockApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Shell.SetBackgroundColor(this, Color.FromArgb("#23221e"));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(BuildsPage), typeof(BuildsPage));
            Routing.RegisterRoute(nameof(ItemDetailsPage), typeof(ItemDetailsPage));
            Routing.RegisterRoute(nameof(BuildDetailsPage), typeof(BuildDetailsPage));
            Routing.RegisterRoute(nameof(BuildCreatePage), typeof(BuildCreatePage));
            Routing.RegisterRoute(nameof(BuildEditPage), typeof(BuildEditPage));
            Routing.RegisterRoute(nameof(ItemSelectionPage), typeof(ItemSelectionPage));
        }
    }
}
