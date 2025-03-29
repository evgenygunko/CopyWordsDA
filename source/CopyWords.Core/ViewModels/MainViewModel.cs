using System.Diagnostics;
using CommunityToolkit.Maui.Core.Platform;
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
        private readonly IInstantTranslationService _instantTranslationService;

        private ILookUpWord _lookUpWord;
        private WordViewModel _wordViewModel;

        public MainViewModel(
            ISettingsService settingsService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            IInstantTranslationService instantTranslationService,
            ILookUpWord lookUpWord,
            WordViewModel wordViewModel)
        {
            _settingsService = settingsService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _instantTranslationService = instantTranslationService;

            _lookUpWord = lookUpWord;
            _wordViewModel = wordViewModel;

            searchWord = string.Empty;
        }

        #region Properties

        public WordViewModel WordViewModel => _wordViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private bool isBusy;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string dictionaryName = default!;

        [ObservableProperty]
        private string dictionaryImage = default!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private string searchWord;

        public bool CanExecuteLookUp => !IsBusy && !string.IsNullOrWhiteSpace(SearchWord);

        public bool CanShowSettingsDialog => !IsBusy;

        #endregion

        #region Commands

        [RelayCommand]
        public async Task InitAsync()
        {
            string? instantText = _instantTranslationService.GetTextAndClear();
            if (!string.IsNullOrWhiteSpace(instantText))
            {
                SearchWord = instantText;
                await LookUpAsync(null, CancellationToken.None);
            }

            DictionaryName = _settingsService.GetSelectedParser();
            if (DictionaryName == SourceLanguage.Danish.ToString())
            {
                DictionaryImage = "flag_of_denmark.png";
            }
            else if (DictionaryName == SourceLanguage.Spanish.ToString())
            {
                DictionaryImage = "flag_of_spain.png";
            }
            else
            {
                throw new NotSupportedException($"Source language '{DictionaryName}' is not supported.");
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteLookUp))]
        public async Task LookUpAsync(ITextInput? entryElement, CancellationToken token)
        {
            try
            {
                if (!OperatingSystem.IsMacCatalyst() && entryElement != null)
                {
                    await entryElement.HideKeyboardAsync(token);
                }
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            WordModel? wordModel = new WordModel(SearchWord, null, null, Enumerable.Empty<Definition>(), Enumerable.Empty<Variant>());
            UpdateUI(wordModel);

            IsBusy = true;

            wordModel = await LookUpWordInDictionaryAsync(SearchWord);
            UpdateUI(wordModel);

            IsBusy = false;
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            IsRefreshing = true;

            WordModel? wordModel = await LookUpWordInDictionaryAsync(SearchWord);
            UpdateUI(wordModel);

            IsRefreshing = false;
        }

        [RelayCommand(CanExecute = nameof(CanShowSettingsDialog))]
        public async Task ShowSettingsDialog()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }

        [RelayCommand]
        public async Task SelectDictionaryAsync()
        {
            string[] strings = [SourceLanguage.Danish.ToString(), SourceLanguage.Spanish.ToString()];
            string result = await _dialogService.DisplayActionSheet(title: "Select dictionary", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, strings);

            // Check that user clicked OK
            // BUG: The action sheet returns "Cancel" on Android.
            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                if (result == SourceLanguage.Danish.ToString())
                {
                    DictionaryName = result;
                    DictionaryImage = "flag_of_denmark.png";
                }
                else if (result == SourceLanguage.Spanish.ToString())
                {
                    DictionaryName = result;
                    DictionaryImage = "flag_of_spain.png";
                }
                else
                {
                    throw new NotSupportedException($"Source language '{result}' selected in the action sheet is not supported.");
                }

                _settingsService.SetSelectedParser(result);
            }
        }

        #endregion

        #region Internal Methods

        internal async Task GetVariantAsync(string url)
        {
            IsBusy = true;

            WordModel? wordModel = null;
            try
            {
                string? translatorApiURL = null;
                AppSettings appSettings = _settingsService.LoadSettings();
                if (appSettings.UseTranslator && !string.IsNullOrEmpty(appSettings.TranslatorApiUrl))
                {
                    translatorApiURL = appSettings.TranslatorApiUrl;
                }

                wordModel = await _lookUpWord.GetWordByUrlAsync(url,
                    new Options(Enum.Parse<SourceLanguage>(appSettings.SelectedParser), translatorApiURL, appSettings.TranslateHeadword, appSettings.TranslateMeanings));

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

        internal void UpdateUI(WordModel? wordModel)
        {
            IsBusy = true;

            if (wordModel != null)
            {
                _wordViewModel.SoundUrl = wordModel.SoundUrl;
                _wordViewModel.SoundFileName = wordModel.SoundFileName;

                _wordViewModel.DefinitionViewModels.Clear();
                foreach (var definition in wordModel.Definitions)
                {
                    _wordViewModel.DefinitionViewModels.Add(new DefinitionViewModel(wordModel.Word, definition, _copySelectedToClipboardService, _dialogService, _clipboardService, _settingsService));
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

        internal async Task<WordModel?> LookUpWordInDictionaryAsync(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return null;
            }

            (bool isValid, string? errorMessage) = _lookUpWord.CheckThatWordIsValid(word);
            if (!isValid)
            {
                await _dialogService.DisplayAlert("Invalid search term", errorMessage ?? string.Empty, "OK");
                return null;
            }

            WordModel? wordModel = null;
            try
            {
                string? translatorApiURL = null;
                AppSettings appSettings = _settingsService.LoadSettings();
                if (appSettings.UseTranslator && !string.IsNullOrEmpty(appSettings.TranslatorApiUrl))
                {
                    translatorApiURL = appSettings.TranslatorApiUrl;
                }
                wordModel = await _lookUpWord.LookUpWordAsync(word,
                    new Options(Enum.Parse<SourceLanguage>(appSettings.SelectedParser), translatorApiURL, appSettings.TranslateHeadword, appSettings.TranslateMeanings));

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
