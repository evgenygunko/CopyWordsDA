// Ignore Spelling: Validator Api

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Constants;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
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
        private readonly IAnkiConnectService _ankiConnectService;
        private readonly IAppThemeService _appThemeService;

        private bool _isInitialized;

        public SettingsViewModel(
            ISettingsService settingsService,
            IDialogService dialogService,
            IShellService shellService,
            IFilePicker filePicker,
            IDeviceInfo deviceInfo,
            IFileSaver fileSaver,
            IValidator<SettingsViewModel> settingsViewModelValidator,
            IAnkiConnectService ankiConnectService,
            IAppThemeService appThemeService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _shellService = shellService;
            _filePicker = filePicker;
            _deviceInfo = deviceInfo;
            _fileSaver = fileSaver;
            _settingsViewModelValidator = settingsViewModelValidator;
            _ankiConnectService = ankiConnectService;
            _appThemeService = appThemeService;

            // Subscribe to theme changes
            _appThemeService.ThemeChanged += (s, e) => OnPropertyChanged(nameof(ButtonIconColor));
        }

        #region Properties

        internal bool CanUpdateIndividualSettings => _deviceInfo.Platform == DevicePlatform.Android;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectAnkiDeckNameDanishCommand))]
        public partial string? AnkiDeckNameDanish { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectAnkiDeckNameSpanishCommand))]
        public partial string? AnkiDeckNameSpanish { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectAnkiModelNameCommand))]
        public partial string? AnkiModelName { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial string? AnkiSoundsFolder { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial bool ShowCopyWithAnkiConnectButton { get; set; }

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
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial bool UseDarkTheme { get; set; }

        partial void OnUseDarkThemeChanged(bool value)
        {
            OnUseDarkThemeChangedInternal(value);
        }

        [ObservableProperty]
        public partial ValidationResult? ValidationResult { get; set; }

        public string About => $"App version: {AppInfo.VersionString} (Build {AppInfo.BuildString}), {RuntimeInformation.FrameworkDescription}";

        public Color ButtonIconColor => ThemeColors.GetButtonForegroundColor(_appThemeService.CurrentTheme);

        #endregion

        #region Commands

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
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
        public async Task ImportSettingsAsync(CancellationToken cancellationToken)
        {
            string settingFile = await PickSettingsFilePathAsync();

            if (!string.IsNullOrEmpty(settingFile))
            {
                try
                {
                    AppSettings? appSettings = await _settingsService.ImportSettingsAsync(settingFile);
                    if (appSettings == null)
                    {
                        await _dialogService.DisplayAlertAsync("Cannot import setting", $"Cannot import settings from the file '{settingFile}'. The format is incorrect", "OK");
                        return;
                    }

                    await UpdateUIAsync(appSettings, cancellationToken);

                    await _dialogService.DisplayToast("Settings successfully imported.");
                }
                catch (Exception ex)
                {
                    await _dialogService.DisplayAlertAsync("Cannot import setting", $"Cannot import settings from the file '{settingFile}'. Error: {ex}", "OK");
                }
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [RelayCommand]
        public async Task SelectAnkiDeckNameDanishAsync(CancellationToken cancellationToken)
        {
            string? selectedDeck = await SelectAnkiDeckNameAsync(cancellationToken);
            if (selectedDeck != null)
            {
                AnkiDeckNameDanish = selectedDeck;
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [RelayCommand]
        public async Task SelectAnkiDeckNameSpanishAsync(CancellationToken cancellationToken)
        {
            string? selectedDeck = await SelectAnkiDeckNameAsync(cancellationToken);
            if (selectedDeck != null)
            {
                AnkiDeckNameSpanish = selectedDeck;
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        private async Task<string?> SelectAnkiDeckNameAsync(CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<string> deckNames = await _ankiConnectService.GetDeckNamesAsync(cancellationToken);

                if (deckNames.Any())
                {
                    string result = await _dialogService.DisplayActionSheetAsync(title: "Select deck:", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, deckNames.ToArray());
                    if (!string.IsNullOrEmpty(result) && result != "Cancel")
                    {
                        return result;
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Fetching deck names", "Cannot get deck names from AnkiConnect.", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot select deck", "Error occurred while trying to select deck name: " + ex.Message, "OK");
            }

            return null;
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [RelayCommand]
        public async Task SelectAnkiModelNameAsync(CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<string> modelNames = await _ankiConnectService.GetModelNamesAsync(cancellationToken);

                if (modelNames.Any())
                {
                    string result = await _dialogService.DisplayActionSheetAsync(title: "Select model:", cancel: "Cancel", destruction: null!, flowDirection: FlowDirection.LeftToRight, modelNames.ToArray());
                    if (!string.IsNullOrEmpty(result) && result != "Cancel")
                    {
                        AnkiModelName = result;
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Fetching model names", "Cannot get model names from AnkiConnect.", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot select model", "Error occurred while trying to select model name: " + ex.Message, "OK");
            }
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
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
            // Restore theme if a user changed it but canceled the settings
            AppTheme theme = _settingsService.GetUseDarkTheme() ? AppTheme.Dark : AppTheme.Light;
            _appThemeService.ApplyTheme(theme);

            await _shellService.GoToAsync("..");
        }

        [RelayCommand]
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            AppSettings appSettings = _settingsService.LoadSettings();
            await UpdateUIAsync(appSettings, cancellationToken);

            _isInitialized = true;
        }

        #endregion

        #region Internal Methods

        internal async Task UpdateUIAsync(AppSettings appSettings, CancellationToken cancellationToken)
        {
            AnkiDeckNameDanish = appSettings.AnkiDeckNameDanish;
            AnkiDeckNameSpanish = appSettings.AnkiDeckNameSpanish;
            AnkiModelName = appSettings.AnkiModelName;

            if (string.IsNullOrEmpty(appSettings.AnkiSoundsFolder) && (_deviceInfo.Platform == DevicePlatform.WinUI || _deviceInfo.Platform == DevicePlatform.MacCatalyst))
            {
                AnkiSoundsFolder = await _ankiConnectService.GetAnkiMediaDirectoryPathAsync(cancellationToken);
            }
            else
            {
                AnkiSoundsFolder = appSettings.AnkiSoundsFolder;
            }

            ShowCopyButtons = appSettings.ShowCopyButtons;
            ShowCopyWithAnkiConnectButton = appSettings.ShowCopyWithAnkiConnectButton;
            CopyTranslatedMeanings = appSettings.CopyTranslatedMeanings;
            UseDarkTheme = appSettings.UseDarkTheme;
        }

        internal bool CanSaveSettings()
        {
            // Apply the validation and get the result
            if (SynchronizationContext.Current == null && TaskScheduler.Current == TaskScheduler.Default)
            {
                ValidationResult = _settingsViewModelValidator.ValidateAsync(this).GetAwaiter().GetResult();
            }
            else
            {
                ValidationResult = Task.Run(() => _settingsViewModelValidator.ValidateAsync(this)).GetAwaiter().GetResult();
            }

            if (ValidationResult?.IsValid == false)
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

        internal void OnUseDarkThemeChangedInternal(bool value)
        {
            if (_isInitialized)
            {
                AppTheme theme = value ? AppTheme.Dark : AppTheme.Light;
                _appThemeService.ApplyTheme(theme);

                if (CanUpdateIndividualSettings)
                {
                    _settingsService.SetUseDarkTheme(value);
                }

                Debug.WriteLine($"UseDarkTheme has changed to {value}");
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

        private void UpdateAppSettingsWithCurrentValues(AppSettings appSettings)
        {
            appSettings.AnkiDeckNameDanish = AnkiDeckNameDanish ?? string.Empty;
            appSettings.AnkiDeckNameSpanish = AnkiDeckNameSpanish ?? string.Empty;
            appSettings.AnkiModelName = AnkiModelName ?? string.Empty;
            appSettings.AnkiSoundsFolder = AnkiSoundsFolder ?? string.Empty;
            appSettings.ShowCopyButtons = ShowCopyButtons;
            appSettings.ShowCopyWithAnkiConnectButton = ShowCopyWithAnkiConnectButton;
            appSettings.CopyTranslatedMeanings = CopyTranslatedMeanings;
            appSettings.UseDarkTheme = UseDarkTheme;
        }

        #endregion
    }
}
