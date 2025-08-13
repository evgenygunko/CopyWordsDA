using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IInstantTranslationService _instantTranslationService;
        private readonly ITranslationsService _translationsService;

        private readonly IWordViewModel _wordViewModel;

        public MainViewModel(
            IGlobalSettings globalSettings,
            ISettingsService settingsService,
            IDialogService dialogService,
            IInstantTranslationService instantTranslationService,
            ITranslationsService translationsService,
            IWordViewModel wordViewModel)
        {
            _globalSettings = globalSettings;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _instantTranslationService = instantTranslationService;
            _translationsService = translationsService;
            _wordViewModel = wordViewModel;
            SearchWord = string.Empty;
        }

        #region Properties

        public IWordViewModel WordViewModel => _wordViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
        [NotifyCanExecuteChangedFor(nameof(SelectDictionaryCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowSettingsDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowHistoryCommand))]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectDictionaryCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowSettingsDialogCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowHistoryCommand))]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string DictionaryName { get; set; } = default!;

        [ObservableProperty]
        public partial string DictionaryImage { get; set; } = default!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        public partial string SearchWord { get; set; }

        public bool CanRefresh => !IsBusy;

        public bool CanShowSettingsDialog => !IsBusy && !IsRefreshing;

        public bool CanSelectDictionary => !IsBusy && !IsRefreshing;

        public bool CanOpenHistory => !IsBusy && !IsRefreshing;

        #endregion

        #region Commands

        [RelayCommand]
        public async Task InitAsync()
        {
            if (IsBusy)
            {
                return;
            }

            // Check if the app was called from a context menu on Android (or from History page) and set the search word accordingly
            string? instantText = _instantTranslationService.GetTextAndClear();
            if (!string.IsNullOrWhiteSpace(instantText))
            {
                SearchWord = instantText;
                await LookUpAsync(null, CancellationToken.None);
            }
            else
            {
                WordModel? wordModel = new WordModel(string.Empty, null, null, [], []);
                UpdateUI(wordModel);
            }

            DictionaryName = _settingsService.GetSelectedParser();
            UpdateDictionaryImage(DictionaryName);
        }

        [RelayCommand]
        public async Task LookUpAsync(ITextInput? searchBarElement, CancellationToken token)
        {
            try
            {
                if (searchBarElement is SearchBar searchBar)
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

            WordModel? wordModel = new WordModel(string.Empty, null, null, [], []);
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

        [RelayCommand(CanExecute = nameof(CanOpenHistory))]
        public async Task ShowHistory()
        {
            await Shell.Current.GoToAsync("HistoryPage");
        }

        [RelayCommand(CanExecute = nameof(CanShowSettingsDialog))]
        public async Task ShowSettingsDialog()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }

        [RelayCommand(CanExecute = nameof(CanSelectDictionary))]
        public async Task SelectDictionaryAsync()
        {
            string[] strings = [nameof(SourceLanguage.Danish), nameof(SourceLanguage.Spanish)];
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

            WordModel? wordModel = await LookUpWordInDictionaryAsync(url);
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

                bool showCopyButtons = _settingsService.GetShowCopyButtons();

                _wordViewModel.ClearDefinitions();
                foreach (var definition in wordModel.Definitions)
                {
                    _wordViewModel.AddDefinition(new DefinitionViewModel(definition, sourceLanguage, showCopyButtons));
                }

                _wordViewModel.ClearVariants();
                foreach (var variant in wordModel.Variations)
                {
                    var variantVM = new VariantViewModel(variant);
                    variantVM.Clicked += async (sender, url) =>
                    {
                        Debug.WriteLine($"Variant clicked, will lookup '{url}'");
                        await GetVariantAsync(url);
                    };

                    _wordViewModel.AddVariant(variantVM);
                }

                _wordViewModel.ShowCopyButtons = showCopyButtons;
                _wordViewModel.UpdateUI();

                _settingsService.AddToHistory(wordModel.Word);
            }

            IsBusy = false;
        }

        internal async Task<WordModel?> LookUpWordInDictionaryAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return null;
            }

            WordModel? wordModel;
            try
            {
                AppSettings appSettings = _settingsService.LoadSettings();

                wordModel = await _translationsService.LookUpWordAsync(searchTerm,
                    new Options(Enum.Parse<SourceLanguage>(appSettings.SelectedParser), _globalSettings.TranslatorApiUrl));

                if (wordModel == null)
                {
                    await _dialogService.DisplayAlert("Cannot find word", $"Could not find a translation for '{searchTerm}'", "OK");
                }
            }
            catch (InvalidInputException ex)
            {
                await _dialogService.DisplayAlert("Search input is invalid", ex.Message, "OK");
                return null;
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("An error occurred while searching for translations", ex.Message, "OK");
                return null;
            }

            return wordModel;
        }

        #endregion

        #region Private Methods

        private void UpdateDictionaryImage(string language)
        {
            if (language == nameof(SourceLanguage.Danish))
            {
                DictionaryImage = "flag_of_denmark.png";
            }
            else if (language == nameof(SourceLanguage.Spanish))
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
