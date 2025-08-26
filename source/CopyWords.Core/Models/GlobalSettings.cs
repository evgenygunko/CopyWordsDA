// Ignore Spelling: Syncfusion Api

namespace CopyWords.Core.Models
{
    public interface IGlobalSettings
    {
        string TranslatorApiUrl { get; set; }

        string SyncfusionLicenseKey { get; set; }
    }

    public class GlobalSettings : IGlobalSettings
    {
        public string TranslatorApiUrl { get; set; } = null!;

        public string SyncfusionLicenseKey { get; set; } = null!;
    }
}
