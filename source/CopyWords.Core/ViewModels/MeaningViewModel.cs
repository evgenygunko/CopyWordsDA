using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class MeaningViewModel : ObservableObject
    {
        public MeaningViewModel(Meaning meaning)
        {
            English = meaning.Description;
            AlphabeticalPosition = meaning.AlphabeticalPosition;
            ImageUrl = meaning.ImageUrl;

            ExampleViewModels.Clear();
            foreach (var example in meaning.Examples)
            {
                ExampleViewModels.Add(new ExampleViewModel(example));
            }
        }

        [ObservableProperty]
        private string english;

        [ObservableProperty]
        private string alphabeticalPosition;

        [ObservableProperty]
        private string imageUrl;

        [ObservableProperty]
        private bool isImageChecked;

        public ObservableCollection<ExampleViewModel> ExampleViewModels { get; } = new();
    }
}
