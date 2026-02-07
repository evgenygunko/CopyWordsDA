using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class MeaningViewModel : ObservableObject
    {
        public event EventHandler<string> MeaningLookupClicked = default!;

        public MeaningViewModel(
            Meaning meaning,
            SourceLanguage sourceLanguage,
            bool showCheckBoxes)
        {
            Original = meaning.Original;

            Translation = meaning.Translation ?? string.Empty;
            AlphabeticalPosition = meaning.AlphabeticalPosition;
            Tag = meaning.Tag ?? string.Empty;
            ImageUrl = meaning.ImageUrl ?? string.Empty;
            LookupUrl = meaning.LookupUrl ?? string.Empty;

            ExampleViewModels.Clear();
            foreach (var example in meaning.Examples)
            {
                ExampleViewModels.Add(new ExampleViewModel(example, showCheckBoxes));
            }

            ShowCheckBoxes = showCheckBoxes;

            if (sourceLanguage == SourceLanguage.Spanish)
            {
                ExamplesMargin = new Thickness(9, 5, 0, 10);
            }
            else
            {
                ExamplesMargin = new Thickness(9, 5, 0, 20);
            }
        }

        [ObservableProperty]
        public partial string Original { get; set; }

        [ObservableProperty]
        public partial string Translation { get; set; }

        [ObservableProperty]
        public partial string AlphabeticalPosition { get; set; }

        [ObservableProperty]
        public partial string Tag { get; set; }

        [ObservableProperty]
        public partial string ImageUrl { get; set; }

        [ObservableProperty]
        public partial bool IsImageChecked { get; set; }

        [ObservableProperty]
        public partial bool ShowCheckBoxes { get; set; }

        [ObservableProperty]
        public partial Thickness ExamplesMargin { get; set; }

        [ObservableProperty]
        public partial string LookupUrl { get; set; }

        public bool CanLookupMeaning => !string.IsNullOrEmpty(LookupUrl);

        public ObservableCollection<ExampleViewModel> ExampleViewModels { get; } = [];

        [RelayCommand(CanExecute = nameof(CanLookupMeaning))]
        private void SelectLookupUrl()
        {
            MeaningLookupClicked?.Invoke(this, LookupUrl);
        }
    }
}
