#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class HeadwordViewModel : ObservableObject
    {
        public HeadwordViewModel(Headword headword)
        {
            Original = headword?.Original;
            English = headword?.English;
            Russian = headword?.Russian;
        }

        [ObservableProperty]
        public string? original;

        [ObservableProperty]
        private string? english;

        [ObservableProperty]
        private string? russian;

        [ObservableProperty]
        private bool isTranslationTranslationChecked;
    }
}
