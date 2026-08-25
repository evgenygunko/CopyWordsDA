// Ignore Spelling: Dict

namespace CopyWords.Parsers.Models.SpanishDict
{
    public record Meaning(
        string Original,
        string AlphabeticalPosition,
        string? ImageUrl,
        string? LookupUrl,
        IEnumerable<CopyWords.Parsers.Models.Example> Examples);
}
