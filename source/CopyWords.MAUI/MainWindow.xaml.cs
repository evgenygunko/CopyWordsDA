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
    private readonly IPreferences _preferences;
    private readonly GetUpdateViewModel _getUpdateViewModel;
    private readonly LastCrashViewModel _lastCrashViewModel;

    public MainWindow(
        IUpdateService updateService,
        IDialogService dialogService,
        IPreferences preferences,
        GetUpdateViewModel getUpdateViewModel,
        LastCrashViewModel lastCrashViewModel)
    {
        InitializeComponent();

        _updateService = updateService;
        _dialogService = dialogService;
        _preferences = preferences;
        _getUpdateViewModel = getUpdateViewModel;
        _lastCrashViewModel = lastCrashViewModel;
    }

    protected override async void OnCreated()
    {
        bool isLastCrashHandled = _preferences.Get("LastCrashHandled", false);
        string lastCrashMessage = _preferences.Get("LastCrashMessage", string.Empty);

        if (!isLastCrashHandled && !string.IsNullOrEmpty(lastCrashMessage))
        {
            await Navigation.PushModalAsync(new LastCrashPage
            {
                BindingContext = _lastCrashViewModel
            });
        }

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
        try
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }
        catch (Exception ex)
        {
            await _dialogService.DisplayAlert("Cannot open Settings Page", $"Exception occurred: " + ex.ToString(), "OK");
        }
    }
}