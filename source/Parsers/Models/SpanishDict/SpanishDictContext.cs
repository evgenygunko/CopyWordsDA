// Ignore Spelling: Dict

namespace CopyWords.Parsers.Models.SpanishDict
{
    public record SpanishDictContext(
        string ContextEN,
        int Position,
        IEnumerable<Meaning> Meanings);
}
