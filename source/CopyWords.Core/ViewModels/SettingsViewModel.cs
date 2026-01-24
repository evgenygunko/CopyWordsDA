// Ignore Spelling: Validator Api

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
        private readonly IDeviceInfo _deviceInfo;
        private readonly IValidator<SettingsViewModel> _settingsViewModelValidator;
        private readonly IAnkiConnectService _ankiConnectService;
        private readonly IAnkiDroidService _ankiDroidService;
        private readonly IAppThemeService _appThemeService;

        private bool _isInitialized;

        public SettingsViewModel(
            ISettingsService settingsService,
            IDialogService dialogService,
            IShellService shellService,
            IDeviceInfo deviceInfo,
            IValidator<SettingsViewModel> settingsViewModelValidator,
            IAnkiConnectService ankiConnectService,
            IAnkiDroidService ankiDroidService,
            IAppThemeService appThemeService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _shellService = shellService;
            _deviceInfo = deviceInfo;
            _settingsViewModelValidator = settingsViewModelValidator;
            _ankiConnectService = ankiConnectService;
            _ankiDroidService = ankiDroidService;
            _appThemeService = appThemeService;

            // Subscribe to theme changes
            _appThemeService.ThemeChanged += (s, e) => OnPropertyChanged(nameof(ButtonIconColor));
        }

        #region Properties

        internal bool CanUpdateIndividualSettings => _deviceInfo.Platform == DevicePlatform.Android;

        [ObservableProperty]
        public partial ObservableCollection<string> DeckNames { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<string> ModelNames { get; set; } = [];

        [ObservableProperty]
        public partial bool IsAnkiIntegrationAvailable { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectAnkiDeckNameDanishCommand))]
        public partial string? AnkiDeckNameDanish { get; set; }

        partial void OnAnkiDeckNameDanishChanged(string? value)
        {
            OnAnkiDeckNameDanishChangedInternal(value);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectAnkiDeckNameSpanishCommand))]
        public partial string? AnkiDeckNameSpanish { get; set; }

        partial void OnAnkiDeckNameSpanishChanged(string? value)
        {
            OnAnkiDeckNameSpanishChangedInternal(value);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectAnkiModelNameCommand))]
        public partial string? AnkiModelName { get; set; }

        partial void OnAnkiModelNameChanged(string? value)
        {
            OnAnkiModelNameChangedInternal(value);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial string? AnkiSoundsFolder { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        public partial bool ShowAnkiButton { get; set; }

        partial void OnShowAnkiButtonChanged(bool value)
        {
            OnShowAnkiButtonInternal(value);
        }

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

            appSettings.AnkiDeckNameDanish = AnkiDeckNameDanish ?? string.Empty;
            appSettings.AnkiDeckNameSpanish = AnkiDeckNameSpanish ?? string.Empty;
            appSettings.AnkiModelName = AnkiModelName ?? string.Empty;
            appSettings.AnkiSoundsFolder = AnkiSoundsFolder ?? string.Empty;
            appSettings.ShowCopyButtons = ShowCopyButtons;
            appSettings.ShowAnkiButton = ShowAnkiButton;
            appSettings.CopyTranslatedMeanings = CopyTranslatedMeanings;
            appSettings.UseDarkTheme = UseDarkTheme;

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
            // Load deck and model names for Android Picker controls
            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                await InitializeAnkiDroidAsync();
            }

            AppSettings appSettings = _settingsService.LoadSettings();
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
            ShowAnkiButton = appSettings.ShowAnkiButton;
            CopyTranslatedMeanings = appSettings.CopyTranslatedMeanings;
            UseDarkTheme = appSettings.UseDarkTheme;

            _isInitialized = true;
        }

        #endregion

        #region Internal Methods

        internal async Task InitializeAnkiDroidAsync()
        {
            try
            {
                IsAnkiIntegrationAvailable = _ankiDroidService.IsAvailable();

                if (IsAnkiIntegrationAvailable)
                {
                    // Check and request permission to access AnkiDroid
                    if (!_ankiDroidService.HasPermission())
                    {
                        await _ankiDroidService.RequestPermissionAsync();
                    }

                    // Only proceed if permission was granted
                    if (!_ankiDroidService.HasPermission())
                    {
                        Debug.WriteLine("AnkiDroid permission was not granted by the user.");
                        return;
                    }

                    IEnumerable<string> deckNames = _ankiDroidService.GetDeckNames();
                    DeckNames = new ObservableCollection<string>(deckNames);

                    IEnumerable<string> modelNames = _ankiDroidService.GetModelNames();
                    ModelNames = new ObservableCollection<string>(modelNames);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading deck/model names: {ex.Message}");
            }
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

        internal void OnShowAnkiButtonInternal(bool value)
        {
            if (_isInitialized && CanUpdateIndividualSettings)
            {
                _settingsService.SetShowAnkiButton(value);
                Debug.WriteLine($"ShowAnkiButton has changed to {value}");
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

        internal void OnAnkiDeckNameDanishChangedInternal(string? value)
        {
            if (_isInitialized && CanUpdateIndividualSettings && !string.IsNullOrEmpty(value))
            {
                _settingsService.SetAnkiDeckNameDanish(value);
                Debug.WriteLine($"AnkiDeckNameDanish has changed to {value}");
            }
        }

        internal void OnAnkiDeckNameSpanishChangedInternal(string? value)
        {
            if (_isInitialized && CanUpdateIndividualSettings && !string.IsNullOrEmpty(value))
            {
                _settingsService.SetAnkiDeckNameSpanish(value);
                Debug.WriteLine($"AnkiDeckNameSpanish has changed to {value}");
            }
        }

        internal void OnAnkiModelNameChangedInternal(string? value)
        {
            if (_isInitialized && CanUpdateIndividualSettings && !string.IsNullOrEmpty(value))
            {
                _settingsService.SetAnkiModelName(value);
                Debug.WriteLine($"AnkiModelName has changed to {value}");
            }
        }

        #endregion
    }
}
