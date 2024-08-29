namespace CopyWords.Parsers.Models
{
    public record TranslationInput(string HeadWord, string Meaning, string sourceLanguage, IEnumerable<string> destinationLanguages);
}
