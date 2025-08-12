using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CopyWords.Core.ViewModels
{
    public partial class PreviousWordViewModel : ObservableObject
    {
        public event EventHandler<string> Clicked = default!;

        public PreviousWordViewModel(string word, string url)
        {
            Word = word;
            Url = url;
        }

        [ObservableProperty]
        public partial string Word { get; set; }

        [ObservableProperty]
        public partial string Url { get; set; }

        [RelayCommand]
        public void SelectPreviousWord()
        {
            Clicked?.Invoke(this, Url);
        }
    }
}
