using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class MeaningViewModel : ObservableObject
    {
        public MeaningViewModel(Meaning meaning)
        {
            Original = meaning.Original;

            Translation = meaning.Translation ?? string.Empty;
            AlphabeticalPosition = meaning.AlphabeticalPosition;
            tag = meaning.Tag ?? string.Empty;
            ImageUrl = meaning.ImageUrl ?? string.Empty;

            ExampleViewModels.Clear();
            foreach (var example in meaning.Examples)
            {
                ExampleViewModels.Add(new ExampleViewModel(example));
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

        public ObservableCollection<ExampleViewModel> ExampleViewModels { get; } = new();
    }
}
