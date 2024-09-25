using System.Globalization;
using System.Runtime.InteropServices;

namespace CopyWords.Core.Services
{
    public interface ISettingsService
    {
        double MainWindowWidth { get; set; }

        double MainWindowHeight { get; set; }

        double MainWindowXPos { get; set; }

        double MainWindowYPos { get; set; }

        string GetAnkiSoundsFolder();

        void SetAnkiSoundsFolder(string path);

        string GetMp3gainPath();

        void SetMp3gainPath(string path);

        bool UseMp3gain { get; set; }

        void SetTranslatorApiUrl(string url);

        string GetTranslatorApiUrl();

        bool UseTranslator { get; set; }

        string SelectedParser { get; set; }
    }

    public class SettingsService : ISettingsService
    {
        public double MainWindowWidth
        {
            get => GetDoubleValue(nameof(MainWindowWidth), 800);
            set => SetDoubleValue(nameof(MainWindowWidth), value);
        }

        public double MainWindowHeight
        {
            get => GetDoubleValue(nameof(MainWindowHeight), 600);
            set => SetDoubleValue(nameof(MainWindowHeight), value);
        }

        public double MainWindowXPos
        {
            get => GetDoubleValue(nameof(MainWindowXPos), 100);
            set => SetDoubleValue(nameof(MainWindowXPos), value);
        }

        public double MainWindowYPos
        {
            get => GetDoubleValue(nameof(MainWindowYPos), 100);
            set => SetDoubleValue(nameof(MainWindowYPos), value);
        }

        public string GetAnkiSoundsFolder() => Preferences.Default.Get("AnkiSoundsFolder", Path.GetTempPath());

        public void SetAnkiSoundsFolder(string path) => Preferences.Default.Set("AnkiSoundsFolder", path);

        public string GetMp3gainPath() => Preferences.Default.Get("Mp3gainPath", string.Empty);

        public void SetMp3gainPath(string path) => Preferences.Default.Set("Mp3gainPath", path);

        public bool UseMp3gain
        {
            get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Preferences.Default.Get<bool>("UseMp3gain", false);
            set => Preferences.Default.Set("UseMp3gain", value);
        }

        public bool UseTranslator
        {
            get => Preferences.Default.Get<bool>("UseTranslator", false);
            set => Preferences.Default.Set("UseTranslator", value);
        }

        public string SelectedParser
        {
            get => Preferences.Default.Get("SelectedParser", string.Empty);
            set => Preferences.Default.Set("SelectedParser", value);
        }

        #region Private Methods

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

        public string GetTranslatorApiUrl() => Preferences.Default.Get("TranslatorApi", string.Empty);

        public void SetTranslatorApiUrl(string url) => Preferences.Default.Set("TranslatorApi", url);

        #endregion
    }
}
