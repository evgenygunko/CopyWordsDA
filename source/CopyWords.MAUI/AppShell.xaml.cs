using CopyWords.MAUI.Views;

namespace CopyWords.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}

