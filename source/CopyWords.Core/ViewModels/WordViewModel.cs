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
        bool ShowAddNoteWithAnkiConnectButton { get; set; }
        bool ShowShareButton { get; set; }

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
        [NotifyCanExecuteChangedFor(nameof(AddNoteWithAnkiConnectCommand))]
        public partial bool CanCopyFront { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyPartOfSpeechCommand))]
        public partial bool CanCopyPartOfSpeech { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyEndingsCommand))]
        public partial bool CanCopyEndings { get; set; }

        [ObservableProperty]
        public partial bool ShowAddNoteWithAnkiConnectButton { get; set; }

        [ObservableProperty]
        public partial bool ShowCopyButtons { get; set; }

        [ObservableProperty]
        public partial bool ShowShareButton { get; set; }

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
        public async Task AddNoteWithAnkiConnectAsync()
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

                string front = await _copySelectedToClipboardService.CompileFrontAsync(DefinitionViewModel);
                if (string.IsNullOrEmpty(front))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least one example.", "OK");
                    return;
                }

                string back = await _copySelectedToClipboardService.CompileBackAsync(DefinitionViewModel);
                if (string.IsNullOrEmpty(back))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least one example.", "OK");
                    return;
                }

                string partOfSpeech = await _copySelectedToClipboardService.CompilePartOfSpeechAsync(DefinitionViewModel);
                string endings = await _copySelectedToClipboardService.CompileEndingsAsync(DefinitionViewModel);
                string examples = await _copySelectedToClipboardService.CompileExamplesAsync(DefinitionViewModel);

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

                var addNoteCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
                await _ankiConnectService.AddNoteAsync(note, addNoteCancellationToken);
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

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task ShareAsync()
        {
            try
            {
                string subjectToShare = string.Empty;
                string textToShare = string.Empty;

                if (ShowCopyButtons)
                {
                    // When checkboxes are shown, allow a user to select any meanings to share. The shared text will have html formatting
                    // to make it look nice in AnkiDroid.
                    subjectToShare = await _copySelectedToClipboardService.CompileFrontAsync(DefinitionViewModel);
                    textToShare = await _copySelectedToClipboardService.CompileBackAsync(DefinitionViewModel);
                }

                if (string.IsNullOrEmpty(textToShare))
                {
                    // If the checkboxes are not shown, or a user didn't select any checkboxes, share the headword and its translations.
                    // The shared text will not have any special formatting.
                    textToShare = _copySelectedToClipboardService.CompileHeadword(DefinitionViewModel);
                }

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
            await CompileAndCopyToClipboard("front", _copySelectedToClipboardService.CompileFrontAsync);
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyBackAsync()
        {
            await CompileAndCopyToClipboard("back", _copySelectedToClipboardService.CompileBackAsync);
        }

        [RelayCommand(CanExecute = nameof(CanCopyPartOfSpeech))]
        public async Task CopyPartOfSpeechAsync()
        {
            await CompileAndCopyToClipboard("word type", _copySelectedToClipboardService.CompilePartOfSpeechAsync);
        }

        [RelayCommand(CanExecute = nameof(CanCopyEndings))]
        public async Task CopyEndingsAsync()
        {
            await CompileAndCopyToClipboard("endings", _copySelectedToClipboardService.CompileEndingsAsync);
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyExamplesAsync()
        {
            await CompileAndCopyToClipboard("examples", _copySelectedToClipboardService.CompileExamplesAsync);
        }

        #endregion

        #region Public Methods

        public void UpdateUI()
        {
            CanCopyFront = !string.IsNullOrEmpty(DefinitionViewModel?.HeadwordViewModel.Original);

            CanCopyPartOfSpeech = !string.IsNullOrEmpty(DefinitionViewModel?.PartOfSpeech);

            CanCopyEndings = !string.IsNullOrEmpty(DefinitionViewModel?.Endings);
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

        internal async Task CompileAndCopyToClipboard(string wordPartName, Func<DefinitionViewModel, Task<string>> action)
        {
            try
            {
                string textToCopy = await action(DefinitionViewModel);

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

        #endregion

        #region Private methods

        private static Color GetButtonColor(bool isEnabled) => ThemeColors.GetButtonBackgroundColor(isEnabled);

        #endregion
    }
}
