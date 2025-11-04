using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Core.ViewModels.Validation;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace CopyWords.MAUI;

public static class MauiProgram
{
    private static IServiceProvider _serviceProvider = default!;

    public static TService GetService<TService>() => _serviceProvider.GetService<TService>()!;

    [SupportedOSPlatform("windows10.0.17763")]
    [SupportedOSPlatform("maccatalyst15.0")]
    [SupportedOSPlatform("android26.0")]
    public static MauiApp CreateMauiApp()
    {
        GlobalSettings globalSettings = ReadGlobalSettings();
        Debug.Assert(!string.IsNullOrEmpty(globalSettings.SentryDsn), "Sentry DSN is not configured in appsettings.");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionToolkit()
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .UseSentry(options =>
            {
                // The DSN is the only required setting.
                options.Dsn = globalSettings.SentryDsn;

                // Use debug mode if you want to see what the SDK is doing.
                // Debug messages are written to stdout with Console.Writeline,
                // and are viewable in your IDE's debug console or with 'adb logcat', etc.
                // This option is not recommended when deploying your application.
                options.Debug = true;

                // Other Sentry options can be set here.
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined-Regular");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons-Regular");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Proxies for .net classes which don't have interfaces
        builder.Services.AddSingleton<IShellService, ShellService>();
        builder.Services.AddSingleton<IFileIOService, FileIOService>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton(FolderPicker.Default);
        builder.Services.AddSingleton(FilePicker.Default);
        builder.Services.AddSingleton(DeviceInfo.Current);
        builder.Services.AddSingleton(FileSaver.Default);
        builder.Services.AddSingleton(Share.Default);

        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ICopySelectedToClipboardService, CopySelectedToClipboardService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<ISnackbarService, SnackbarService>();
        builder.Services.AddSingleton<IClipboardService, ClipboardService>();
        builder.Services.AddSingleton<IInstantTranslationService, InstantTranslationService>();
        builder.Services.AddSingleton<ISaveImageFileService, SaveImageFileService>();
        builder.Services.AddSingleton<INavigationHistory, NavigationHistory>();
        builder.Services.AddSingleton<IBuildConfiguration, BuildConfiguration>();
        builder.Services.AddSingleton<IGlobalSettings>(globalSettings);

        builder.Services.AddHttpClient<ITranslationsService, TranslationsService>();
        builder.Services.AddHttpClient<IUpdateService, UpdateService>();
        builder.Services.AddHttpClient<ISaveSoundFileService, SaveSoundFileService>();
        builder.Services.AddHttpClient<IFileDownloaderService, FileDownloaderService>();
        builder.Services.AddHttpClient<ISuggestionsService, SuggestionsService>();

        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<IWordViewModel, WordViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<HistoryPageViewModel>();
        builder.Services.AddSingleton<GetUpdateViewModel>();
        builder.Services.AddSingleton<LastCrashViewModel>();

        builder.Services.AddScoped<IValidator<SettingsViewModel>, SettingsViewModelValidator>();

        var app = builder.Build();

        _serviceProvider = app.Services; // store service provider reference

        return app;
    }

    private static GlobalSettings ReadGlobalSettings()
    {
        GlobalSettings? globalSettings = null;

        var a = Assembly.GetExecutingAssembly();

        using var streamLocalFile = a.GetManifestResourceStream("CopyWords.MAUI.appsettings.Local.json");
        if (streamLocalFile != null)
        {
            streamLocalFile.Position = 0;
            globalSettings = System.Text.Json.JsonSerializer.Deserialize<GlobalSettings>(streamLocalFile);
        }

        if (globalSettings == null)
        {
            using var stream = a.GetManifestResourceStream("CopyWords.MAUI.appsettings.json");
            if (stream != null)
            {
                stream.Position = 0;
                globalSettings = System.Text.Json.JsonSerializer.Deserialize<GlobalSettings>(stream);
            }
        }

        if (globalSettings == null)
        {
            throw new Exception("Cannot deserialize CopyWords.MAUI.appsettings.json.");
        }

        return globalSettings;
    }
}