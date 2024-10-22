namespace CopyWords.Core.Models
{
    public class AppSettings
    {
        public double MainWindowWidth { get; set; }

        public double MainWindowHeight { get; set; }

        public double MainWindowXPos { get; set; }

        public double MainWindowYPos { get; set; }

        public string AnkiSoundsFolder { get; set; }

        public string FfmpegBinFolder { get; set; }

        public string Mp3gainPath { get; set; }

        public bool UseMp3gain { get; set; }

        public string TranslatorApiUrl { get; set; }

        public bool UseTranslator { get; set; }

        public string SelectedParser { get; set; }
    }
}
