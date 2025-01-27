// Ignore Spelling: Dict

namespace CopyWords.Parsers.Models.SpanishDict
{
    public record Meaning(
        string Original,
        string AlphabeticalPosition,
        string? ImageUrl,
        IEnumerable<CopyWords.Parsers.Models.Example> Examples);
}
