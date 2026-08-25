namespace CopyWords.Parsers.Models.Translations.Input
{
    public record TranslationInput(
        string Version,
        string SourceLanguage,
        string DestinationLanguage,
        IEnumerable<Definition> Definitions);

    public record Definition(
        int id,
        string PartOfSpeech,
        Headword Headword,
        IEnumerable<Context> Contexts);

    public record Context(
        int id,
        string ContextString,
        IEnumerable<Meaning> Meanings);

    public record Headword(
        string Text,
        string Meaning,
        IEnumerable<string> Examples);

    public record Meaning(
        int id,
        string Text,
        IEnumerable<string> Examples);
}
