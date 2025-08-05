// Ignore Spelling: Ffmpeg Validator Api

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentValidation;
using FluentValidation.Results;

namespace CopyWords.Core.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IShellService _shellService;
        private readonly IFilePicker _filePicker;
        private readonly IDeviceInfo _deviceInfo;
        private readonly IFileSaver _fileSaver;
        private readonly IValidator<SettingsViewModel> _settingsViewModelValidator;

        private bool _isInitialized;

        public SettingsViewModel(
            ISettingsService settingsService,
            IDialogService dialogService,
            IShellService shellService,
            IFilePicker filePicker,
            IDeviceInfo deviceInfo,
            IFileSaver fileSaver,
            IValidator<SettingsViewModel> settingsViewModelValidator)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _shellService = shellService;
            _filePicker = filePicker;
            _deviceInfo = deviceInfo;
            _fileSaver = fileSaver;
            _settingsViewModelValidator = settingsViewModelValidator;
        }

        #region Properties

        internal bool CanUpdateIndividualSettings => _deviceInfo.Platform == DevicePlatform.Android;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial string? AnkiSoundsFolder { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial string? FfmpegBinFolder { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial bool UseMp3gain { get; set; }

        public bool CanUseMp3gain => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool CanUseFfmpeg => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial string? Mp3gainPath { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial bool ShowCopyButtons { get; set; }

        partial void OnShowCopyButtonsChanged(bool value)
        {
            OnShowCopyButtonsChangedInternal(value);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial bool CopyTranslatedMeanings { get; set; }

        partial void OnCopyTranslatedMeaningsChanged(bool value)
        {
            OnCopyTranslatedMeaningsChangedInternal(value);
        }

        [ObservableProperty]
        public partial ValidationResult? ValidationResult { get; set; }

        public string About => $"App version: {AppInfo.VersionString} (Build {AppInfo.BuildString}), {RuntimeInformation.FrameworkDescription}";

        #endregion

        #region Commands

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [SupportedOSPlatform("android")]
        [RelayCommand]
        public async Task ExportSettingsAsync(CancellationToken cancellationToken)
        {
            // Load all settings but update with values that the user changed
            AppSettings appSettings = _settingsService.LoadSettings();
            UpdateAppSettingsWithCurrentValues(appSettings);

            string json = JsonSerializer.Serialize(appSettings);
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            FileSaverResult fileSaveResult = await _fileSaver.SaveAsync("CopyWords_Settings.json", memoryStream, cancellationToken);
            if (fileSaveResult.IsSuccessful)
            {
                await _dialogService.DisplayToast($"Settings successfully exported to '{fileSaveResult.FilePath}'.");
            }
        }

        [RelayCommand]
        public async Task ImportSettingsAsync()
        {
            string settingFile = await PickSettingsFilePathAsync();

            if (!string.IsNullOrEmpty(settingFile))
            {
                try
                {
                    AppSettings? appSettings = await _settingsService.ImportSettingsAsync(settingFile);
                    if (appSettings == null)
                    {
                        await _dialogService.DisplayAlert("Cannot import setting", $"Cannot import settings from the file '{settingFile}'. The format is incorrect", "OK");
                        return;
                    }

                    UpdateUI(appSettings);

                    await _dialogService.DisplayToast("Settings successfully imported.");
                }
                catch (Exception ex)
                {
                    await _dialogService.DisplayAlert("Cannot import setting", $"Cannot import settings from the file '{settingFile}'. Error: {ex}", "OK");
                }
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [SupportedOSPlatform("android")]
        [RelayCommand]
        public async Task PickAnkiSoundsFolderAsync(CancellationToken cancellationToken)
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            if (result.IsSuccessful)
            {
                AnkiSoundsFolder = result.Folder.Path;
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [SupportedOSPlatform("android")]
        [RelayCommand]
        public async Task PickFfmpegBinFolderAsync(CancellationToken cancellationToken)
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            if (result.IsSuccessful)
            {
                FfmpegBinFolder = result.Folder.Path;
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
            AppSettings appSettings = _settingsService.LoadSettings();
            UpdateAppSettingsWithCurrentValues(appSettings);

            _settingsService.SaveSettings(appSettings);

            await _dialogService.DisplayToast("Settings have been updated");

            await _shellService.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await _shellService.GoToAsync("..");
        }

        [RelayCommand]
        public void Init()
        {
            AppSettings appSettings = _settingsService.LoadSettings();
            UpdateUI(appSettings);

            _isInitialized = true;
        }

        #endregion

        #region Internal Methods

        internal bool CanSaveSettings()
        {
            // Apply the validation and get the result
            ValidationResult = _settingsViewModelValidator.ValidateAsync(this).GetAwaiter().GetResult();

            if (SynchronizationContext.Current == null && TaskScheduler.Current == TaskScheduler.Default)
            {
                ValidationResult = _settingsViewModelValidator.ValidateAsync(this).GetAwaiter().GetResult();
            }
            else
            {
                ValidationResult = Task.Run(() => _settingsViewModelValidator.ValidateAsync(this)).GetAwaiter().GetResult();
            }

            if (ValidationResult != null && !ValidationResult.IsValid)
            {
                // The validation contains errors
                return false;
            }

            return true;
        }

        internal void OnCopyTranslatedMeaningsChangedInternal(bool value)
        {
            if (_isInitialized && CanUpdateIndividualSettings)
            {
                _settingsService.SetCopyTranslatedMeanings(value);
                Debug.WriteLine($"CopyTranslatedMeanings has changed to {value}");
            }
        }

        internal void OnShowCopyButtonsChangedInternal(bool value)
        {
            if (_isInitialized && CanUpdateIndividualSettings)
            {
                _settingsService.SetShowCopyButtons(value);
                Debug.WriteLine($"ShowCopyButtons has changed to {value}");
            }
        }

        #endregion

        #region Private Methods

        private async Task<string> PickSettingsFilePathAsync()
        {
            string settingsFilePath = "";

            PickOptions? options = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Attempting to use a filter on MacOS causes the FilePicker to throw an exception.
                // There’s a related issue on GitHub (https://github.com/dotnet/maui/issues/9394#issuecomment-1285814762)
                // indicating it should work, but an exception still occurs.
                // For now, filtering is enabled only for Windows.
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".json" } }, // file extension
                        { DevicePlatform.MacCatalyst, new[] { "json" } }, // UTType values
                    });

                options = new PickOptions()
                {
                    PickerTitle = "Please select path to settings file",
                    FileTypes = customFileType,
                };
            }

            try
            {
                var result = await _filePicker.PickAsync(options);
                if (result != null)
                {
                    settingsFilePath = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                _ = ex;
                // The user canceled or something went wrong
            }

            return settingsFilePath;
        }

        private void UpdateUI(AppSettings appSettings)
        {
            AnkiSoundsFolder = appSettings.AnkiSoundsFolder;
            FfmpegBinFolder = appSettings.FfmpegBinFolder;

            UseMp3gain = appSettings.UseMp3gain;
            Mp3gainPath = appSettings.Mp3gainPath;

            ShowCopyButtons = appSettings.ShowCopyButtons;
            CopyTranslatedMeanings = appSettings.CopyTranslatedMeanings;
        }

        private void UpdateAppSettingsWithCurrentValues(AppSettings appSettings)
        {
            appSettings.AnkiSoundsFolder = AnkiSoundsFolder ?? string.Empty;
            appSettings.FfmpegBinFolder = FfmpegBinFolder ?? string.Empty;
            appSettings.Mp3gainPath = Mp3gainPath ?? string.Empty;
            appSettings.UseMp3gain = UseMp3gain;
            appSettings.ShowCopyButtons = ShowCopyButtons;
            appSettings.CopyTranslatedMeanings = CopyTranslatedMeanings;
        }

        #endregion
    }
}
