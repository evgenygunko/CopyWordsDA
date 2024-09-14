namespace CopyWords.Parsers.Models
{
    public record WordVariant(string WordES, string Type, IEnumerable<Context> Contexts);

    public record Context(string ContextEN, int Position, IEnumerable<Translation> Translations);
}
