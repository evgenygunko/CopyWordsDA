﻿// Ignore Spelling: Ffmpeg Validator

#nullable enable
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
        }

        #region Properties

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string? ankiSoundsFolder;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string? ffmpegBinFolder;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool useMp3gain;

        public bool CanUseMp3gain => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool CanUseFfmpeg => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string? mp3gainPath;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string? translatorApiUrl;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool useTranslator;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool translateHeadword;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool translateMeanings;

        [ObservableProperty]
        private ValidationResult? validationResult;

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

        [RelayCommand]
        public void Init()
        {
            AppSettings appSettings = _settingsService.LoadSettings();
            UpdateUI(appSettings);
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
