namespace CopyWords.Parsers.Models
{
    public record TranslationOutput(string Language, string? HeadWord, IEnumerable<string?> Meanings);
}
