using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

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
