using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Views;

namespace CopyWords.MAUI;

public partial class MainWindow : Window
{
    private readonly IUpdateService _updateService;
    private readonly GetUpdateViewModel _getUpdateViewModel;

    public MainWindow(
        IUpdateService updateService,
        GetUpdateViewModel getUpdateViewModel)
    {
        InitializeComponent();

        _getUpdateViewModel = getUpdateViewModel;
        _updateService = updateService;
    }

    protected override async void OnCreated()
    {
        // todo: also check the setting "check for updates"
        if (await _updateService.IsUpdateAvailableAsync())
        {
            await Navigation.PushModalAsync(new GetUpdatePage
            {
                BindingContext = _getUpdateViewModel
            });
        }
    }

    private async void ButtonSettings_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SettingsPage");
    }
}