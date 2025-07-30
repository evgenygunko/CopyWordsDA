using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ExampleViewModel : ObservableObject
    {
        public ExampleViewModel(Example example, bool showCopyButtons)
        {
            Original = example.Original;
            Translation = example.Translation ?? string.Empty;
            ShowCopyButtons = showCopyButtons;
        }

        [ObservableProperty]
        private string original;

        [ObservableProperty]
        private string translation;

        [ObservableProperty]
        private bool isChecked;

        [ObservableProperty]
        private bool showCopyButtons;
    }
}
