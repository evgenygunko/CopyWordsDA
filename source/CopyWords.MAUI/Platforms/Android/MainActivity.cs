using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using CopyWords.Core.Services;

namespace CopyWords.MAUI;

[Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        Exported = true)]
[IntentFilter(
        [Intent.ActionProcessText, Intent.CategoryDefault],
        DataMimeType = "text/plain")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

#pragma warning disable CA1416 // Validate platform compatibility
        string? selectedText = this.Intent?.GetStringExtra(Intent.ExtraProcessText)?.ToString();
#pragma warning restore CA1416 // Validate platform compatibility

        if (!string.IsNullOrEmpty(selectedText))
        {
            Log.Debug("MainActivity", "Received text: " + selectedText);

            var instantTranslationService = MauiProgram.GetService<IInstantTranslationService>();
            instantTranslationService.SetText(selectedText);
        }
    }
}

