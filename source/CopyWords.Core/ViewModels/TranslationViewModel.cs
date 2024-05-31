using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class TranslationViewModel : ObservableObject
    {
        public TranslationViewModel(Translation translation)
        {
            English = translation.English;
            AlphabeticalPosition = translation.AlphabeticalPosition;
            ImageUrl = translation.ImageUrl;

            ExampleViewModels.Clear();
            foreach (var examples in translation.Examples)
            {
                ExampleViewModels.Add(new ExampleViewModel(examples));
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
