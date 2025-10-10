using CopyWords.Core.ViewModels;
#if WINDOWS
using CopyWords.MAUI.Helpers;
#endif

namespace CopyWords.MAUI.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

#if WINDOWS
        // Add NavigateBackCommand toolbar button only on Windows.
        // There is no "Visible" property on ToolbarItem, so we need to create and add it conditionally, see https://stackoverflow.com/a/74424283
        CreateWindowsToolbarItems();
#endif

        Unloaded += (_, e) =>
        {
            vm.CancelHttpRequests();
            throw new Exception("Unloaded event triggered, cancelling HTTP requests");
        };
    }

#if WINDOWS
    private void CreateWindowsToolbarItems()
    {
        // Create the Navigate Back toolbar item for Windows only
        var navigateBackToolbarItem = new ToolbarItem
        {
            Command = ((MainViewModel)BindingContext).NavigateBackCommand,
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
#endif

    protected override bool OnBackButtonPressed()
    {
        // Check if we have a MainViewModel and can navigate back in search history
        if (BindingContext is MainViewModel mainViewModel)
        {
            if (mainViewModel.CanNavigateBack)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await mainViewModel.NavigateBackAsync();
                });

                // Back navigation was handled, prevent default back behavior
                return true;
            }
        }

        return base.OnBackButtonPressed();
    }
}
