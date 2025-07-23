namespace CopyWords.Core.Models
{
    public enum SourceLanguage
    {
        Danish,
        Spanish
    }

    public record Options(SourceLanguage SourceLang, string TranslatorApiURL);
}
