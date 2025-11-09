// Ignore Spelling: snackbar

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
        private readonly ISuggestionsService _suggestionsService;
        private readonly INavigationHistory _navigationHistory;
        private readonly IConnectivityService _connectivityService;

        private readonly IWordViewModel _wordViewModel;

        // Add cancellation token source to cancel ongoing operations during dispose
        private CancellationTokenSource _cancellationTokenSource = new();

        public MainViewModel(
            IGlobalSettings globalSettings,
            ISettingsService settingsService,
            IDialogService dialogService,
            IInstantTranslationService instantTranslationService,
            ITranslationsService translationsService,
            ISuggestionsService suggestionsService,
            INavigationHistory navigationHistory,
            IConnectivityService connectivityService,
            IWordViewModel wordViewModel)
        {
            _globalSettings = globalSettings;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _instantTranslationService = instantTranslationService;
            _translationsService = translationsService;
            _suggestionsService = suggestionsService;
            _navigationHistory = navigationHistory;

            _connectivityService = connectivityService;
            _connectivityService.ConnectivityChanged += async (object? sender, ConnectivityChangedEventArgs e) =>
            {
                await _connectivityService.UpdateConnectivitySnackbarAsync(e.NetworkAccess == NetworkAccess.Internet, _cancellationTokenSource.Token);
            };

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
        [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string DictionaryName { get; set; } = default!;

        [ObservableProperty]
        public partial string DictionaryImage { get; set; } = default!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
        public partial string SearchWord { get; set; }

        public bool CanRefresh => !IsBusy;

        public bool CanShowSettingsDialog => !IsBusy && !IsRefreshing;

        public bool CanSelectDictionary => !IsBusy && !IsRefreshing;

        public bool CanOpenHistory => !IsBusy && !IsRefreshing;

        public bool CanNavigateBack => !IsBusy && !IsRefreshing && _navigationHistory.CanNavigateBack;

        #endregion

        #region Commands

        [RelayCommand]
        public async Task InitAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();

            DictionaryName = _settingsService.GetSelectedParser();
            UpdateDictionaryImage(DictionaryName);

            _navigationHistory.Clear();
            NotifyNavigationStateChanged();

            _connectivityService.Initialize();
            if (!_connectivityService.TestConnection())
            {
                await _connectivityService.UpdateConnectivitySnackbarAsync(false, _cancellationTokenSource.Token);
            }

            // Check if the app was called from a context menu on Android (or from History page) and set the search word accordingly
            string instantText = _instantTranslationService.GetTextAndClear() ?? string.Empty;
            SearchWord = instantText;
            await LookUpAsync();
        }

        [RelayCommand]
        public async Task LookUpAsync()
        {
            // Clear previous word while we are waiting for the new one
            WordModel? wordModel = new WordModel(string.Empty, GetSourceLanguage(), null, null, [], []);
            UpdateUI(wordModel);

            IsBusy = true;

            wordModel = await LookUpWordInDictionaryAsync(SearchWord);
            UpdateUI(wordModel);

            // Add current search word to navigation history
            if (wordModel != null)
            {
                _settingsService.AddToHistory(wordModel.Word);

                _navigationHistory.Push(wordModel.Word, wordModel.SourceLanguage.ToString());
                NotifyNavigationStateChanged();
            }

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

        [RelayCommand(CanExecute = nameof(CanNavigateBack))]
        public async Task<bool> NavigateBackAsync()
        {
            if (!CanNavigateBack)
            {
                return false;
            }

            NavigationEntry? previousEntry = null;
            while (_navigationHistory.Count > 0)
            {
                // Get the previous word from history
                previousEntry = _navigationHistory.Pop();
                NotifyNavigationStateChanged();

                if (SearchWord != previousEntry.Value.Word)
                {
                    break;
                }

                previousEntry = null;
            }

            if (previousEntry is null)
            {
                return false;
            }

            // Update the search word and perform lookup
            SearchWord = previousEntry.Value.Word;
            _settingsService.SetSelectedParser(previousEntry.Value.Dictionary);

            await LookUpAsync();

            return true;
        }

        #endregion

        #region Public Methods

        public async Task<List<string>> GetSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            // This method is called from the autocomplete control on every key press: CustomAsyncFilter.
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return new List<string>();
            }

            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);

            List<string> suggestions = new List<string>();

            SourceLanguage sourceLanguage;
            if (Enum.TryParse<SourceLanguage>(_settingsService.GetSelectedParser(), out sourceLanguage))
            {
                if (sourceLanguage == SourceLanguage.Danish)
                {
                    suggestions = (await _suggestionsService.GetDanishWordsSuggestionsAsync(inputText, combinedCts.Token)).ToList();
                }
                else if (sourceLanguage == SourceLanguage.Spanish)
                {
                    suggestions = (await _suggestionsService.GetSpanishWordsSuggestionsAsync(inputText, combinedCts.Token)).ToList();
                }
            }

#if ANDROID
            // On Android we have a keyboard open, so show only 6 elements so that they fit the screen above the keyboard.
            return suggestions.Take(6).ToList();
#else
            return suggestions;
#endif
        }

        public void CancelHttpRequests() => _cancellationTokenSource.Cancel();

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

                bool showCopyButtons = _settingsService.GetShowCopyButtons();

                _wordViewModel.ClearDefinitions();
                foreach (var definition in wordModel.Definitions)
                {
                    _wordViewModel.AddDefinition(new DefinitionViewModel(definition, wordModel.SourceLanguage, showCopyButtons));
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

                _settingsService.SetSelectedParser(wordModel.SourceLanguage.ToString());
                DictionaryName = wordModel.SourceLanguage.ToString();
                UpdateDictionaryImage(DictionaryName);
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
                    new Models.Options(Enum.Parse<SourceLanguage>(appSettings.SelectedParser), _globalSettings.TranslatorAppUrl, _globalSettings.TranslatorAppRequestCode),
                    _cancellationTokenSource.Token);

                if (wordModel == null)
                {
                    await _dialogService.DisplayAlert("Cannot find word", $"Could not find a translation for '{searchTerm}'", "OK");
                }
            }
            catch (TaskCanceledException)
            {
                return null;
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

        private void NotifyNavigationStateChanged()
        {
            NavigateBackCommand.NotifyCanExecuteChanged();
        }

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

        private SourceLanguage GetSourceLanguage()
        {
            SourceLanguage sourceLanguage;
            if (!Enum.TryParse<SourceLanguage>(_settingsService.GetSelectedParser(), out sourceLanguage))
            {
                sourceLanguage = SourceLanguage.Danish;
            }
            return sourceLanguage;
        }

        #endregion
    }
}
