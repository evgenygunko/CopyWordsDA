using CopyWords.Core.Services;

namespace CopyWords.MAUI;

public partial class App : Application
{
    private readonly ISettingsService _settingsService;

    public App(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new Window(new AppShell())
        {
            Width = _settingsService.MainWindowWidth,
            Height = _settingsService.MainWindowHeight,
            X = _settingsService.MainWindowXPos,
            Y = _settingsService.MainWindowYPos
        };

        window.SizeChanged += (s, e) =>
        {
            _settingsService.MainWindowWidth = window.Width;
            _settingsService.MainWindowHeight = window.Height;
            _settingsService.MainWindowXPos = window.X;
            _settingsService.MainWindowYPos = window.Y;
        };

        return window;
    }
}

