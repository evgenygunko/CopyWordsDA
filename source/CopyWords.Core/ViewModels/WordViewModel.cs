using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public interface IWordViewModel
    {
        string? SoundUrl { get; set; }
        string? SoundFileName { get; set; }
        bool ShowCopyButtons { get; set; }

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

        private string _soundUrl = string.Empty;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IShare share)
        {
            _saveSoundFileService = saveSoundFileService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _share = share;
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
        [NotifyCanExecuteChangedFor(nameof(OpenCopyMenuCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShareCommand))]
        public partial bool CanCopyFront { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyPartOfSpeechCommand))]
        public partial bool CanCopyPartOfSpeech { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyEndingsCommand))]
        public partial bool CanCopyEndings { get; set; }

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
                await _dialogService.DisplayAlert("Cannot play sound file", "Cannot play sound file, SoundUrl is null or empty", "OK");
                return;
            }

            IsBusy = true;

            MediaElement mediaElement = (MediaElement)control;
            if (SoundUrl != _soundUrl)
            {
                mediaElement.Source = MediaSource.FromUri(SoundUrl);

                Stopwatch stopwatch = Stopwatch.StartNew();
                // Wait for the media element to be ready playing - but give up after 5 seconds
                while (stopwatch.ElapsedMilliseconds < 5000 &&
                    !(mediaElement.CurrentState == MediaElementState.Stopped || mediaElement.CurrentState == MediaElementState.Paused))
                {
                    Debug.WriteLine($"Waiting for media element to load, current state: {mediaElement.CurrentState}");
                    await Task.Delay(200);
                }
                stopwatch.Stop();

                Debug.WriteLine($"Will play '{SoundUrl}', current state: {mediaElement.CurrentState}");
                mediaElement.Play();
            }
            else
            {
                Debug.WriteLine($"Will play '{SoundUrl}', current state: {mediaElement.CurrentState}");
#if ANDROID
                // On Android calling "Play" again doesn't do anything - we will change the position in current media instead, which will trigger play automatically.
                await mediaElement.SeekTo(TimeSpan.Zero, new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
#else
                // On Windows calling "Play" again will restart the sound.
                if (mediaElement.CurrentState == MediaElementState.Paused)
                {
                    mediaElement.Stop();
                }
                mediaElement.Play();
#endif
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
                await _dialogService.DisplayAlert("Cannot save sound file", "Error occurred while trying to save sound file: " + ex.Message, "OK");
            }

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task OpenCopyMenuAsync()
        {
            // This command is used to open the context menu for copying.
            var options = new List<string>();
            if (CanCopyFront)
            {
                options.Add("Front");
                options.Add("Back");
            }
            if (CanCopyPartOfSpeech)
            {
                options.Add("Part of speech");
            }
            if (CanCopyEndings)
            {
                options.Add("Endings");
            }
            if (CanCopyFront)
            {
                options.Add("Examples");
            }

            string result = await _dialogService.DisplayActionSheet(title: "Select field to copy:", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, options.ToArray());

            // The action sheet returns the button that user pressed, so it can also be "Cancel"
            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                switch (result)
                {
                    case "Front":
                        await CopyFrontAsync();
                        break;
                    case "Back":
                        await CopyBackAsync();
                        break;
                    case "Part of speech":
                        await CopyPartOfSpeechAsync();
                        break;
                    case "Endings":
                        await CopyEndingsAsync();
                        break;
                    case "Examples":
                        await CopyExamplesAsync();
                        break;
                    default:
                        await _dialogService.DisplayAlert("Error", "Unknown action", "OK");
                        break;
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task ShareAsync()
        {
            try
            {
                string textToShare = await _copySelectedToClipboardService.CompileFrontAsync(DefinitionViewModels);
                if (!string.IsNullOrEmpty(textToShare))
                {
                    string translations = await _copySelectedToClipboardService.CompileBackAsync(DefinitionViewModels);

                    await _share.RequestAsync(new ShareTextRequest
                    {
                        Subject = textToShare,  // In AnkiDroid it will be extras.getString(Intent.EXTRA_SUBJECT) and will go to the first edit box
                        Text = translations,    // In AnkiDroid it will be extras.getString(Intent.EXTRA_TEXT) and will go to the second edit box
                        Title = "Share Translations",
                    });
                }
                else
                {
                    await _dialogService.DisplayAlert("Oops!", "You need to select at least one example before sharing.", "OK");
                }
            }
            catch (ExamplesFromSeveralDefinitionsSelectedException ex)
            {
                await _dialogService.DisplayAlert("Cannot share the word", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("Cannot share the word", "Error occurred while trying to share the word: " + ex.Message, "OK");
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
                    await _dialogService.DisplayAlert("Text not copied", "Please select at least one example", "OK");
                }
            }
            catch (ExamplesFromSeveralDefinitionsSelectedException ex)
            {
                await _dialogService.DisplayAlert($"Cannot copy {wordPartName}", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert($"Cannot copy {wordPartName}", $"Error occurred while trying to copy {wordPartName}: " + ex.Message, "OK");
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
