namespace CopyWords.Parsers.Models
{
    public record WordModel(
        string Word,
        List<VariationUrl> VariationUrls,
        string Endings, string Pronunciation,
        string Definitions, string Sound,
        List<string> Examples);
}
