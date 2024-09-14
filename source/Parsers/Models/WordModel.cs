namespace CopyWords.Parsers.Models
{
    public record WordModel(
        Headword Headword,
        string PartOfSpeech,
        string Endings, // only for Danish dictionary
        string? SoundUrl,
        string? SoundFileName,
        IEnumerable<Definition> Definitions,
        IEnumerable<Variant> Variations); // only for Danish dictionary

    public record Headword(string Original, string? English, string? Russian);

    public record Definition(string Meaning, string? Tag, int Position, IEnumerable<Translation> Translations);

    public record Translation(string English, string AlphabeticalPosition, string? ImageUrl, IEnumerable<Example> Examples);

    public record Example(string Original, string? English, string? Russian);

    public record Variant(string Word, string Url);
}
