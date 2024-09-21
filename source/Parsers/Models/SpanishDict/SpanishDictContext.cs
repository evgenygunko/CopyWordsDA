namespace CopyWords.Parsers.Models.SpanishDict
{
    public record SpanishDictContext(
        string ContextEN,
        int Position,
        IEnumerable<Models.Meaning> Meanings);
}
