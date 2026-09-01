using System.Runtime.Versioning;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.MAUI.Navigation;
using Microsoft.Maui.Platform;

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
    private AndroidBackPressedCallback? _backPressedCallback;
    private IAppThemeService? _appThemeService;
    private Android.Views.View? _statusBarBackground;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        CreateStatusBarBackground();

        _appThemeService = MauiProgram.GetService<IAppThemeService>();
        if (_appThemeService is not null)
        {
            _appThemeService.ThemeChanged += OnThemeChanged;
        }

        UpdateStatusBar();

        var backNavigationCoordinator = MauiProgram.GetService<AndroidBackNavigationCoordinator>();
        if (backNavigationCoordinator is not null)
        {
            _backPressedCallback = new AndroidBackPressedCallback(backNavigationCoordinator, OnBackPressedDispatcher);
            OnBackPressedDispatcher.AddCallback(this, _backPressedCallback);
        }

        string? selectedText = this.Intent?.GetStringExtra(Intent.ExtraProcessText)?.ToString();

        if (!string.IsNullOrEmpty(selectedText))
        {
            Log.Debug("MainActivity", "Received text: " + selectedText);

            var instantTranslationService = MauiProgram.GetService<IInstantTranslationService>();
            instantTranslationService?.SetText(selectedText);
        }
    }

    protected override void OnDestroy()
    {
        if (_appThemeService is not null)
        {
            _appThemeService.ThemeChanged -= OnThemeChanged;
            _appThemeService = null;
        }

        if (_statusBarBackground?.Parent is ViewGroup parent)
        {
            parent.RemoveView(_statusBarBackground);
        }

        _statusBarBackground?.Dispose();
        _statusBarBackground = null;

        _backPressedCallback?.Remove();
        _backPressedCallback?.Dispose();
        _backPressedCallback = null;

        // todo: this is workaround for a crash https://github.com/dotnet/maui/issues/32600#issuecomment-3646966167
        // Delete this method when a fix is released.
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
        try
        {
            base.OnDestroy();
        }
        catch (Exception)
        {
        }
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception
    }

    private void CreateStatusBarBackground()
    {
        if (Window?.DecorView is not ViewGroup decorView)
        {
            return;
        }

        _statusBarBackground = new Android.Views.View(this)
        {
            ImportantForAccessibility = ImportantForAccessibility.No,
            LayoutParameters = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                GetStatusBarHeight(),
                GravityFlags.Top)
        };

        decorView.AddView(_statusBarBackground);
    }

    private int GetStatusBarHeight()
    {
        int resourceId = Resources?.GetIdentifier("status_bar_height", "dimen", "android") ?? 0;
        return resourceId > 0 ? Resources!.GetDimensionPixelSize(resourceId) : 0;
    }

    private void OnThemeChanged(object? sender, AppColorTheme theme) => RunOnUiThread(UpdateStatusBar);

    private void UpdateStatusBar()
    {
        if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue("NavigationBarBackgroundColor", out object? value) != true ||
            value is not Microsoft.Maui.Graphics.Color color)
        {
            return;
        }

        _statusBarBackground?.SetBackgroundColor(color.ToPlatform());

        // Android 15 enforces a transparent status bar, so the view above provides
        // its background. Older Android versions still use the window color.
        if (!OperatingSystem.IsAndroidVersionAtLeast(35))
        {
#pragma warning disable CA1422 // Needed for status bars before Android 15 edge-to-edge enforcement.
            Window?.SetStatusBarColor(color.ToPlatform());
#pragma warning restore CA1422
        }

        // Every app theme uses a dark navigation color, so status-bar icons must be light.
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            Window?.InsetsController?.SetSystemBarsAppearance(
                0,
                (int)WindowInsetsControllerAppearance.LightStatusBars);
        }
        else if (Window?.DecorView is not null)
        {
#pragma warning disable CA1422 // Required for the app's Android 8-10 minimum versions.
            Window.DecorView.SystemUiFlags &= ~Android.Views.SystemUiFlags.LightStatusBar;
#pragma warning restore CA1422
        }
    }
}
