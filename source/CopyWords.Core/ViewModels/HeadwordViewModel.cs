using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class HeadwordViewModel : ObservableObject
    {
        public HeadwordViewModel(
            Headword headword,
            SourceLanguage sourceLanguage,
            bool showCopyButtons)
        {
            Original = headword?.Original;

            // Make first letter lower case, it looks better
            English = FirstLetterToLower(headword?.English);
            Russian = FirstLetterToLower(headword?.Russian);

            // SpanishDict already return English translations, no need to show what the Translator app returned.
            ShowEnglishTranslation = sourceLanguage != SourceLanguage.Spanish;

            CanCheckRussianTranslation = showCopyButtons;
            CanCheckEnglishTranslation = showCopyButtons && ShowEnglishTranslation;

            if (showCopyButtons)
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
        public partial string? Russian { get; set; }

        [ObservableProperty]
        public partial bool IsEnglishTranslationChecked { get; set; }

        [ObservableProperty]
        public partial bool IsRussianTranslationChecked { get; set; }

        [ObservableProperty]
        public partial bool ShowEnglishTranslation { get; set; }

        [ObservableProperty]
        public partial bool CanCheckRussianTranslation { get; set; }

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
