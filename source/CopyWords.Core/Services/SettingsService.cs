using System.Globalization;
using System.Runtime.InteropServices;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();

        void SaveSettings(AppSettings appSettings);
    }

    public class SettingsService : ISettingsService
    {
        public AppSettings LoadSettings()
        {
            AppSettings appSettings = new AppSettings();
            appSettings.MainWindowWidth = GetDoubleValue("MainWindowWidth", 800);
            appSettings.MainWindowHeight = GetDoubleValue("MainWindowHeight", 600);
            appSettings.MainWindowXPos = GetDoubleValue("MainWindowXPos", 100);
            appSettings.MainWindowYPos = GetDoubleValue("MainWindowYPos", 100);

            appSettings.AnkiSoundsFolder = Preferences.Default.Get("AnkiSoundsFolder", Path.GetTempPath());
            appSettings.FfmpegBinFolder = GetFfmpegBinFolder();
            appSettings.Mp3gainPath = Preferences.Default.Get("Mp3gainPath", string.Empty);
            appSettings.UseMp3gain = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Preferences.Default.Get<bool>("UseMp3gain", false);
            appSettings.UseTranslator = Preferences.Default.Get<bool>("UseTranslator", false);
            appSettings.TranslatorApiUrl = Preferences.Default.Get("TranslatorApiUrl", string.Empty);
            appSettings.SelectedParser = Preferences.Default.Get("SelectedParser", string.Empty);

            return appSettings;
        }

        public void SaveSettings(AppSettings appSettings)
        {
            SetDoubleValue("MainWindowWidth", appSettings.MainWindowWidth);
            SetDoubleValue("MainWindowHeight", appSettings.MainWindowHeight);
            SetDoubleValue("MainWindowXPos", appSettings.MainWindowXPos);
            SetDoubleValue("MainWindowYPos", appSettings.MainWindowYPos);

            Preferences.Default.Set("AnkiSoundsFolder", appSettings.AnkiSoundsFolder);
            Preferences.Default.Set("FfmpegBinFolder", appSettings.FfmpegBinFolder);
            Preferences.Default.Set("Mp3gainPath", appSettings.Mp3gainPath);
            Preferences.Default.Set("UseMp3gain", appSettings.UseMp3gain);
            Preferences.Default.Set("UseTranslator", appSettings.UseTranslator);
            Preferences.Default.Set("TranslatorApiUrl", appSettings.TranslatorApiUrl);
            Preferences.Default.Set("SelectedParser", appSettings.SelectedParser);
        }

        #region Private Methods

        private string GetFfmpegBinFolder()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst)
            {
                return Preferences.Default.Get("FfmpegBinFolder", "/usr/local/bin/");
            }

            string wingetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Microsoft", "WinGet", "Links");
            return Preferences.Default.Get("FfmpegBinFolder", wingetPath);
        }

        private static double GetDoubleValue(string settingName, int defaultValue)
        {
            string str = Preferences.Default.Get(settingName, defaultValue.ToString(CultureInfo.CurrentCulture));
            if (double.TryParse(str, out double doubleValue) && doubleValue > 0)
            {
                return doubleValue;
            }

            return defaultValue;
        }

        private static void SetDoubleValue(string settingName, double newValue)
        {
            if (newValue > 0)
            {
                Preferences.Default.Set(settingName, newValue.ToString(CultureInfo.CurrentCulture));
            }
        }

        #endregion
    }
}
