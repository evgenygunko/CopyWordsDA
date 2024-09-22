namespace CopyWords.Parsers.Models.DDO
{
    public record DDODefinition(
        int Position,
        IEnumerable<Meaning> Meanings);
}
