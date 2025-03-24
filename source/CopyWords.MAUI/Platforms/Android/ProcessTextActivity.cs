using Android.App;
using Android.Content;
using Android.OS;
using CopyWords.Core.Services;

namespace CopyWords.MAUI;

[Activity(Theme = "@style/Maui.MainTheme.NoActionBar", Exported = true)]
[IntentFilter([Intent.ActionProcessText, Intent.CategoryDefault], DataMimeType = "text/plain")]
public class ProcessTextActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

#pragma warning disable CA1416 // Validate platform compatibility
        string? selectedText = this.Intent?.GetStringExtra(Intent.ExtraProcessText)?.ToString();
#pragma warning restore CA1416 // Validate platform compatibility

        if (!string.IsNullOrEmpty(selectedText))
        {
            var instantTranslationService = MauiProgram.GetService<IInstantTranslationService>();
            instantTranslationService.SetText(selectedText);
        }
    }
}
