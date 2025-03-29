using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Parsers.Models;

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
        private string word;

        [ObservableProperty]
        private string url;

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
