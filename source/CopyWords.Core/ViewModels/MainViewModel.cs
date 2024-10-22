using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Parsers;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly ICopySelectedToClipboardService _copySelectedToClipboardService;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;

        private ILookUpWord _lookUpWord;
        private WordViewModel _wordViewModel;
        private SelectDictionaryViewModel _selectDictionaryViewModel;

        public MainViewModel(
            ISettingsService settingsService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            ILookUpWord lookUpWord,
            WordViewModel wordViewModel,
            SelectDictionaryViewModel selectDictionaryViewModel)
        {
            _settingsService = settingsService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;

            _lookUpWord = lookUpWord;
            _wordViewModel = wordViewModel;
            _selectDictionaryViewModel = selectDictionaryViewModel;
        }

        #region Properties

        public WordViewModel WordViewModel => _wordViewModel;

        public SelectDictionaryViewModel SelectDictionaryViewModel => _selectDictionaryViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private bool isBusy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private string searchWord;

        public bool CanExecuteLookUp => !IsBusy && !string.IsNullOrWhiteSpace(SearchWord);

        public bool CanShowSettingsDialog => !IsBusy;

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanExecuteLookUp))]
        public async Task LookUpAsync()
        {
            IsBusy = true;

            WordModel wordModel = await LookUpWordInDictionaryAsync(SearchWord);
            UpdateUI(wordModel);

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanShowSettingsDialog))]
        public async Task ShowSettingsDialog()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }

        #endregion

        #region Internal Methods

        public async Task GetVariantAsync(string url)
        {
            IsBusy = true;

            WordModel wordModel = null;
            try
            {
                string translatorApiURL = null;
                AppSettings appSettings = _settingsService.LoadSettings();
                if (appSettings.UseTranslator && !string.IsNullOrEmpty(appSettings.TranslatorApiUrl))
                {
                    translatorApiURL = appSettings.TranslatorApiUrl;
                }

                wordModel = await _lookUpWord.GetWordByUrlAsync(url, new Options(_selectDictionaryViewModel.SelectedParser.SourceLanguage, translatorApiURL));

                if (wordModel == null)
                {
                    await _dialogService.DisplayAlert("Cannot find word", $"Could not parse the word by URL '{url}'", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("Error occurred while parsing the word", ex.Message, "OK");
            }

            UpdateUI(wordModel);

            IsBusy = false;
        }

        internal void UpdateUI(WordModel wordModel)
        {
            IsBusy = true;

            if (wordModel != null)
            {
                _wordViewModel.SoundUrl = wordModel.SoundUrl;
                _wordViewModel.SoundFileName = wordModel.SoundFileName;

                _wordViewModel.DefinitionViewModels.Clear();
                foreach (var definition in wordModel.Definitions)
                {
                    _wordViewModel.DefinitionViewModels.Add(new DefinitionViewModel(definition, _copySelectedToClipboardService, _dialogService, _clipboardService));
                }

                _wordViewModel.Variants.Clear();
                foreach (var variant in wordModel.Variations)
                {
                    var variantVM = new VariantViewModel(variant);
                    variantVM.Clicked += async (sender, url) =>
                    {
                        Debug.WriteLine($"Variant clicked, will lookup '{url}'");
                        await GetVariantAsync(url);
                    };

                    _wordViewModel.Variants.Add(variantVM);
                }
            }

            IsBusy = false;
        }

        internal async Task<WordModel> LookUpWordInDictionaryAsync(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return null;
            }

            (bool isValid, string errorMessage) = _lookUpWord.CheckThatWordIsValid(word);
            if (!isValid)
            {
                await _dialogService.DisplayAlert("Invalid search term", errorMessage, "OK");
                return null;
            }

            WordModel wordModel = null;
            try
            {
                string translatorApiURL = null;
                AppSettings appSettings = _settingsService.LoadSettings();
                if (appSettings.UseTranslator && !string.IsNullOrEmpty(appSettings.TranslatorApiUrl))
                {
                    translatorApiURL = appSettings.TranslatorApiUrl;
                }
                wordModel = await _lookUpWord.LookUpWordAsync(word, new Options(_selectDictionaryViewModel.SelectedParser.SourceLanguage, translatorApiURL));

                if (wordModel == null)
                {
                    await _dialogService.DisplayAlert("Cannot find word", $"Could not find a translation for '{word}'", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("Error occurred while searching translations", ex.Message, "OK");
                return null;
            }

            return wordModel;
        }

        #endregion
    }
}
