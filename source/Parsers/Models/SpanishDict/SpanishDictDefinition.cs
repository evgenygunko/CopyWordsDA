namespace CopyWords.Parsers.Models.SpanishDict
{
    public record SpanishDictDefinition(
        string WordES,
        string Type,
        IEnumerable<SpanishDictContext> Contexts);
}
