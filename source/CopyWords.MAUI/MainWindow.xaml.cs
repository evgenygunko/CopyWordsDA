using System.Diagnostics;
using System.Runtime.InteropServices;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Views;

namespace CopyWords.MAUI;

public partial class MainWindow : Window
{
    private readonly IUpdateService _updateService;
    private readonly IDialogService _dialogService;
    private readonly IPreferences _preferences;
    private readonly IGlobalSettings _globalSettings;
    private readonly ILaunchDarklyService _launchDarklyService;

    private readonly GetUpdateViewModel _getUpdateViewModel;
    private readonly LastCrashViewModel _lastCrashViewModel;

    public MainWindow(
        IUpdateService updateService,
        IDialogService dialogService,
        IPreferences preferences,
        IGlobalSettings globalSettings,
        ILaunchDarklyService launchDarklyService,
        GetUpdateViewModel getUpdateViewModel,
        LastCrashViewModel lastCrashViewModel)
    {
        InitializeComponent();

        _updateService = updateService;
        _dialogService = dialogService;
        _preferences = preferences;
        _globalSettings = globalSettings;
        _launchDarklyService = launchDarklyService;

        _getUpdateViewModel = getUpdateViewModel;
        _lastCrashViewModel = lastCrashViewModel;
    }

    protected override async void OnCreated()
    {
        bool isLastCrashHandled = _preferences.Get("LastCrashHandled", false);
        string lastCrashMessage = _preferences.Get("LastCrashMessage", string.Empty);

        // Check for last crash and show the LastCrashPage if needed.
        if (!isLastCrashHandled && !string.IsNullOrEmpty(lastCrashMessage))
        {
            await Navigation.PushModalAsync(new LastCrashPage
            {
                BindingContext = _lastCrashViewModel
            });
        }

        // Initialize LaunchDarkly
        if (!string.IsNullOrWhiteSpace(_globalSettings.LaunchDarklyMobileKey))
        {
            try
            {
                // Create context key using device information
                string contextKey = $"{DeviceInfo.Current.Platform}-{DeviceInfo.Current.Model}-{DeviceInfo.Current.Name}";

                // If the contextKey is empty or just contains dashes, use a persistent unique identifier
                if (string.IsNullOrWhiteSpace(contextKey.Trim('-')))
                {
                    // Try to get saved context key from settings
                    contextKey = _preferences.Get("LDDeviceContextKey", string.Empty);

                    // If no saved context key exists, generate a new unique one and save it
                    if (string.IsNullOrWhiteSpace(contextKey))
                    {
                        contextKey = Guid.NewGuid().ToString();
                        _preferences.Get("LDDeviceContextKey", contextKey);
                    }
                }

                await _launchDarklyService.InitializeAsync(contextKey, _globalSettings.LaunchDarklyMobileKey, _globalSettings.LaunchDarklyMemberId);
            }
            catch (Exception ex)
            {
                // Log the error but don't block app startup
                Debug.WriteLine($"LaunchDarkly initialization failed: {ex.Message}");
            }
        }

        // Check for updates on Windows platform and show the GetUpdatePage if an update is available.
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