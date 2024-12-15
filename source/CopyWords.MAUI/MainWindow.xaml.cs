using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void ButtonSettings_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SettingsPage");
    }
}