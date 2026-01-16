using System.Diagnostics;
using System.Runtime.InteropServices;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
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

    public MainWindow(
        IUpdateService updateService,
        IDialogService dialogService,
        IPreferences preferences,
        IGlobalSettings globalSettings,
        ILaunchDarklyService launchDarklyService,
        GetUpdateViewModel getUpdateViewModel)
    {
        InitializeComponent();

        _updateService = updateService;
        _dialogService = dialogService;
        _preferences = preferences;
        _globalSettings = globalSettings;
        _launchDarklyService = launchDarklyService;

        _getUpdateViewModel = getUpdateViewModel;
    }

    protected override async void OnCreated()
    {
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
                        _preferences.Set("LDDeviceContextKey", contextKey);
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
            await _dialogService.DisplayAlertAsync("Cannot open Settings Page", $"Exception occurred: " + ex.ToString(), "OK");
        }
    }
}