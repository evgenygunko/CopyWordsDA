using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class MeaningViewModel : ObservableObject
    {
        public MeaningViewModel(
            Meaning meaning,
            SourceLanguage sourceLanguage,
            bool showCopyButtons)
        {
            Original = meaning.Original;

            Translation = meaning.Translation ?? string.Empty;
            AlphabeticalPosition = meaning.AlphabeticalPosition;
            tag = meaning.Tag ?? string.Empty;
            ImageUrl = meaning.ImageUrl ?? string.Empty;

            ExampleViewModels.Clear();
            foreach (var example in meaning.Examples)
            {
                ExampleViewModels.Add(new ExampleViewModel(example, showCopyButtons));
            }

            ShowCopyButtons = showCopyButtons;

            if (sourceLanguage == SourceLanguage.Spanish)
            {
                examplesMargin = new Thickness(9, 5, 0, 10);
            }
            else
            {
                examplesMargin = new Thickness(9, 5, 0, 20);
            }
        }

        [ObservableProperty]
        private string original;

        [ObservableProperty]
        private string translation;

        [ObservableProperty]
        private string alphabeticalPosition;

        [ObservableProperty]
        private string tag;

        [ObservableProperty]
        private string imageUrl;

        [ObservableProperty]
        private bool isImageChecked;

        [ObservableProperty]
        private bool showCopyButtons;

        [ObservableProperty]
        private Thickness examplesMargin;

        public ObservableCollection<ExampleViewModel> ExampleViewModels { get; } = new();
    }
}
