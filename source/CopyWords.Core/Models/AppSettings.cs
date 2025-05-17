// Ignore Spelling: Ffmpeg Api App

namespace CopyWords.Core.Models
{
    public class AppSettings
    {
        public double MainWindowWidth { get; set; }

        public double MainWindowHeight { get; set; }

        public double MainWindowXPos { get; set; }

        public double MainWindowYPos { get; set; }

        public string AnkiSoundsFolder { get; set; } = string.Empty;

        public string FfmpegBinFolder { get; set; } = string.Empty;

        public string Mp3gainPath { get; set; } = string.Empty;

        public bool UseMp3gain { get; set; }

        public bool TranslateMeanings { get; set; }

        public bool TranslateHeadword { get; set; }

        public bool CopyTranslatedMeanings { get; set; }

        public string SelectedParser { get; set; } = string.Empty;
    }
}
