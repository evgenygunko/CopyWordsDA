// Ignore Spelling: Syncfusion Api Dsn

namespace CopyWords.Core.Models
{
    public interface IGlobalSettings
    {
        string TranslatorAppUrl { get; set; }

        string TranslatorAppRequestCode { get; set; }

        string SyncfusionLicenseKey { get; set; }

        string SentryDsn { get; set; }

        string LaunchDarklyMobileKey { get; set; }

        string LaunchDarklyMemberId { get; set; }
    }

    public class GlobalSettings : IGlobalSettings
    {
        public string TranslatorAppUrl { get; set; } = null!;

        public string TranslatorAppRequestCode { get; set; } = null!;

        public string SyncfusionLicenseKey { get; set; } = null!;

        public string SentryDsn { get; set; } = null!;

        public string LaunchDarklyMobileKey { get; set; } = null!;

        public string LaunchDarklyMemberId { get; set; } = null!;
    }
}
