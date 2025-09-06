using System.Runtime.Versioning;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using CopyWords.Core.Services;

namespace CopyWords.MAUI;

[SupportedOSPlatform("android26.0")]
[Activity(
        Theme = "@style/Maui.SplashTheme",
        ResizeableActivity = true,
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        Exported = true)]
[IntentFilter(
        [Intent.ActionProcessText, Intent.CategoryDefault],
        DataMimeType = "text/plain",
#if DEBUG
        Label = "CopyWords (debug)")]
#else
        Label = "CopyWords")]
#endif
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string? selectedText = this.Intent?.GetStringExtra(Intent.ExtraProcessText)?.ToString();

        if (!string.IsNullOrEmpty(selectedText))
        {
            Log.Debug("MainActivity", "Received text: " + selectedText);

            var instantTranslationService = MauiProgram.GetService<IInstantTranslationService>();
            instantTranslationService.SetText(selectedText);
        }
    }
}

