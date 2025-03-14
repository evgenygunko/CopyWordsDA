// Ignore Spelling: popup

#nullable enable
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels.Popups;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class SelectDictionaryViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IPopupService _popupService;

        public SelectDictionaryViewModel(
            ISettingsService settingsService,
            IPopupService popupService)
        {
            _settingsService = settingsService;
            _popupService = popupService;

            Init();
        }

        public ObservableCollection<Models.Parsers> Parsers { get; } = new ObservableCollection<Models.Parsers>();

        [ObservableProperty]
        private Models.Parsers selectedParser = default!;

        // When the parser is changes in the Popup and Save is pressed.
        [RelayCommand]
        public async Task SelectDictionaryAsync(CancellationToken cancellationToken)
        {
            bool isDDOChecked = SelectedParser.SourceLanguage == SourceLanguage.Danish;
            Object? result = await _popupService.ShowPopupAsync<SelectDictionaryPopupViewModel>(onPresenting: viewModel =>
            {
                viewModel.IsDDOChecked = isDDOChecked;
                viewModel.IsSpanishDictChecked = !isDDOChecked;
            });

            if (result is null)
            {
                // User pressed "Cancel"
                return;
            }

            string? strResult = result.ToString();
            if (!Enum.TryParse(strResult, out SourceLanguage selectedLanguage))
            {
                throw new NotSupportedException($"Source language '{strResult}' selected in the popup is not supported.");
            }

            SelectedParser = Parsers.First(x => x.SourceLanguage == selectedLanguage);

            AppSettings appSettings = _settingsService.LoadSettings();
            appSettings.SelectedParser = selectedLanguage.ToString();
            _settingsService.SaveSettings(appSettings);

            Debug.WriteLine($"Selected parser has changed to '{selectedLanguage}'");
        }

        internal void Init()
        {
            Parsers.Add(new Models.Parsers("Den Danske Ordbog", "flag_of_denmark.png", SourceLanguage.Danish));
            Parsers.Add(new Models.Parsers("Spanish Dict", "flag_of_spain.png", SourceLanguage.Spanish));

            string lastUsedLanguage = _settingsService.LoadSettings().SelectedParser;
            Models.Parsers? savedParser = Parsers.FirstOrDefault(x => x.SourceLanguage.ToString() == lastUsedLanguage);
            if (savedParser is null)
            {
                SelectedParser = Parsers[0];
            }
            else
            {
                SelectedParser = savedParser;
            }
        }

        // When the parser is changed in Picker
        partial void OnSelectedParserChanged(Models.Parsers value)
        {
            SaveSelectedParserInSettings(value);
        }

        internal void SaveSelectedParserInSettings(Models.Parsers value)
        {
            AppSettings appSettings = _settingsService.LoadSettings();

            string selectedParserInPicker = value.SourceLanguage.ToString();
            if (appSettings.SelectedParser != selectedParserInPicker)
            {
                appSettings.SelectedParser = value.SourceLanguage.ToString();
                _settingsService.SaveSettings(appSettings);

                Debug.WriteLine($"Selected parser has changed to '{value.Name}'");
            }
        }
    }
}
