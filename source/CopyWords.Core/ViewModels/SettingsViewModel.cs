// Ignore Spelling: Ffmpeg Validator

#nullable enable
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentValidation;

namespace CopyWords.Core.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IShellService _shellService;
        private readonly IFileIOService _fileIOService;
        private readonly IFolderPicker _folderPicker;
        private readonly IFilePicker _filePicker;
        private readonly IValidator<SettingsViewModel> _settingsViewModelValidator;

        public SettingsViewModel(
            ISettingsService settingsService,
            IDialogService dialogService,
            IShellService shellService,
            IFileIOService fileIOService,
            IFolderPicker folderPicker,
            IFilePicker filePicker,
            IValidator<SettingsViewModel> settingsViewModelValidator)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _shellService = shellService;
            _fileIOService = fileIOService;
            _folderPicker = folderPicker;
            _filePicker = filePicker;
            _settingsViewModelValidator = settingsViewModelValidator;

            AppSettings appSettings = _settingsService.LoadSettings();
            UpdateUI(appSettings);
        }

        #region Properties

        [ObservableProperty]
        private string? ankiSoundsFolder;

        [ObservableProperty]
        private string? ffmpegBinFolder;

        [ObservableProperty]
        private bool useMp3gain;

        public bool CanUseMp3gain => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool CanUseFfmpeg => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        [ObservableProperty]
        private string? mp3gainPath;

        [ObservableProperty]
        private string? translatorApiUrl;

        [ObservableProperty]
        private bool useTranslator;

        [ObservableProperty]
        private bool translateHeadword;

        [ObservableProperty]
        private bool translateMeanings;

        [ObservableProperty]
        private string validationErrors;

        public string About => $"App version: {AppInfo.VersionString} (Build {AppInfo.BuildString}), {RuntimeInformation.FrameworkDescription}";

        #endregion

        #region Commands

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [SupportedOSPlatform("android")]
        [RelayCommand]
        public async Task ExportSettingsAsync(CancellationToken cancellationToken)
        {
            var result = await _folderPicker.PickAsync(cancellationToken);
            if (result.IsSuccessful)
            {
                string settingFileFolder = result.Folder.Path;
                string settingFile = Path.Combine(settingFileFolder, "CopyWords_Settings.json");

                if (_fileIOService.FileExists(settingFile))
                {
                    bool answer = await _dialogService.DisplayAlert("File already exists", $"File '{Path.GetFileName(settingFile)}' already exists. Overwrite?", "Yes", "No");
                    if (!answer)
                    {
                        return;
                    }
                }

                await _settingsService.ExportSettingsAsync(settingFile);

                await _dialogService.DisplayToast($"Settings exported to '{settingFile}'.");
                await _shellService.GoToAsync("..");
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
                    AppSettings appSettings = await _settingsService.ImportSettingsAsync(settingFile);
                    UpdateUI(appSettings);

                    await _dialogService.DisplayToast("Settings imported.");
                    await _shellService.GoToAsync("..");
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

        [RelayCommand]
        public async Task SaveSettingsAsync()
        {
            // Apply the validation and get the result
            var validation = await _settingsViewModelValidator.ValidateAsync(this);
            if (validation != null && !validation.IsValid)
            {
                // The validation contains error, we stop the process
                ValidationErrors = string.Join(Environment.NewLine, validation.Errors.Select(x => x.ErrorMessage));
                return;
            }
            else
            {
                ValidationErrors = string.Empty;
            }

            AppSettings appSettings = _settingsService.LoadSettings();

            appSettings.AnkiSoundsFolder = AnkiSoundsFolder;
            appSettings.FfmpegBinFolder = FfmpegBinFolder;
            appSettings.UseMp3gain = UseMp3gain;
            appSettings.Mp3gainPath = Mp3gainPath;
            appSettings.UseTranslator = UseTranslator;
            appSettings.TranslatorApiUrl = TranslatorApiUrl;
            appSettings.TranslateMeanings = TranslateMeanings;
            appSettings.TranslateHeadword = TranslateHeadword;

            _settingsService.SaveSettings(appSettings);

            await _dialogService.DisplayToast("Settings have been updated");

            await _shellService.GoToAsync("..");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await _shellService.GoToAsync("..");
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

                options = new()
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

            UseTranslator = appSettings.UseTranslator;
            TranslatorApiUrl = appSettings.TranslatorApiUrl;
            TranslateMeanings = appSettings.TranslateMeanings;
            TranslateHeadword = appSettings.TranslateHeadword;
        }

        #endregion
    }
}
