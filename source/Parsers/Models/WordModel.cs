namespace CopyWords.Parsers.Models
{
    public record WordModel(
        string Word,
        string? SoundUrl,
        string? SoundFileName,
        IEnumerable<Definition> Definitions,
        IEnumerable<Variant> Variations); // only for Danish dictionary

    public record Definition(
        Headword Headword,
        string Meaning,
        string? Tag,
        string PartOfSpeech,
        string Endings, // only for Danish dictionary
        int Position,
        IEnumerable<Meaning> Meanings);

    public record Headword(
        string Original,
        string? English,
        string? Russian);

    public record Meaning(
        string English,
        string AlphabeticalPosition,
        string? ImageUrl,
        IEnumerable<Example> Examples);

    public record Example(
        string Original,
        string? English,
        string? Russian);

    /// <summary>
    /// List of related words (only for Danish dictionary)
    /// </summary>
    public record Variant(
        string Word,
        string Url);
}
