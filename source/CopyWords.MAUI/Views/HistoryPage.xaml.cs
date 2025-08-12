using CopyWords.Core.ViewModels;

namespace CopyWords.MAUI.Views;

public partial class HistoryPage : ContentPage
{
    public HistoryPage(HistoryPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}