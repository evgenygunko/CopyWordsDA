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
        public partial string Original { get; set; }

        [ObservableProperty]
        public partial string Translation { get; set; }

        [ObservableProperty]
        public partial bool IsChecked { get; set; }

        [ObservableProperty]
        public partial bool ShowCopyButtons { get; set; }
    }
}
