// Ignore Spelling: snackbar

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IInstantTranslationService _instantTranslationService;
        private readonly ITranslationsService _translationsService;
        private readonly ILaunchDarklyService _launchDarklyService;
        private readonly ISuggestionsService _suggestionsService;
        private readonly INavigationHistory _navigationHistory;
        private readonly IShellService _shellService;
        private readonly IConnectivityService _connectivityService;
        private readonly IDeviceInfo _deviceInfo;

        private readonly IWordViewModel _wordViewModel;

        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposed;
        private bool _isInitialized;

        public MainViewModel(
            ISettingsService settingsService,
            IDialogService dialogService,
            IInstantTranslationService instantTranslationService,
            ITranslationsService translationsService,
            ILaunchDarklyService launchDarklyService,
            ISuggestionsService suggestionsService,
            INavigationHistory navigationHistory,
            IShellService shellService,
            IConnectivityService connectivityService,
            IDeviceInfo deviceInfo,
            IWordViewModel wordViewModel)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _instantTranslationService = instantTranslationService;
            _translationsService = translationsService;
            _launchDarklyService = launchDarklyService;
            _suggestionsService = suggestionsService;
            _navigationHistory = navigationHistory;
            _shellService = shellService;

            _connectivityService = connectivityService;
            _connectivityService.ConnectivityChanged += OnConnectivityChanged;

            _deviceInfo = deviceInfo;

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

        public ObservableCollection<VariantViewModel> SuggestionViewModels { get; } = [];

        public bool ShowSuggestions => SuggestionViewModels.Count > 0;

        #endregion

        #region Commands

        [RelayCommand]
        public async Task InitAsync()
        {
            if (_disposed || IsBusy)
            {
                return;
            }

            // Cancel any running http calls and start a new lookup
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();

            DictionaryName = _settingsService.GetSelectedParser();
            UpdateDictionaryImage(DictionaryName);

            if (!_isInitialized)
            {
                _navigationHistory.Clear();
                NotifyNavigationStateChanged();

                _connectivityService.Initialize();
                if (!_connectivityService.TestConnection())
                {
                    await _connectivityService.UpdateConnectivitySnackbarAsync(false, _cancellationTokenSource.Token);
                }
            }

            // Check if the app was called from a context menu on Android (or from History page) and set the search word accordingly
            string? instantText = _instantTranslationService.GetTextAndClear();
            if (!string.IsNullOrEmpty(instantText))
            {
                SearchWord = instantText;
            }

            await LookUpAsync();

            _isInitialized = true;
        }

        [RelayCommand]
        public async Task LookUpAsync()
        {
            if (_disposed)
            {
                return;
            }

            ClearUI();

            IsBusy = true;

            try
            {
                WordModel? wordModel;

                try
                {
                    wordModel = await LookUpWordInDictionaryAsync(SearchWord);
                }
                catch (WordNotFoundException ex)
                {
                    if (await TryShowSuggestedWordsAsync(ex.SearchedWord))
                    {
                        return;
                    }

                    await _dialogService.DisplayAlertAsync("Cannot find word", $"Could not find a translation for '{ex.SearchedWord}'", "OK");
                    return;
                }

                UpdateUI(wordModel);

                // Add current search word to navigation history
                if (wordModel != null)
                {
                    _settingsService.AddToHistory(wordModel.Word);

                    _navigationHistory.Push(wordModel.Word, wordModel.SourceLanguage.ToString());
                    NotifyNavigationStateChanged();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

#if ANDROID
        [RelayCommand(CanExecute = nameof(CanRefresh))]
#else
        [RelayCommand]
#endif
        public async Task RefreshAsync()
        {
            if (_disposed)
            {
                return;
            }

            IsRefreshing = true;

            try
            {
                WordModel? wordModel;
                try
                {
                    wordModel = await LookUpWordInDictionaryAsync(SearchWord);
                }
                catch (WordNotFoundException ex)
                {
                    await _dialogService.DisplayAlertAsync("Cannot find word", $"Could not find a translation for '{ex.SearchedWord}'", "OK");
                    return;
                }

                UpdateUI(wordModel);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanOpenHistory))]
        public async Task ShowHistory()
        {
            await _shellService.GoToAsync("HistoryPage");
        }

        [RelayCommand(CanExecute = nameof(CanShowSettingsDialog))]
        public async Task ShowSettingsDialog()
        {
            await _shellService.GoToAsync("SettingsPage");
        }

        [RelayCommand(CanExecute = nameof(CanSelectDictionary))]
        public async Task SelectDictionaryAsync()
        {
            string[] strings = [nameof(SourceLanguage.Danish), nameof(SourceLanguage.Spanish)];
            string result = await _dialogService.DisplayActionSheetAsync(title: "Select dictionary:", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, strings);

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
            if (_disposed || !CanNavigateBack)
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
            if (_disposed || string.IsNullOrWhiteSpace(inputText))
            {
                return new List<string>();
            }

            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            List<string> suggestions = (await _suggestionsService.GetSuggestionsAsync(inputText, combinedCts.Token)).ToList();

            // On Android we have a keyboard open, so show only 6 elements so that they fit the screen above the keyboard.
            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                return suggestions.Take(6).ToList();
            }

            return suggestions;
        }

        #endregion

        #region Internal Methods

        internal async Task GetVariantAsync(string url)
        {
            IsBusy = true;

            try
            {
                WordModel? wordModel;
                try
                {
                    wordModel = await LookUpWordInDictionaryAsync(url);
                }
                catch (WordNotFoundException ex)
                {
                    await _dialogService.DisplayAlertAsync("Cannot find word", $"Could not find a translation for '{ex.SearchedWord}'", "OK");
                    return;
                }

                UpdateUI(wordModel);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal void UpdateUI(WordModel? wordModel)
        {
            if (_disposed)
            {
                return;
            }

            SuggestionViewModels.Clear();
            OnPropertyChanged(nameof(ShowSuggestions));

            if (wordModel != null)
            {
                _wordViewModel.Word = wordModel.Word;
                _wordViewModel.SoundUrl = wordModel.SoundUrl;

                if (wordModel.Definition is not null)
                {
                    bool showCheckBoxes = false;
                    if (_deviceInfo.Platform == DevicePlatform.Android)
                    {
                        showCheckBoxes = _settingsService.GetShowAnkiButton();
                    }
                    else
                    {
                        // On Windows and MacCatalyst a user might want to hide copy buttons, but keep Anki button visible.
                        showCheckBoxes = _settingsService.GetShowCopyButtons() || _settingsService.GetShowAnkiButton();
                    }

                    var definitionVM = new DefinitionViewModel(
                        wordModel.Definition,
                        wordModel.SourceLanguage,
                        showCheckBoxes,
                        _settingsService.GetDestinationLanguage());
                    definitionVM.MeaningLookupClicked += async (sender, url) =>
                    {
                        Debug.WriteLine($"Meaning lookup clicked, will lookup '{url}'");
                        await GetVariantAsync(url);
                    };
                    _wordViewModel.SetDefinition(definitionVM);

                    _wordViewModel.ClearVariants();
                    foreach (var variant in wordModel.Variants)
                    {
                        var variantVM = new VariantViewModel(variant);
                        variantVM.Clicked += async (sender, url) =>
                        {
                            Debug.WriteLine($"Variant clicked, will lookup '{url}'");
                            await GetVariantAsync(url);
                        };

                        _wordViewModel.AddVariant(variantVM);
                    }

                    _wordViewModel.ClearExpressions();
                    foreach (var expression in wordModel.Expressions)
                    {
                        var expressionVM = new VariantViewModel(expression);
                        expressionVM.Clicked += async (sender, url) =>
                        {
                            Debug.WriteLine($"Expression clicked, will lookup '{url}'");
                            await GetVariantAsync(url);
                        };

                        _wordViewModel.AddExpression(expressionVM);
                    }
                }

                _settingsService.SetSelectedParser(wordModel.SourceLanguage.ToString());
                DictionaryName = wordModel.SourceLanguage.ToString();
                UpdateDictionaryImage(DictionaryName);
            }

            _wordViewModel.UpdateUI();
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
                wordModel = await _translationsService.LookUpWordAsync(searchTerm, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (WordNotFoundException)
            {
                throw;
            }
            catch (InvalidInputException ex)
            {
                await _dialogService.DisplayAlertAsync("Search input is invalid", ex.Message, "OK");
                return null;
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("An error occurred while searching for translations", ex.Message, "OK");
                return null;
            }

            return wordModel;
        }

        #endregion

        #region Event Handlers

        private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                await _connectivityService.UpdateConnectivitySnackbarAsync(e.NetworkAccess == NetworkAccess.Internet, _cancellationTokenSource.Token);
            }
            catch (ObjectDisposedException)
            {
                // Swallow exception if disposed during async operation
            }
        }

        #endregion

        #region Private Methods

        private void ClearUI()
        {
            var definition = new Definition(new Headword(string.Empty, null, null), string.Empty, string.Empty, []);
            var emptyWord = new WordModel(string.Empty, GetSourceLanguage(), null, null, definition, [], []);
            UpdateUI(emptyWord);
        }

        private async Task<bool> TryShowSuggestedWordsAsync(string searchedWord)
        {
            if (!_launchDarklyService.GetBooleanFlag("test-suggested-words"))
            {
                return false;
            }

            try
            {
                SuggestedWordsModel suggestedWords = await _translationsService.GetSuggestedWordsAsync(
                    searchedWord, _cancellationTokenSource.Token);

                PopulateSuggestionViewModels(suggestedWords.Words);
                return SuggestionViewModels.Count > 0;
            }
            catch (TaskCanceledException)
            {
                return true;
            }
            catch (InvalidInputException ex)
            {
                await _dialogService.DisplayAlertAsync("Search input is invalid", ex.Message, "OK");
                return true;
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("An error occurred while searching for translations", ex.Message, "OK");
                return true;
            }
        }

        private void PopulateSuggestionViewModels(IEnumerable<string> suggestedWords)
        {
            SuggestionViewModels.Clear();

            foreach (string suggestedWord in suggestedWords)
            {
                var suggestionVM = new VariantViewModel(new Variant(suggestedWord, suggestedWord));
                suggestionVM.Clicked += async (sender, url) =>
                {
                    Debug.WriteLine($"Suggestion clicked, will lookup '{url}'");
                    await GetVariantAsync(url);
                };

                SuggestionViewModels.Add(suggestionVM);
            }

            OnPropertyChanged(nameof(ShowSuggestions));
        }

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

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _connectivityService.ConnectivityChanged -= OnConnectivityChanged;

                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
