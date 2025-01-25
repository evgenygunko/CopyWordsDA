// Ignore Spelling: Popup

using CommunityToolkit.Maui.Views;
using CopyWords.Core.ViewModels.Popups;

namespace CopyWords.MAUI.Views.Popups;

public partial class SelectDictionaryPopup : Popup
{
    public SelectDictionaryPopup(SelectDictionaryPopupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}