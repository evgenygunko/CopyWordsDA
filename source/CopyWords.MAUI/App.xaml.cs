// Ignore Spelling: App

using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Resources.Styles;

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

    public App(
        ISettingsService settingsService,
        IUpdateService updateService,
        IDialogService dialogService,
        IPreferences preferences,
        MainWindowViewModel mainWindowViewModel,
        GetUpdateViewModel getUpdateViewModel,
        LastCrashViewModel lastCrashViewModel,
        IGlobalSettings globalSettings,
        ILaunchDarklyService launchDarklyService)
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

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(globalSettings.SyncfusionLicenseKey);

        InitializeComponent();

        // Apply the initial theme based on settings
        ApplyTheme(AppTheme.Light);
    }

    /// <summary>
    /// Applies the specified theme to the application.
    /// </summary>
    /// <param name="theme">The theme to apply (Light or Dark).</param>
    public static void ApplyTheme(AppTheme theme)
    {
        if (Current?.Resources.MergedDictionaries == null)
        {
            return;
        }

        ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;

        // Find and remove the current theme dictionary
        var existingTheme = mergedDictionaries.FirstOrDefault(d => d is LightTheme or DarkTheme);
        if (existingTheme != null)
        {
            mergedDictionaries.Remove(existingTheme);
        }

        // Add the new theme dictionary
        ResourceDictionary newTheme = theme == AppTheme.Dark ? new DarkTheme() : new LightTheme();

        // Insert the theme before Styles.xaml (which should be the last item)
        var stylesDictionary = mergedDictionaries.LastOrDefault();
        if (stylesDictionary != null)
        {
            var list = mergedDictionaries.ToList();
            int stylesIndex = list.IndexOf(stylesDictionary);
            list.Insert(stylesIndex, newTheme);

            mergedDictionaries.Clear();
            foreach (var dict in list)
            {
                mergedDictionaries.Add(dict);
            }
        }
        else
        {
            mergedDictionaries.Add(newTheme);
        }

        // Update the UserAppTheme to match
        Current.UserAppTheme = theme;
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