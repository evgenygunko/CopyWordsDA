namespace CopyWords.Parsers.Models.DDO
{
    public record DDODefinition(
        string Meaning,
        string? Tag,
        IEnumerable<Example> Examples);
}
