// Ignore Spelling: Api App

namespace CopyWords.Core.Models
{
    public class AppSettings
    {
        public double MainWindowWidth { get; set; }

        public double MainWindowHeight { get; set; }

        public double MainWindowXPos { get; set; }

        public double MainWindowYPos { get; set; }

        public string AnkiSoundsFolder { get; set; } = string.Empty;

        public bool ShowCopyButtons { get; set; }

        public bool CopyTranslatedMeanings { get; set; }

        public string SelectedParser { get; set; } = string.Empty;
    }
}
