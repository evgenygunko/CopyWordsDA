using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class HeadwordViewModel : ObservableObject
    {
        public HeadwordViewModel(Headword headword, SourceLanguage sourceLanguage)
        {
            Original = headword?.Original;

            // Make first letter lower case, it looks better
            English = FirstLetterToLower(headword?.English);
            Russian = FirstLetterToLower(headword?.Russian);

            // SpanishDict already return English translations, no need to show what the Translator app returned.
            ShowEnglishTranslation = sourceLanguage != SourceLanguage.Spanish;
        }

        [ObservableProperty]
        public string? original;

        [ObservableProperty]
        private string? english;

        [ObservableProperty]
        private string? russian;

        [ObservableProperty]
        private bool isEnglishTranslationChecked;

        [ObservableProperty]
        private bool isRussianTranslationChecked;

        [ObservableProperty]
        private bool showEnglishTranslation;

        internal static string? FirstLetterToLower(string? input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            {
                return input; // If the input is null/empty or first letter is already lowercase, return as is
            }

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}
