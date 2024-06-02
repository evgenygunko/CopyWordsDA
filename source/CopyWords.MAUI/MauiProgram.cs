using CommunityToolkit.Maui;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Views;
using CopyWords.Parsers;
using CopyWords.Parsers.Services;
using Microsoft.Extensions.Logging;

namespace CopyWords.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
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

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<SettingsPage>();

        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ILookUpWord, LookUpWord>();
        builder.Services.AddSingleton<IDDOPageParser, DDOPageParser>();
        builder.Services.AddSingleton<ICopySelectedToClipboardService, CopySelectedToClipboardService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IClipboardService, ClipboardService>();

        builder.Services.AddHttpClient<ISaveSoundFileService, SaveSoundFileService>();
        builder.Services.AddHttpClient<ISaveImageFileService, SaveImageFileService>();
        builder.Services.AddHttpClient<IFileDownloader, FileDownloader>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<WordViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        return builder.Build();
    }
}