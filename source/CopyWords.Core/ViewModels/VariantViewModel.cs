using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class VariantViewModel : ObservableObject
    {
        public event EventHandler<string> Clicked = default!;

        public VariantViewModel(Variant variant)
        {
            Word = variant.Word;
            Url = variant.Url;
        }

        [ObservableProperty]
        public partial string Word { get; set; }

        [ObservableProperty]
        public partial string Url { get; set; }

        [RelayCommand]
        public void SelectVariant()
        {
            var eventHandler = Clicked;
            if (eventHandler != null)
            {
                eventHandler(this, Url);
            }
        }
    }
}
