namespace CopyWords.Core.Models
{
    public interface IGlobalSettings
    {
        string TranslatorApiUrl { get; set; }
    }

    public class GlobalSettings : IGlobalSettings
    {
        public string TranslatorApiUrl { get; set; } = null!;
    }
}
