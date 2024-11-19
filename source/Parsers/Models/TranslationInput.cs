namespace CopyWords.Parsers.Models
{
    public record TranslationInput(string Word, string Meaning, string PartOfSpeech, string SourceLanguage, IEnumerable<string> DestinationLanguages);
}
