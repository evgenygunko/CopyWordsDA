using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class WordViewModel : ObservableObject
    {
        private readonly ISaveSoundFileService _saveSoundFileService;
        private readonly ICopySelectedToClipboardService _copySelectedToClipboardService;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IDialogService dialogService,
            IClipboardService clipboardService)
        {
            _saveSoundFileService = saveSoundFileService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;

            Headword = new HeadwordViewModel(default);
        }

        #region Properties

        public ObservableCollection<VariantViewModel> Variants { get; } = new();

        public ObservableCollection<DefinitionViewModel> Definitions { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyFrontCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyFormsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyExamplesCommand))]
        private string front = "<>";

        [ObservableProperty]
        private HeadwordViewModel headword;

        [ObservableProperty]
        private string partOfSpeech = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyFormsCommand))]
        private string forms = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private string _soundUrl;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(SaveSoundButtonColor))]
        private string _soundFileName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyFrontCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyFormsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyExamplesCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private bool isBusy;

        public bool CanSaveSoundFile => !IsBusy && !string.IsNullOrEmpty(SoundFileName);

        public bool CanPlaySound => !IsBusy && !string.IsNullOrEmpty(SoundUrl);

        public bool CanCopyFront => !IsBusy && Front != "<>";

        public bool CanCopyForms => !IsBusy && !string.IsNullOrEmpty(Forms);

        public Color PlaySoundButtonColor => GetButtonColor(CanPlaySound);

        public Color SaveSoundButtonColor => GetButtonColor(CanSaveSoundFile);

        public Color CopyFrontButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyBackButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyFormsButtonColor => GetButtonColor(CanCopyForms);

        public Color CopyExamplesButtonColor => GetButtonColor(CanCopyFront);

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanPlaySound))]
        public void PlaySound(object control)
        {
            IsBusy = true;

            Debug.WriteLine("Will play " + SoundUrl);

            MediaElement mediaElement = (MediaElement)control;
            mediaElement.Source = MediaSource.FromUri(SoundUrl);
            mediaElement.Play();

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanSaveSoundFile))]
        public async Task SaveSoundFileAsync()
        {
            IsBusy = true;

            try
            {
                bool result = await _saveSoundFileService.SaveSoundFileAsync(SoundUrl, SoundFileName);

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
        public async Task CopyFrontAsync()
        {
            string formattedText = await _copySelectedToClipboardService.CompileFrontAsync(Front, PartOfSpeech);

            await _clipboardService.CopyTextToClipboardAsync(formattedText);
            await _dialogService.DisplayToast("Front copied");
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyBackAsync()
        {
            try
            {
                string textToCopy = await _copySelectedToClipboardService.CompileBackAsync(Definitions, Headword);

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await _clipboardService.CopyTextToClipboardAsync(textToCopy);
                    await _dialogService.DisplayToast("Back copied");
                }
                else
                {
                    await _dialogService.DisplayAlert("Text not copied", "Please select at least one example", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert($"Cannot copy Back", $"Error occurred while trying to copy Back: " + ex.Message, "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(CanCopyForms))]
        public async Task CopyFormsAsync()
        {
            string formattedText = await _copySelectedToClipboardService.CompileFormsAsync(Forms);
            await _clipboardService.CopyTextToClipboardAsync(formattedText);
            await _dialogService.DisplayToast("Forms copied");
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyExamplesAsync()
        {
            try
            {
                string textToCopy = await _copySelectedToClipboardService.CompileExamplesAsync(Definitions);

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await _clipboardService.CopyTextToClipboardAsync(textToCopy);
                    await _dialogService.DisplayToast("Examples copied");
                }
                else
                {
                    await _dialogService.DisplayAlert("Text not copied", "Please select at least one example", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert($"Cannot copy Examples", $"Error occurred while trying to copy Examples: " + ex.Message, "OK");
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
