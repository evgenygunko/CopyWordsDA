namespace CopyWords.Parsers.Models
{
    public record WordModel(string Headword, string? SoundUrl, string? SoundFileName, IEnumerable<Definition> Definitions);

    public record Definition(string Meaning, IEnumerable<string> Examples);
}
