namespace CopyWords.Parsers.Models
{
    public record WordModel(
        string Headword,
        string PartOfSpeech,
        string Endings,
        string? SoundUrl,
        string? SoundFileName,
        IEnumerable<Definition> Definitions,
        IEnumerable<Variant> Variations);

    public record Definition(string Meaning, string? Tag, IEnumerable<string> Examples);

    public record Variant(string Word, string Url);
}
