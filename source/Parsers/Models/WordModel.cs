namespace CopyWords.Parsers.Models
{
    public record WordModel(string Word, string? SoundUrl, string? SoundFileName, IEnumerable<WordVariant>? Variants);

    public record WordVariant(string WordES, string Type, IEnumerable<Context> Contexts);

    public record Context(string ContextEN, int Position, IEnumerable<Translation> Translations);

    public record Translation(string English, string AlphabeticalPosition, string? ImageUrl, IEnumerable<Example> Examples);

    public record Example(string ExampleES, string ExampleEN);

    // todo: merge is into a context?
    public record VariationUrl(string Word, string URL);
}
