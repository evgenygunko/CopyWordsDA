namespace CopyWords.Core.Models
{
    public enum SourceLanguage
    {
        Danish,
        Spanish
    }

    public record Options(SourceLanguage SourceLang, string? TranslatorApiURL, bool TranslateHeadword, bool TranslateMeanings);
}
