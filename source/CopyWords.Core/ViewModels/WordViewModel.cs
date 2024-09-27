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
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            IDialogService dialogService,
            IClipboardService clipboardService)
        {
            _saveSoundFileService = saveSoundFileService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
        }

        #region Properties

        public ObservableCollection<VariantViewModel> Variants { get; } = new();

        public ObservableCollection<DefinitionViewModel> DefinitionViewModels { get; } = new();

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
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private bool isBusy;

        public bool CanSaveSoundFile => !IsBusy && !string.IsNullOrEmpty(SoundFileName);

        public bool CanPlaySound => !IsBusy && !string.IsNullOrEmpty(SoundUrl);

        public Color PlaySoundButtonColor => GetButtonColor(CanPlaySound);

        public Color SaveSoundButtonColor => GetButtonColor(CanSaveSoundFile);

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
