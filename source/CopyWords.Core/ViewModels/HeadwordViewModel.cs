using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class HeadwordViewModel : ObservableObject
    {
        public HeadwordViewModel(
            Headword headword,
            SourceLanguage sourceLanguage,
            bool showCheckBoxes,
            string destinationLanguage)
        {
            Original = headword?.Original;

            // Make first letter lower case, it looks better
            English = FirstLetterToLower(headword?.English);
            Translation = FirstLetterToLower(headword?.Translation);

            bool isDestinationLanguageEnglish = string.Equals(destinationLanguage, "English", StringComparison.OrdinalIgnoreCase);
            // SpanishDict already returns English translations. Also hide English translation when destination language is English to avoid duplicate content.
            ShowEnglishTranslation = sourceLanguage != SourceLanguage.Spanish && !isDestinationLanguageEnglish;

            CanCheckDestinationTranslation = showCheckBoxes;
            CanCheckEnglishTranslation = showCheckBoxes && ShowEnglishTranslation;

            if (showCheckBoxes)
            {
                BorderPadding = new Thickness(0);
            }
            else
            {
                BorderPadding = new Thickness(5, 3, 5, 5);
            }
        }

        [ObservableProperty]
        public partial string? Original { get; set; }

        [ObservableProperty]
        public partial string? English { get; set; }

        [ObservableProperty]
        public partial string? Translation { get; set; }

        [ObservableProperty]
        public partial bool IsEnglishTranslationChecked { get; set; }

        [ObservableProperty]
        public partial bool IsDestinationTranslationChecked { get; set; }

        [ObservableProperty]
        public partial bool ShowEnglishTranslation { get; set; }

        [ObservableProperty]
        public partial bool CanCheckDestinationTranslation { get; set; }

        [ObservableProperty]
        public partial bool CanCheckEnglishTranslation { get; set; }

        [ObservableProperty]
        public partial Thickness BorderPadding { get; set; }

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
