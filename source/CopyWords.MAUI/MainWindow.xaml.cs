using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.Maui.Views;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Views;

namespace CopyWords.MAUI;

public partial class MainWindow : Window
{
    private readonly IUpdateService _updateService;
    private readonly GetUpdateViewModel _getUpdateViewModel;

    public MainWindow(
        IUpdateService updateService,
        GetUpdateViewModel getUpdateViewModel)
    {
        InitializeComponent();

        _updateService = updateService;
        _getUpdateViewModel = getUpdateViewModel;
    }

    protected override async void OnCreated()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && await _updateService.IsUpdateAvailableAsync(AppInfo.VersionString))
            {
                await Navigation.PushModalAsync(new GetUpdatePage
                {
                    BindingContext = _getUpdateViewModel
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occurred while checking for the updates. Error: " + ex);

            _getUpdateViewModel.ErrorMessage = ex.Message;
            await Navigation.PushModalAsync(new CannotCheckUpdates
            {
                BindingContext = _getUpdateViewModel
            });
        }
    }

    protected override void OnDestroying()
    {
        base.OnDestroying();

        // fix to a crash on Windows when closing the app: https://github.com/CommunityToolkit/Maui/issues/962
        var mediaElement = MauiProgram.GetService<MediaElement>();
        mediaElement?.Handler?.DisconnectHandler();
    }

    private async void ButtonSettings_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SettingsPage");
    }
}