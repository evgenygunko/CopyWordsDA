namespace CopyWords.Parsers.Models
{
    public record TranslationInput(string HeadWord, IEnumerable<string> Meanings, string sourceLanguage, IEnumerable<string> destinationLanguages);
}
