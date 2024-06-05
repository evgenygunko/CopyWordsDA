namespace CopyWords.Parsers.Models
{
    public record WordModel(string Headword, string? SoundUrl, string? SoundFileName, IEnumerable<Definition> Definitions);

    public record Definition(string Meaning, IEnumerable<string> Examples);

    // todo: find a better name and merge is into a WordModel?
    public record VariationUrl(string Word, string URL);
}
