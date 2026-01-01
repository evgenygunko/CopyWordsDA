// Ignore Spelling: App

using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI;

public partial class App : Application
{
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;
    private readonly IDialogService _dialogService;
    private readonly IPreferences _preferences;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly GetUpdateViewModel _getUpdateViewModel;
    private readonly LastCrashViewModel _lastCrashViewModel;
    private readonly IGlobalSettings _globalSettings;
    private readonly ILaunchDarklyService _launchDarklyService;
    private readonly IAppThemeService _appThemeService;

    public App(
        ISettingsService settingsService,
        IUpdateService updateService,
        IDialogService dialogService,
        IPreferences preferences,
        MainWindowViewModel mainWindowViewModel,
        GetUpdateViewModel getUpdateViewModel,
        LastCrashViewModel lastCrashViewModel,
        IGlobalSettings globalSettings,
        ILaunchDarklyService launchDarklyService,
        IAppThemeService appThemeService)
    {
        _settingsService = settingsService;
        _updateService = updateService;
        _dialogService = dialogService;
        _preferences = preferences;
        _mainWindowViewModel = mainWindowViewModel;
        _getUpdateViewModel = getUpdateViewModel;
        _lastCrashViewModel = lastCrashViewModel;
        _globalSettings = globalSettings;
        _launchDarklyService = launchDarklyService;
        _appThemeService = appThemeService;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(globalSettings.SyncfusionLicenseKey);

        InitializeComponent();

        // Apply the initial theme based on settings
        AppTheme appTheme = _settingsService.GetUseDarkTheme() ? AppTheme.Dark : AppTheme.Light;
        _appThemeService.ApplyTheme(appTheme);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        AppSettings appSettings = _settingsService.LoadSettings();

        Window window = new MainWindow(_updateService, _dialogService, _preferences, _globalSettings, _launchDarklyService, _getUpdateViewModel, _lastCrashViewModel)
        {
            BindingContext = _mainWindowViewModel,
            Width = appSettings.MainWindowWidth,
            Height = appSettings.MainWindowHeight,
            X = appSettings.MainWindowXPos,
            Y = appSettings.MainWindowYPos
        };

        window.SizeChanged += (s, e) =>
        {
            AppSettings appSettings1 = _settingsService.LoadSettings();
            appSettings1.MainWindowWidth = window.Width;
            appSettings1.MainWindowHeight = window.Height;
            appSettings1.MainWindowXPos = window.X;
            appSettings1.MainWindowYPos = window.Y;

            _settingsService.SaveSettings(appSettings1);
        };

        return window;
    }
}