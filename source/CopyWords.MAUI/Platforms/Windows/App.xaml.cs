// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CopyWords.MAUI.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        UnhandledException += (sender, e) =>
        {
            if (e.Exception is Exception ex)
            {
                Preferences.Default.Set("LastCrashHandled", false);
                Preferences.Default.Set("LastCrashTime", DateTimeOffset.Now.ToUnixTimeMilliseconds());
                Preferences.Default.Set("LastCrashException", ex.GetType().FullName);
                Preferences.Default.Set("LastCrashMessage", ex.Message);
                Preferences.Default.Set("LastCrashStackTrace", ex.StackTrace);
            }
            e.Handled = false;
        };

    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}