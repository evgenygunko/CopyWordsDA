namespace CopyWords.Parsers.Models.DDO
{
    public record DDODefinition(
        string Meaning,
        string? Tag,
        int Position,
        IEnumerable<Translation> Translations);
}
