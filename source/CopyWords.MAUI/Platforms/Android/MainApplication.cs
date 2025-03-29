// Ignore Spelling: App

using Android.App;
using Android.Runtime;

namespace CopyWords.MAUI;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp()
    {
        // Remove SearchBar control underline
        // https://learn.microsoft.com/en-us/answers/questions/1029179/how-do-i-remove-the-underline-from-searchbar-in-an
        Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("SearchBarNoUnderline", (handler, view) =>
        {
            Android.Widget.LinearLayout? linearLayout = handler.PlatformView.GetChildAt(0) as Android.Widget.LinearLayout;
            linearLayout = linearLayout?.GetChildAt(2) as Android.Widget.LinearLayout;
            linearLayout = linearLayout?.GetChildAt(1) as Android.Widget.LinearLayout;

            if (linearLayout != null)
            {
                linearLayout.Background = null;
            }
        });

        return MauiProgram.CreateMauiApp();
    }
}

