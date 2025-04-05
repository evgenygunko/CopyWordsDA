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
        private readonly IDialogService _dialogService;
        private readonly IInstantTranslationService _instantTranslationService;

        private ILookUpWord _lookUpWord;
        private WordViewModel _wordViewModel;

        public MainViewModel(
            ISettingsService settingsService,
            IDialogService dialogService,
            IInstantTranslationService instantTranslationService,
            ILookUpWord lookUpWord,
            WordViewModel wordViewModel)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _instantTranslationService = instantTranslationService;

            _lookUpWord = lookUpWord;
            _wordViewModel = wordViewModel;

            searchWord = string.Empty;
        }

        #region Properties

        public WordViewModel WordViewModel => _wordViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
        [NotifyCanExecuteChangedFor(nameof(SelectDictionaryCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowSettingsDialogCommand))]
        private bool isBusy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectDictionaryCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowSettingsDialogCommand))]
        private bool isRefreshing;

        [ObservableProperty]
        private string dictionaryName = default!;

        [ObservableProperty]
        private string dictionaryImage = default!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private string searchWord;

        public bool CanRefresh => !IsBusy;

        public bool CanShowSettingsDialog => !IsBusy && !IsRefreshing;

        public bool CanSelectDictionary => !IsBusy && !IsRefreshing;

        #endregion

        #region Commands

        [RelayCommand]
        public async Task InitAsync()
        {
            if (IsBusy)
            {
                return;
            }

            string? instantText = _instantTranslationService.GetTextAndClear();
            if (!string.IsNullOrWhiteSpace(instantText))
            {
                SearchWord = instantText;
                await LookUpAsync(null, CancellationToken.None);
            }

            DictionaryName = _settingsService.GetSelectedParser();
            UpdateDictionaryImage(DictionaryName);
        }

        [RelayCommand]
        public async Task LookUpAsync(ITextInput? searchBarElement, CancellationToken token)
        {
            try
            {
                SearchBar? searchBar = searchBarElement as SearchBar;
                if (searchBar != null)
                {
                    if (searchBar.IsSoftInputShowing())
                    {
                        await searchBar.HideSoftInputAsync(token);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            WordModel? wordModel = new WordModel(string.Empty, null, null, Enumerable.Empty<Definition>(), Enumerable.Empty<Variant>());
            UpdateUI(wordModel);

            IsBusy = true;

            wordModel = await LookUpWordInDictionaryAsync(SearchWord);
            UpdateUI(wordModel);

            IsBusy = false;
        }

#if ANDROID
        [RelayCommand(CanExecute = nameof(CanRefresh))]
#else
        [RelayCommand]
#endif
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

        [RelayCommand(CanExecute = nameof(CanSelectDictionary))]
        public async Task SelectDictionaryAsync()
        {
            string[] strings = [SourceLanguage.Danish.ToString(), SourceLanguage.Spanish.ToString()];
            string result = await _dialogService.DisplayActionSheet(title: "Select dictionary:", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, strings);

            // The action sheet returns the button that user pressed, so it can also be "Cancel"
            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                DictionaryName = result;
                UpdateDictionaryImage(result);

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

                SourceLanguage sourceLanguage;
                if (!Enum.TryParse<SourceLanguage>(_settingsService.GetSelectedParser(), out sourceLanguage))
                {
                    sourceLanguage = SourceLanguage.Danish;
                }

                _wordViewModel.DefinitionViewModels.Clear();
                foreach (var definition in wordModel.Definitions)
                {
                    _wordViewModel.DefinitionViewModels.Add(new DefinitionViewModel(definition, sourceLanguage));
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

                _wordViewModel.UpdateUI();
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

        #region Private Methods

        private void UpdateDictionaryImage(string language)
        {
            if (language == SourceLanguage.Danish.ToString())
            {
                DictionaryImage = "flag_of_denmark.png";
            }
            else if (language == SourceLanguage.Spanish.ToString())
            {
                DictionaryImage = "flag_of_spain.png";
            }
            else
            {
                throw new NotSupportedException($"Source language '{language}' selected in the action sheet is not supported.");
            }
        }

        #endregion
    }
}
