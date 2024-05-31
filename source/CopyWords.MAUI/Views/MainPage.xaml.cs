using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        // workaround for layout bugs in MAUI
        WidthRequest = Width - 1;
    }
}
