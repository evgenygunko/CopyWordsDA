using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.ViewModels
{
    public interface IWordViewModel
    {
        string? SoundUrl { get; set; }
        string? SoundFileName { get; set; }
        bool ShowCopyButtons { get; set; }
        bool ShowAddNoteWithAnkiConnectButton { get; set; }

        void UpdateUI();
        void ClearDefinitions();
        void ClearVariants();
        void AddDefinition(DefinitionViewModel definition);
        void AddVariant(VariantViewModel variant);
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

        private string _soundUrl = string.Empty;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IShare share,
            IDeviceInfo deviceInfo,
            IAnkiConnectService ankiConnectService)
        {
            _saveSoundFileService = saveSoundFileService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _share = share;
            _deviceInfo = deviceInfo;
            _ankiConnectService = ankiConnectService;
        }

        #region Properties

        public ObservableCollection<VariantViewModel> Variants { get; } = [];

        public ObservableCollection<DefinitionViewModel> DefinitionViewModels { get; } = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        public partial string? SoundUrl { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(SaveSoundButtonColor))]
        public partial string? SoundFileName { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        public partial bool IsBusy { get; set; }

        public bool CanSaveSoundFile => !IsBusy && !string.IsNullOrEmpty(SoundFileName);

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

        public Color PlaySoundButtonColor => GetButtonColor(CanPlaySound);

        public Color SaveSoundButtonColor => GetButtonColor(CanSaveSoundFile);

        public Color CopyFrontButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyBackButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyPartOfSpeechButtonColor => GetButtonColor(CanCopyPartOfSpeech);

        public Color CopyEndingsButtonColor => GetButtonColor(CanCopyEndings);

        public Color CopyExamplesButtonColor => GetButtonColor(CanCopyFront);

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

            MediaElement mediaElement = (MediaElement)control;
            if (SoundUrl != _soundUrl)
            {
                mediaElement.Source = MediaSource.FromUri(SoundUrl);

                Debug.WriteLine($"Will play '{SoundUrl}', current state: {mediaElement.CurrentState}");
                mediaElement.Play();

                if (_deviceInfo.Platform == DevicePlatform.WinUI)
                {
                    if (!string.IsNullOrEmpty(_soundUrl))
                    {
                        // There is a strange bug on Windows when Media element would try to play the old sound even when the source has changed.
                        // The sound would be silent though. So we call Play method again, this time it will play the new sound.
                        await Task.Delay(200);
                        mediaElement.Play();
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Will play '{SoundUrl}', current state: {mediaElement.CurrentState}");

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
            _soundUrl = SoundUrl;

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanSaveSoundFile))]
        public async Task SaveSoundFileAsync(CancellationToken cancellationToken)
        {
            IsBusy = true;

            try
            {
                bool result = await _saveSoundFileService.SaveSoundFileAsync(SoundUrl!, SoundFileName!, cancellationToken);

                if (result)
                {
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
                string front = await _copySelectedToClipboardService.CompileFrontAsync(DefinitionViewModels);
                if (string.IsNullOrEmpty(front))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least example.", "OK");
                    return;
                }

                string back = await _copySelectedToClipboardService.CompileBackAsync(DefinitionViewModels);
                if (string.IsNullOrEmpty(back))
                {
                    await _dialogService.DisplayAlertAsync("Cannot add note", "Please select at least example.", "OK");
                    return;
                }

                string partOfSpeech = await _copySelectedToClipboardService.CompilePartOfSpeechAsync(DefinitionViewModels);
                string endings = await _copySelectedToClipboardService.CompileEndingsAsync(DefinitionViewModels);
                string examples = await _copySelectedToClipboardService.CompileExamplesAsync(DefinitionViewModels);

                // todo: get them from Settings dialog
                const string deckName = "Test";
                const string modelName = "Основная";

                var ankiNoteOptions = new AnkiNoteOptions(
                    AllowDuplicate: false,
                    DuplicateScope: "deck",
                    DuplicateScopeOptions: new AnkiDuplicateScopeOptions(
                        DeckName: deckName,
                        CheckChildren: false));

                var note = new AnkiNote(
                    DeckName: deckName,
                    ModelName: modelName,
                    Front: front,
                    Back: back,
                    PartOfSpeech: partOfSpeech,
                    Forms: endings,
                    Example: examples,
                    Sound: null,
                    Options: ankiNoteOptions);
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

                await _ankiConnectService.AddNoteAsync(note, cancellationToken);
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
                    subjectToShare = await _copySelectedToClipboardService.CompileFrontAsync(DefinitionViewModels);
                    textToShare = await _copySelectedToClipboardService.CompileBackAsync(DefinitionViewModels);
                }

                if (string.IsNullOrEmpty(subjectToShare))
                {
                    // If the checkboxes are not shown, or a user didn't select any checkboxes, share the headword and its translations.
                    // The shared text will not have any special formatting.
                    subjectToShare = _copySelectedToClipboardService.CompileHeadword(DefinitionViewModels);
                    textToShare = subjectToShare;
                }

                await _share.RequestAsync(new ShareTextRequest
                {
                    Subject = subjectToShare,  // In AnkiDroid it will be extras.getString(Intent.EXTRA_SUBJECT) and will go to the first edit box
                    Text = textToShare,    // In AnkiDroid it will be extras.getString(Intent.EXTRA_TEXT) and will go to the second edit box
                    Title = "Share Translations",
                });
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
            CanCopyFront = DefinitionViewModels.Any();

            bool canCopyPartOfSpeech = false;
            foreach (var definitionViewModel in DefinitionViewModels)
            {
                if (!string.IsNullOrEmpty(definitionViewModel.PartOfSpeech))
                {
                    canCopyPartOfSpeech = true;
                    break;
                }
            }
            CanCopyPartOfSpeech = canCopyPartOfSpeech;

            bool canCopyEndings = false;
            foreach (var definitionViewModel in DefinitionViewModels)
            {
                if (!string.IsNullOrEmpty(definitionViewModel.Endings))
                {
                    canCopyEndings = true;
                    break;
                }
            }
            CanCopyEndings = canCopyEndings;
        }

        public void ClearDefinitions() => DefinitionViewModels.Clear();

        public void ClearVariants() => Variants.Clear();

        public void AddDefinition(DefinitionViewModel definition) => DefinitionViewModels.Add(definition);

        public void AddVariant(VariantViewModel variant) => Variants.Add(variant);

        #endregion

        #region Internal methods

        internal async Task CompileAndCopyToClipboard(string wordPartName, Func<ObservableCollection<DefinitionViewModel>, Task<string>> action)
        {
            try
            {
                string textToCopy = await action(DefinitionViewModels);

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

        private static Color GetButtonColor(bool isEnabled)
        {
            Color color = isEnabled ? Color.Parse("#512BD4") : Color.Parse("#919191");
            return color;
        }

        #endregion
    }
}
