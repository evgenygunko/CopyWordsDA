using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Constants;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.ViewModels
{
    public interface IWordViewModel
    {
        string Word { get; set; }
        string? SoundUrl { get; set; }
        bool ShowCopyButtons { get; set; }
        bool ShowAnkiButton { get; set; }

        void UpdateUI();
        void SetDefinition(DefinitionViewModel definition);
        void ClearVariants();
        void AddVariant(VariantViewModel variant);
        void ClearExpressions();
        void AddExpression(VariantViewModel expression);
    }

    public partial class WordViewModel : ObservableObject, IWordViewModel
    {
        private readonly ISaveSoundFileService _saveSoundFileService;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;
        private readonly ICopySelectedToClipboardService _copySelectedToClipboardService;
        private readonly IShare _share;
        private readonly IDeviceInfo _deviceInfo;
        private readonly IAnkiConnectService _ankiConnectService;
        private readonly IAnkiDroidService _ankiDroidService;
        private readonly ISettingsService _settingsService;
        private readonly IAppThemeService _appThemeService;

        private string _soundUrl = string.Empty;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IShare share,
            IDeviceInfo deviceInfo,
            IAnkiConnectService ankiConnectService,
            IAnkiDroidService ankiDroidService,
            ISettingsService settingsService,
            IAppThemeService appThemeService)
        {
            _saveSoundFileService = saveSoundFileService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _share = share;
            _deviceInfo = deviceInfo;
            _ankiConnectService = ankiConnectService;
            _ankiDroidService = ankiDroidService;
            _settingsService = settingsService;
            _appThemeService = appThemeService;

            // Subscribe to theme changes
            _appThemeService.ThemeChanged += (s, e) => OnPropertyChanged(nameof(ButtonTextColor));
        }

        #region Properties

        public ObservableCollection<VariantViewModel> Variants { get; } = [];

        public ObservableCollection<VariantViewModel> Expressions { get; } = [];

        public bool HasExpressions => Expressions.Count > 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDefinitionVisible))]
        public partial DefinitionViewModel DefinitionViewModel { get; set; } = default!;

        public bool IsDefinitionVisible => !string.IsNullOrEmpty(DefinitionViewModel?.HeadwordViewModel.Original);

        [ObservableProperty]
        public partial string Word { get; set; } = default!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        [NotifyPropertyChangedFor(nameof(SaveSoundButtonColor))]
        public partial string? SoundUrl { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        [NotifyPropertyChangedFor(nameof(SaveSoundButtonColor))]
        public partial bool IsBusy { get; set; }

        public bool CanSaveSoundFile => !IsBusy && !string.IsNullOrEmpty(SoundUrl);

        public bool CanPlaySound => !IsBusy && !string.IsNullOrEmpty(SoundUrl);

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyFrontCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyExamplesCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShareCommand))]
        [NotifyCanExecuteChangedFor(nameof(AddNoteToAnkiCommand))]
        public partial bool CanCopyFront { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyPartOfSpeechCommand))]
        public partial bool CanCopyPartOfSpeech { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyEndingsCommand))]
        public partial bool CanCopyEndings { get; set; }

        [ObservableProperty]
        public partial bool ShowAnkiButton { get; set; }

        [ObservableProperty]
        public partial bool ShowCopyButtons { get; set; }

        public bool ShowShareButton => (_deviceInfo.Platform == DevicePlatform.Android);

        public Color PlaySoundButtonColor => GetButtonColor(CanPlaySound);

        public Color SaveSoundButtonColor => GetButtonColor(CanSaveSoundFile);

        public Color CopyFrontButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyBackButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyPartOfSpeechButtonColor => GetButtonColor(CanCopyPartOfSpeech);

        public Color CopyEndingsButtonColor => GetButtonColor(CanCopyEndings);

        public Color CopyExamplesButtonColor => GetButtonColor(CanCopyFront);

        public Color ButtonTextColor => ThemeColors.GetButtonForegroundColor(_appThemeService.CurrentTheme);

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanPlaySound))]
        public async Task PlaySoundAsync(object control)
        {
            if (string.IsNullOrEmpty(SoundUrl))
            {
                await _dialogService.DisplayAlertAsync("Cannot play sound file", "Cannot play sound file, SoundUrl is null or empty", "OK");
                return;
            }

            IsBusy = true;

            IMediaElement mediaElement = (IMediaElement)control;
            if (SoundUrl != _soundUrl)
            {
                await PlayNewSoundAsync(new MediaElementWrapper(mediaElement), SoundUrl);
            }
            else
            {
                await PlaySameSoundAgainAsync(new MediaElementWrapper(mediaElement), SoundUrl);
            }

            _soundUrl = SoundUrl;

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanSaveSoundFile))]
        public async Task SaveSoundFileAsync(CancellationToken cancellationToken)
        {
            IsBusy = true;

            try
            {
                bool result = await _saveSoundFileService.SaveSoundFileAsync(SoundUrl!, Word, cancellationToken);

                if (result)
                {
                    string textToCopy = _copySelectedToClipboardService.CompileSoundFileName(Word);
                    await _clipboardService.CopyTextToClipboardAsync(textToCopy);
                    await _dialogService.DisplayToast("Sound file saved");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot save sound file", "Error occurred while trying to save sound file: " + ex.Message, "OK");
            }

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task AddNoteToAnkiAsync()
        {
            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                await AddNoteWithAnkiDroidServiceAsync();
            }
            else
            {
                await AddNoteWithAnkiConnectAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task ShareAsync()
        {
            // This command is only available on Android platform
            try
            {
                string subjectToShare = _copySelectedToClipboardService.CompileFront(DefinitionViewModel);

                // Compile the translations - they will go to the second edit box in AnkiDroid
                string textToShare = _copySelectedToClipboardService.CompileTranslations(DefinitionViewModel);

                var shareRequest = new ShareTextRequest
                {
                    Subject = subjectToShare,  // In AnkiDroid it will be extras.getString(Intent.EXTRA_SUBJECT) and will go to the first edit box
                    Text = textToShare,    // In AnkiDroid it will be extras.getString(Intent.EXTRA_TEXT) and will go to the second edit box
                    Title = "Share Translations",
                };
                await _share.RequestAsync(shareRequest);
            }
            catch (ExamplesFromSeveralDefinitionsSelectedException ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot share the word", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot share the word", "Error occurred while trying to share the word: " + ex.Message, "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyFrontAsync()
        {
            await CompileAndCopyToClipboard("front", _copySelectedToClipboardService.CompileFront);
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyBackAsync()
        {
            await CompileAndCopyToClipboard("back", _copySelectedToClipboardService.CompileBack);
            await _copySelectedToClipboardService.SaveImagesAsync(DefinitionViewModel);
        }

        [RelayCommand(CanExecute = nameof(CanCopyPartOfSpeech))]
        public async Task CopyPartOfSpeechAsync()
        {
            await CompileAndCopyToClipboard("word type", _copySelectedToClipboardService.CompilePartOfSpeech);
        }

        [RelayCommand(CanExecute = nameof(CanCopyEndings))]
        public async Task CopyEndingsAsync()
        {
            await CompileAndCopyToClipboard("endings", _copySelectedToClipboardService.CompileEndings);
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyExamplesAsync()
        {
            await CompileAndCopyToClipboard("examples", _copySelectedToClipboardService.CompileExamples);
        }

        #endregion

        #region Public Methods

        public void UpdateUI()
        {
            CanCopyFront = !string.IsNullOrEmpty(DefinitionViewModel?.HeadwordViewModel.Original);
            CanCopyPartOfSpeech = !string.IsNullOrEmpty(DefinitionViewModel?.PartOfSpeech);
            CanCopyEndings = !string.IsNullOrEmpty(DefinitionViewModel?.Endings);

            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                // On Android we don't show copy buttons, it only makes sense on Windows and Mac when you have multi-window.
                // But we do show share button instead.
                ShowCopyButtons = false;
            }
            else
            {
                ShowCopyButtons = _settingsService.GetShowCopyButtons();
            }

            ShowAnkiButton = _settingsService.GetShowAnkiButton();
        }

        public void ClearVariants() => Variants.Clear();

        public void SetDefinition(DefinitionViewModel definition) => DefinitionViewModel = definition;

        public void AddVariant(VariantViewModel variant) => Variants.Add(variant);

        public void ClearExpressions()
        {
            Expressions.Clear();
            OnPropertyChanged(nameof(HasExpressions));
        }

        public void AddExpression(VariantViewModel expression)
        {
            Expressions.Add(expression);
            OnPropertyChanged(nameof(HasExpressions));
        }

        #endregion

        #region Internal methods

        internal async Task PlayNewSoundAsync(IMediaElementWrapper mediaElement, string soundUrl)
        {
            // Playing new sound
            mediaElement.Source = MediaSource.FromUri(soundUrl);

            Debug.WriteLine($"Will play '{soundUrl}', current state: {mediaElement.CurrentState}");

            if (_deviceInfo.Platform == DevicePlatform.WinUI)
            {
                // There is a strange bug on Windows when Media element would try to play the old sound even when the source has changed.
                // The sound would be silent though. So we call Play method again, this time it will play the new sound.
                mediaElement.Play();

                if (string.IsNullOrEmpty(_soundUrl))
                {
                    // If it is first time we play the sound, wait longer.
                    await Task.Delay(2000);
                }
                else
                {
                    await Task.Delay(200);
                }
            }

            mediaElement.Play();
        }

        internal async Task PlaySameSoundAgainAsync(IMediaElementWrapper mediaElement, string soundUrl)
        {
            // Playing the same sound again
            Debug.WriteLine($"Will play '{soundUrl}', current state: {mediaElement.CurrentState}");

            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                // On Android calling "Play" again doesn't do anything - we will change the position in current media instead, which will trigger play automatically.
                await mediaElement.SeekTo(TimeSpan.Zero, new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
            }
            else if (_deviceInfo.Platform == DevicePlatform.WinUI)
            {
                // However on Windows calling "SeekTo" doesn't play sound - we need to call "Play".
                mediaElement.Play();
            }
            else
            {
                // On MacCatalyst we need both
                await mediaElement.SeekTo(TimeSpan.Zero, new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
                mediaElement.Play();
            }
        }

        internal async Task CompileAndCopyToClipboard(string wordPartName, Func<DefinitionViewModel, string> action)
        {
            try
            {
                string textToCopy = action(DefinitionViewModel);

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await _clipboardService.CopyTextToClipboardAsync(textToCopy);
                    await _dialogService.DisplayToast($"{wordPartName[0].ToString().ToUpper(CultureInfo.CurrentCulture)}{wordPartName.AsSpan(1)} copied");
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Text not copied", "Please select at least one example", "OK");
                }
            }
            catch (ExamplesFromSeveralDefinitionsSelectedException ex)
            {
                await _dialogService.DisplayAlertAsync($"Cannot copy {wordPartName}", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync($"Cannot copy {wordPartName}", $"Error occurred while trying to copy {wordPartName}: " + ex.Message, "OK");
            }
        }

        internal async Task AddNoteWithAnkiConnectAsync()
        {
            try
            {
                AppSettings appSettings = _settingsService.LoadSettings();
                string deckName = (appSettings.SelectedParser == SourceLanguage.Spanish.ToString()) ? appSettings.AnkiDeckNameSpanish : appSettings.AnkiDeckNameDanish;

                if (string.IsNullOrEmpty(deckName) || string.IsNullOrEmpty(appSettings.AnkiModelName))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please configure Anki deck name and model name in the settings.", "OK");
                    return;
                }

                string front = _copySelectedToClipboardService.CompileFront(DefinitionViewModel);
                if (string.IsNullOrEmpty(front))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least one example.", "OK");
                    return;
                }

                string back = _copySelectedToClipboardService.CompileBack(DefinitionViewModel);
                if (string.IsNullOrEmpty(back))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least one example.", "OK");
                    return;
                }

                await _copySelectedToClipboardService.SaveImagesAsync(DefinitionViewModel);

                string partOfSpeech = _copySelectedToClipboardService.CompilePartOfSpeech(DefinitionViewModel);
                string endings = _copySelectedToClipboardService.CompileEndings(DefinitionViewModel);
                string examples = _copySelectedToClipboardService.CompileExamples(DefinitionViewModel);

                string? sound = null;
                if (CanSaveSoundFile)
                {
                    var saveFileCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
                    bool result = await _saveSoundFileService.SaveSoundFileAsync(SoundUrl!, Word, saveFileCancellationToken);

                    if (result)
                    {
                        sound = _copySelectedToClipboardService.CompileSoundFileName(Word);
                    }
                }

                var ankiNoteOptions = new AnkiNoteOptions(
                    AllowDuplicate: false,
                    DuplicateScope: "deck",
                    DuplicateScopeOptions: new AnkiDuplicateScopeOptions(
                        DeckName: deckName,
                        CheckChildren: false));

                var note = new AnkiNote(
                    DeckName: deckName,
                    ModelName: appSettings.AnkiModelName,
                    Front: front,
                    Back: back,
                    PartOfSpeech: partOfSpeech,
                    Forms: endings,
                    Example: examples,
                    Sound: sound,
                    Options: ankiNoteOptions);

                var ct = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
                await _ankiConnectService.AddNoteAsync(note, ct);
            }
            catch (AnkiConnectNotRunningException ex)
            {
                await _dialogService.DisplayAlertAsync("AnkiConnect is not running",
                    "Please verify that AnkiConnect is installed: run Anki -> Tools -> Add-ons -> Get Add-ons..." + Environment.NewLine + "Error: " + ex.Message,
                    "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot add note", "Error occurred while trying to add note with AnkiConnect: " + ex.Message, "OK");
            }
        }

        internal async Task AddNoteWithAnkiDroidServiceAsync()
        {
            try
            {
                AppSettings appSettings = _settingsService.LoadSettings();
                string deckName = (appSettings.SelectedParser == SourceLanguage.Spanish.ToString()) ? appSettings.AnkiDeckNameSpanish : appSettings.AnkiDeckNameDanish;

                if (string.IsNullOrEmpty(deckName) || string.IsNullOrEmpty(appSettings.AnkiModelName))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please configure Anki deck name and model name in the settings.", "OK");
                    return;
                }

                var ankiNoteOptions = new AnkiNoteOptions(
                    AllowDuplicate: false,
                    DuplicateScope: "deck",
                    DuplicateScopeOptions: new AnkiDuplicateScopeOptions(
                        DeckName: deckName,
                        CheckChildren: false));

                string front = _copySelectedToClipboardService.CompileFront(DefinitionViewModel);
                if (string.IsNullOrEmpty(front))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least one example.", "OK");
                    return;
                }

                string back = _copySelectedToClipboardService.CompileBack(DefinitionViewModel);
                if (string.IsNullOrEmpty(back))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least one example.", "OK");
                    return;
                }

                string partOfSpeech = _copySelectedToClipboardService.CompilePartOfSpeech(DefinitionViewModel);
                string endings = _copySelectedToClipboardService.CompileEndings(DefinitionViewModel);
                string examples = _copySelectedToClipboardService.CompileExamples(DefinitionViewModel);

                // todo: save images
                // todo: to implement
                string? sound = null;

                var note = new AnkiNote(
                    DeckName: deckName,
                    ModelName: appSettings.AnkiModelName,
                    Front: front,
                    Back: back,
                    PartOfSpeech: partOfSpeech,
                    Forms: endings,
                    Example: examples,
                    Sound: sound,
                    Options: ankiNoteOptions);

                long noteId = await _ankiDroidService.AddNoteAsync(note);
                if (noteId > 0)
                {
                    await _dialogService.DisplayToast("The note has been added to Anki.");
                }
            }
            catch (AnkiDroidAPINotAvailableException ex)
            {
                await _dialogService.DisplayAlertAsync("AnkiDroid API is not accessible",
                    "Please verify that AndkiDroid API is enabled: run AnkiDroid -> Settings -> Advanced -> Enable AndkiDroid API" + Environment.NewLine + "Error: " + ex.Message,
                    "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot add note", "Error occurred while trying to add note with  AndkiDroid API: " + ex.Message, "OK");
            }
        }

        #endregion

        #region Private methods

        private static Color GetButtonColor(bool isEnabled) => ThemeColors.GetButtonBackgroundColor(isEnabled);

        #endregion
    }
}
