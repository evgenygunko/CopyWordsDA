namespace CopyWords.Core.Models
{
    public record TranslationInput(
        string Version,
        string SourceLanguage,
        string DestinationLanguage,
        string[] Definitions,
        string Text);
}
