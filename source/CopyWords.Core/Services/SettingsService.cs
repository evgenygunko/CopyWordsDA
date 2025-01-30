// Ignore Spelling: app

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();

        void SaveSettings(AppSettings appSettings);

        Task<AppSettings> ImportSettingsAsync(string filePath);

        Task ExportSettingsAsync(string filePath);
    }

    public class SettingsService : ISettingsService
    {
        private readonly IPreferences _preferences;
        private readonly IFileIOService _fileIOService;

        public SettingsService(
            IPreferences preferences,
            IFileIOService fileIOService)
        {
            _preferences = preferences;
            _fileIOService = fileIOService;
        }

        public AppSettings LoadSettings()
        {
            AppSettings appSettings = new AppSettings();
            appSettings.MainWindowWidth = GetDoubleValue("MainWindowWidth", 800);
            appSettings.MainWindowHeight = GetDoubleValue("MainWindowHeight", 600);
            appSettings.MainWindowXPos = GetDoubleValue("MainWindowXPos", 100);
            appSettings.MainWindowYPos = GetDoubleValue("MainWindowYPos", 100);

            appSettings.AnkiSoundsFolder = _preferences.Get("AnkiSoundsFolder", Path.GetTempPath());
            appSettings.FfmpegBinFolder = GetFfmpegBinFolder();
            appSettings.Mp3gainPath = _preferences.Get("Mp3gainPath", string.Empty);
            appSettings.UseMp3gain = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && _preferences.Get<bool>("UseMp3gain", false);
            appSettings.UseTranslator = _preferences.Get<bool>("UseTranslator", false);
            appSettings.TranslatorApiUrl = _preferences.Get("TranslatorApiUrl", string.Empty);
            appSettings.TranslateMeanings = _preferences.Get<bool>("TranslateMeanings", true);
            appSettings.SelectedParser = _preferences.Get("SelectedParser", string.Empty);

            return appSettings;
        }

        public void SaveSettings(AppSettings appSettings)
        {
            SetDoubleValue("MainWindowWidth", appSettings.MainWindowWidth);
            SetDoubleValue("MainWindowHeight", appSettings.MainWindowHeight);
            SetDoubleValue("MainWindowXPos", appSettings.MainWindowXPos);
            SetDoubleValue("MainWindowYPos", appSettings.MainWindowYPos);

            _preferences.Set("AnkiSoundsFolder", appSettings.AnkiSoundsFolder);
            _preferences.Set("FfmpegBinFolder", appSettings.FfmpegBinFolder);
            _preferences.Set("Mp3gainPath", appSettings.Mp3gainPath);
            _preferences.Set("UseMp3gain", appSettings.UseMp3gain);
            _preferences.Set("UseTranslator", appSettings.UseTranslator);
            _preferences.Set("TranslatorApiUrl", appSettings.TranslatorApiUrl);
            _preferences.Set("TranslateMeanings", appSettings.TranslateMeanings);
            _preferences.Set("SelectedParser", appSettings.SelectedParser);
        }

        public async Task<AppSettings> ImportSettingsAsync(string filePath)
        {
            var json = await _fileIOService.ReadAllTextAsync(filePath);
            AppSettings appSettings = JsonSerializer.Deserialize<AppSettings>(json);

            SaveSettings(appSettings);

            return appSettings;
        }

        public async Task ExportSettingsAsync(string filePath)
        {
            AppSettings appSettings = LoadSettings();
            string json = JsonSerializer.Serialize(appSettings);

            await _fileIOService.WriteAllTextAsync(filePath, json);
        }

        #region Private Methods

        private string GetFfmpegBinFolder()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
            {
                return _preferences.Get("FfmpegBinFolder", "/usr/local/bin/");
            }

            string wingetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Microsoft", "WinGet", "Links");
            return _preferences.Get("FfmpegBinFolder", wingetPath);
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
