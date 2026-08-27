namespace CopyWords.Core.Models
{
    public record SuggestionsRequest(
        string Text,
        string DestinationLanguage);
}
