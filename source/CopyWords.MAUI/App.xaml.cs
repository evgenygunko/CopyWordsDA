﻿// Ignore Spelling: App

using CopyWords.Core.Models;
using CopyWords.Core.Services;
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

    public App(
        ISettingsService settingsService,
        IUpdateService updateService,
        IDialogService dialogService,
        IPreferences preferences,
        MainWindowViewModel mainWindowViewModel,
        GetUpdateViewModel getUpdateViewModel,
        LastCrashViewModel lastCrashViewModel,
        IGlobalSettings globalSettings)
    {
        UserAppTheme = AppTheme.Light;

        _settingsService = settingsService;
        _updateService = updateService;
        _dialogService = dialogService;
        _preferences = preferences;
        _mainWindowViewModel = mainWindowViewModel;
        _getUpdateViewModel = getUpdateViewModel;
        _lastCrashViewModel = lastCrashViewModel;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(globalSettings.SyncfusionLicenseKey);

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        AppSettings appSettings = _settingsService.LoadSettings();

        Window window = new MainWindow(_updateService, _dialogService, _preferences, _getUpdateViewModel, _lastCrashViewModel)
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

