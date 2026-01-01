// Ignore Spelling: app Api

using System.Globalization;
using System.Text.Json;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();

        void SaveSettings(AppSettings appSettings);

        Task<AppSettings?> ImportSettingsAsync(string filePath);

        Task ExportSettingsAsync(string filePath);

        bool GetShowCopyButtons();

        void SetShowCopyButtons(bool value);

        void SetCopyTranslatedMeanings(bool value);

        bool GetUseDarkTheme();

        void SetUseDarkTheme(bool value);

        string GetSelectedParser();

        void SetSelectedParser(string value);

        void AddToHistory(string value);

        IEnumerable<string> LoadHistory();

        void ClearHistory();

        bool GetShowAddNoteWithAnkiConnectButton();
    }

    public class SettingsService : ISettingsService
    {
        private readonly IPreferences _preferences;
        private readonly IFileIOService _fileIOService;
        private readonly IDeviceInfo _deviceInfo;

        public SettingsService(
            IPreferences preferences,
            IFileIOService fileIOService,
            IDeviceInfo deviceInfo)
        {
            _preferences = preferences;
            _fileIOService = fileIOService;
            _deviceInfo = deviceInfo;
        }

        public AppSettings LoadSettings()
        {
            AppSettings appSettings = new AppSettings();
            appSettings.MainWindowWidth = GetDoubleValue(nameof(AppSettings.MainWindowWidth), 800);
            appSettings.MainWindowHeight = GetDoubleValue(nameof(AppSettings.MainWindowHeight), 600);
            appSettings.MainWindowXPos = GetDoubleValue(nameof(AppSettings.MainWindowXPos), 100);
            appSettings.MainWindowYPos = GetDoubleValue(nameof(AppSettings.MainWindowYPos), 100);

            appSettings.AnkiDeckNameDanish = _preferences.Get(nameof(AppSettings.AnkiDeckNameDanish), string.Empty);
            appSettings.AnkiDeckNameSpanish = _preferences.Get(nameof(AppSettings.AnkiDeckNameSpanish), string.Empty);
            appSettings.AnkiModelName = _preferences.Get(nameof(AppSettings.AnkiModelName), string.Empty);
            appSettings.AnkiSoundsFolder = _preferences.Get(nameof(AppSettings.AnkiSoundsFolder), Path.GetTempPath());

            // On mobile by default show the app in a "dictionary" mode, without copy buttons.
            bool showCopyButtonsDefaultValue = _deviceInfo.Platform != DevicePlatform.Android;
            appSettings.ShowCopyButtons = _preferences.Get<bool>(nameof(AppSettings.ShowCopyButtons), showCopyButtonsDefaultValue);

            appSettings.ShowCopyWithAnkiConnectButton = _preferences.Get<bool>(nameof(AppSettings.ShowCopyWithAnkiConnectButton), false);

            appSettings.CopyTranslatedMeanings = _preferences.Get<bool>(nameof(AppSettings.CopyTranslatedMeanings), true);
            appSettings.SelectedParser = _preferences.Get(nameof(AppSettings.SelectedParser), SourceLanguage.Danish.ToString());
            appSettings.UseDarkTheme = _preferences.Get<bool>(nameof(AppSettings.UseDarkTheme), false);

            return appSettings;
        }

        public void SaveSettings(AppSettings appSettings)
        {
            SetDoubleValue(nameof(AppSettings.MainWindowWidth), appSettings.MainWindowWidth);
            SetDoubleValue(nameof(AppSettings.MainWindowHeight), appSettings.MainWindowHeight);
            SetDoubleValue(nameof(AppSettings.MainWindowXPos), appSettings.MainWindowXPos);
            SetDoubleValue(nameof(AppSettings.MainWindowYPos), appSettings.MainWindowYPos);

            _preferences.Set(nameof(AppSettings.AnkiDeckNameDanish), appSettings.AnkiDeckNameDanish);
            _preferences.Set(nameof(AppSettings.AnkiDeckNameSpanish), appSettings.AnkiDeckNameSpanish);
            _preferences.Set(nameof(AppSettings.AnkiModelName), appSettings.AnkiModelName);
            _preferences.Set(nameof(AppSettings.AnkiSoundsFolder), appSettings.AnkiSoundsFolder);
            _preferences.Set(nameof(AppSettings.ShowCopyButtons), appSettings.ShowCopyButtons);
            _preferences.Set(nameof(AppSettings.ShowCopyWithAnkiConnectButton), appSettings.ShowCopyWithAnkiConnectButton);
            _preferences.Set(nameof(AppSettings.CopyTranslatedMeanings), appSettings.CopyTranslatedMeanings);
            _preferences.Set(nameof(AppSettings.SelectedParser), appSettings.SelectedParser);
            _preferences.Set(nameof(AppSettings.UseDarkTheme), appSettings.UseDarkTheme);
        }

        public async Task<AppSettings?> ImportSettingsAsync(string filePath)
        {
            var json = await _fileIOService.ReadAllTextAsync(filePath);
            AppSettings? appSettings = JsonSerializer.Deserialize<AppSettings>(json);

            if (appSettings != null)
            {
                SaveSettings(appSettings);
            }

            return appSettings;
        }

        public async Task ExportSettingsAsync(string filePath)
        {
            AppSettings appSettings = LoadSettings();
            string json = JsonSerializer.Serialize(appSettings);

            await _fileIOService.WriteAllTextAsync(filePath, json);
        }

        public bool GetShowCopyButtons()
        {
            // On mobile by default show the app in a "dictionary" mode, without copy buttons.
            bool showCopyButtonsDefaultValue = _deviceInfo.Platform != DevicePlatform.Android;
            return _preferences.Get<bool>(nameof(AppSettings.ShowCopyButtons), showCopyButtonsDefaultValue);
        }

        public void SetShowCopyButtons(bool value) => _preferences.Set(nameof(AppSettings.ShowCopyButtons), value);

        public bool GetShowAddNoteWithAnkiConnectButton() => _preferences.Get<bool>(nameof(AppSettings.ShowCopyWithAnkiConnectButton), false);

        public void SetCopyTranslatedMeanings(bool value) => _preferences.Set(nameof(AppSettings.CopyTranslatedMeanings), value);

        public bool GetUseDarkTheme() => _preferences.Get<bool>(nameof(AppSettings.UseDarkTheme), false);

        public void SetUseDarkTheme(bool value) => _preferences.Set(nameof(AppSettings.UseDarkTheme), value);

        public string GetSelectedParser() => _preferences.Get(nameof(AppSettings.SelectedParser), SourceLanguage.Danish.ToString());

        public void SetSelectedParser(string value) => _preferences.Set(nameof(AppSettings.SelectedParser), value);

        public void AddToHistory(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            string historyKey = GetHistoryKey();

            // Load existing history from preferences as semicolon-separated string
            string existingHistory = _preferences.Get(historyKey, string.Empty);
            List<string> history = LoadHistory().ToList();

            // Remove the word if it already exists to avoid duplicates and move to front
            history.Remove(word);

            // Add the new word to the beginning of the list
            history.Insert(0, word);

            // If the list contains more than 15 entries, purge the oldest
            if (history.Count > 15)
            {
                history = history.Take(15).ToList();
            }

            // Save back to preferences as semicolon-separated string
            string updatedHistory = string.Join(";", history);
            _preferences.Set(historyKey, updatedHistory);
        }

        public IEnumerable<string> LoadHistory()
        {
            string historyKey = GetHistoryKey();
            string existingHistory = _preferences.Get(historyKey, string.Empty);

            if (string.IsNullOrEmpty(existingHistory))
            {
                return [];
            }

            return existingHistory.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        public void ClearHistory()
        {
            string historyKey = GetHistoryKey();
            _preferences.Remove(historyKey);
        }

        #region Private Methods

        private string GetHistoryKey()
        {
            string dictionary = GetSelectedParser();
            return $"History_{dictionary}";
        }

        private double GetDoubleValue(string settingName, int defaultValue)
        {
            string str = _preferences.Get(settingName, defaultValue.ToString(CultureInfo.CurrentCulture));
            if (double.TryParse(str, out double doubleValue) && doubleValue > 0)
            {
                return doubleValue;
            }

            return defaultValue;
        }

        private void SetDoubleValue(string settingName, double newValue)
        {
            if (newValue > 0)
            {
                _preferences.Set(settingName, newValue.ToString(CultureInfo.CurrentCulture));
            }
        }

        #endregion
    }
}
