using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class WordViewModel : ObservableObject
    {
        private readonly ISaveSoundFileService _saveSoundFileService;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;
        private readonly ICopySelectedToClipboardService _copySelectedToClipboardService;
        private readonly ISettingsService _settingsService;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            ISettingsService settingsService)
        {
            _saveSoundFileService = saveSoundFileService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _settingsService = settingsService;
        }

        #region Properties

        public ObservableCollection<VariantViewModel> Variants { get; } = new();

        public ObservableCollection<DefinitionViewModel> DefinitionViewModels { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private string? soundUrl;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(SaveSoundButtonColor))]
        private string? soundFileName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private bool isBusy;

        public bool CanSaveSoundFile => !IsBusy && !string.IsNullOrEmpty(SoundFileName);

        public bool CanPlaySound => !IsBusy && !string.IsNullOrEmpty(SoundUrl);

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyFrontCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyExamplesCommand))]
        private bool canCopyFront;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyPartOfSpeechCommand))]
        private bool canCopyPartOfSpeech;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyEndingsCommand))]
        private bool canCopyEndings;

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

            Debug.WriteLine("Will play " + SoundUrl);

            var stackSound = (HorizontalStackLayout)control;
            IView? view = stackSound.Children.SingleOrDefault(x => x.GetType() == typeof(MediaElement));

            if (view is not MediaElement)
            {
                await _dialogService.DisplayAlert("Cannot play sound file", "Cannot find MediaElement control in the view", "OK");
                return;
            }

            MediaElement mediaElement = (MediaElement)view;

            // Workaround for a bug in MediaElement:
            // On MacOS, the app crashes at startup with the runtime exception:
            // "System.MissingMethodException: No parameterless constructor defined for type 'CommunityToolkit.Maui.Views.MediaElement'."
            // To resolve this, we create the MediaElement manually in the C# file.
            // However, it must be added to the Visual Tree; otherwise, there is no sound.
            // Reference: https://stackoverflow.com/a/75535084
            //MediaElement mediaElement = (MediaElement)control;
            mediaElement.Source = MediaSource.FromUri(SoundUrl);
            mediaElement.Play();

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
            string[] strings = ["Front", "Back", "Part of speech", "Endings", "Examples"];
            string result = await _dialogService.DisplayActionSheet(title: "Select field to copy:", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, strings);

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

        #region Internal methods

        internal void UpdateUI()
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

        internal async Task CompileAndCopyToClipboard(string wordPartName, Func<ObservableCollection<DefinitionViewModel>, Task<string>> action)
        {
            try
            {
                //string textToCopy = await action(this);
                string textToCopy = await action(DefinitionViewModels);

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await _clipboardService.CopyTextToClipboardAsync(textToCopy);
                    await _dialogService.DisplayToast(string.Concat(wordPartName[0].ToString().ToUpper(CultureInfo.CurrentCulture), wordPartName.AsSpan(1), " copied"));
                }
                else
                {
                    await _dialogService.DisplayAlert("Text not copied", "Please select at least one example", "OK");
                }
            }
            catch (PrepareWordForCopyingException ex)
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
