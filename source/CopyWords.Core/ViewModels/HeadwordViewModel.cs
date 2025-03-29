using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class HeadwordViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        public HeadwordViewModel(Headword headword, ISettingsService settingsService)
        {
            _settingsService = settingsService;

            Update(headword);
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

        public void Update(Headword headword)
        {
            Original = headword?.Original;

            // Make first letter lower case, it looks better
            English = FirstLetterToLower(headword?.English);
            Russian = FirstLetterToLower(headword?.Russian);

            AppSettings appSettings = _settingsService.LoadSettings();
            if (appSettings.SelectedParser == SourceLanguage.Spanish.ToString())
            {
                ShowEnglishTranslation = false;
            }
            else
            {
                ShowEnglishTranslation = true;
            }
        }

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
