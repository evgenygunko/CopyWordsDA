using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CopyWords.Core.ViewModels
{
    public partial class PreviousWordViewModel : ObservableObject
    {
        public event EventHandler<string> Clicked = default!;

        public PreviousWordViewModel(string word)
        {
            Word = word;
        }

        [ObservableProperty]
        public partial string Word { get; set; }

        [RelayCommand]
        public void SelectPreviousWord()
        {
            Clicked?.Invoke(this, Word);
        }
    }
}
