using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;

        public SettingsViewModel(
            ISettingsService settingsService,
            IDialogService dialogService)
        {
            _settingsService = settingsService;

            AnkiSoundsFolder = _settingsService.GetAnkiSoundsFolder();
            UseMp3gain = _settingsService.UseMp3gain;
            Mp3gainPath = _settingsService.GetMp3gainPath();
            _dialogService = dialogService;
        }

        #region Properties

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string ankiSoundsFolder;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool useMp3gain;

#pragma warning disable CA1822
        public bool CanUseMp3gain => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#pragma warning restore CA1822

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string mp3gainPath;

        public string About => $".net version: {RuntimeInformation.FrameworkDescription}";

        public bool CanSaveSettings
        {
            get
            {
                bool result = Directory.Exists(AnkiSoundsFolder);
                if (UseMp3gain)
                {
                    result &= File.Exists(Mp3gainPath);
                }

                return result;
            }
        }

        #endregion

        #region Commands

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [RelayCommand]
        public async Task PickAnkiSoundsFolderAsync(CancellationToken cancellationToken)
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            if (result.IsSuccessful)
            {
                AnkiSoundsFolder = result.Folder.Path;
            }
        }

        [RelayCommand]
        public async Task PickMp3gainPathAsync()
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".exe" } }, // file extension
                });

            PickOptions options = new()
            {
                PickerTitle = "Please select path to mp3gain.exe",
                FileTypes = customFileType,
            };

            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    Mp3gainPath = result.FullPath;
                }
            }
            catch (Exception)
            {
                // The user canceled or something went wrong
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveSettings))]
        public async Task SaveSettingsAsync()
        {
            _settingsService.SetAnkiSoundsFolder(AnkiSoundsFolder);
            _settingsService.UseMp3gain = UseMp3gain;
            _settingsService.SetMp3gainPath(Mp3gainPath);

            await _dialogService.DisplayToast("Settings have been updated");

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        #endregion
    }
}
