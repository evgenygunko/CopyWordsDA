namespace CopyWords.Parsers.Models.SpanishDict
{
    public record SpanishDictDefinition(
        string WordES,
        string PartOfSpeech,
        IEnumerable<SpanishDictContext> Contexts);
}
