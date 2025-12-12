using CopyWords.Core.ViewModels;
using CopyWords.MAUI.Helpers;

namespace CopyWords.MAUI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel vm, IDeviceInfo deviceInfo)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;

        // Add NavigateBackCommand toolbar button only on Windows.
        // There is no "Visible" property on ToolbarItem, so we need to create and add it conditionally, see https://stackoverflow.com/a/74424283
        if (deviceInfo.Platform == DevicePlatform.WinUI)
        {
            CreateWindowsToolbarItems();
        }
    }

    private void CreateWindowsToolbarItems()
    {
        // Create the Navigate Back toolbar item for Windows only
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
            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    await _viewModel.NavigateBackAsync();
                }
                catch (ObjectDisposedException)
                {
                    // Swallow exception if ViewModel disposed during async operation
                }
            });

            // Back navigation was handled, prevent default back behavior
            return true;
        }

        return base.OnBackButtonPressed();
    }
}
