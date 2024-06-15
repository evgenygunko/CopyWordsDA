namespace CopyWords.Parsers.Models
{
    public record WordModel(
        string Headword,
        string PartOfSpeech,
        string Endings,
        string? SoundUrl,
        string? SoundFileName,
        IEnumerable<Definition> Definitions,
        IEnumerable<Variation> Variations);

    public record Definition(string Meaning, string? Tag, IEnumerable<string> Examples);

    public record Variation(string Word, string Url);
}
