using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
