using System.Diagnostics;
using System.Runtime.InteropServices;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Views;

namespace CopyWords.MAUI;

public partial class MainWindow : Window
{
    private readonly IUpdateService _updateService;
    private readonly IDialogService _dialogService;
    private readonly GetUpdateViewModel _getUpdateViewModel;

    public MainWindow(
        IUpdateService updateService,
        IDialogService dialogService,
        GetUpdateViewModel getUpdateViewModel)
    {
        InitializeComponent();

        _updateService = updateService;
        _dialogService = dialogService;
        _getUpdateViewModel = getUpdateViewModel;
    }

    protected override async void OnCreated()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && await _updateService.IsUpdateAvailableAsync(AppInfo.VersionString))
            {
                await Navigation.PushModalAsync(new GetUpdatePage
                {
                    BindingContext = _getUpdateViewModel
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occurred while checking for the updates. Error: " + ex);

            _getUpdateViewModel.ErrorMessage = ex.Message;
            await Navigation.PushModalAsync(new CannotCheckUpdates
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