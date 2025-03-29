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

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            IDialogService dialogService)
        {
            _saveSoundFileService = saveSoundFileService;
            _dialogService = dialogService;
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

        public Color PlaySoundButtonColor => GetButtonColor(CanPlaySound);

        public Color SaveSoundButtonColor => GetButtonColor(CanSaveSoundFile);

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
