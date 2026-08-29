using System.ComponentModel;
using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Helpers;
using CopyWords.MAUI.Navigation;

namespace CopyWords.MAUI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly IBackNavigationCoordinator _backNavigationCoordinator;

    public MainPage(
        MainViewModel vm,
        IDeviceInfo deviceInfo,
        IBackNavigationCoordinator backNavigationCoordinator)
    {
        InitializeComponent();
        _viewModel = vm;
        _backNavigationCoordinator = backNavigationCoordinator;
        BindingContext = _viewModel;

        // Add the NavigateBackCommand toolbar button on desktop platforms.
        // There is no "Visible" property on ToolbarItem, so we need to create and add it conditionally, see https://stackoverflow.com/a/74424283
        if (deviceInfo.Platform == DevicePlatform.WinUI || deviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            CreateDesktopToolbarItems();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _backNavigationCoordinator.Activate(
            () => _viewModel.CanNavigateBack,
            NavigateBackSafelyAsync);
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _backNavigationCoordinator.Deactivate();

        base.OnDisappearing();
    }

    private void CreateDesktopToolbarItems()
    {
        // Create the Navigate Back toolbar item for supported desktop platforms.
        var navigateBackToolbarItem = new ToolbarItem
        {
            Command = _viewModel.NavigateBackCommand,
            Order = ToolbarItemOrder.Primary,
            Priority = -1 // Put it at the beginning
        };

        navigateBackToolbarItem.IconImageSource = new FontImageSource
        {
            FontFamily = "MaterialIconsOutlined-Regular",
            Glyph = MaterialDesignIconFonts.Arrow_back,
            Size = 20
        };

        // Insert at the beginning of the toolbar
        ToolbarItems.Insert(0, navigateBackToolbarItem);
    }

    protected override bool OnBackButtonPressed()
    {
        // Check if we have a MainViewModel and can navigate back in search history
        if (_viewModel.CanNavigateBack)
        {
            Dispatcher.Dispatch(async () => await NavigateBackSafelyAsync());

            // Back navigation was handled, prevent default back behavior
            return true;
        }

        return base.OnBackButtonPressed();
    }

    private async Task<bool> NavigateBackSafelyAsync()
    {
        try
        {
            return await _viewModel.NavigateBackAsync();
        }
        catch (ObjectDisposedException)
        {
            // Treat navigation as handled if the page was disposed while it was running.
            return true;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CanNavigateBack))
        {
            _backNavigationCoordinator.Refresh();
        }
    }
}
