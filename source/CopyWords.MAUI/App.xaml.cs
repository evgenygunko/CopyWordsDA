using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI;

public partial class App : Application
{
    private readonly ISettingsService _settingsService;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public App(
        ISettingsService settingsService,
        MainWindowViewModel mainWindowViewModel)
    {
        _settingsService = settingsService;
        _mainWindowViewModel = mainWindowViewModel;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        AppSettings appSettings = _settingsService.LoadSettings();

        Window window = new MainWindow(_mainWindowViewModel)
        {
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

