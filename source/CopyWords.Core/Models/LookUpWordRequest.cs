namespace CopyWords.Core.Models
{
    public record LookUpWordRequest(
        string Text,
        string SourceLanguage,
        string DestinationLanguage,
        string Version);
}
