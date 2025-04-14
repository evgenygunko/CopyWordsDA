namespace CopyWords.Parsers.Models.Translations.Input
{
    public record TranslationInput(
        string Version,
        string SourceLanguage,
        IEnumerable<string> DestinationLanguages,
        IEnumerable<Definition> Definitions);

    public record Definition(
        int id,
        Headword Headword,
        IEnumerable<Context> Contexts);

    public record Context(
        int id,
        string ContextEN,
        IEnumerable<Meaning> Meanings);

    public record Headword(
        string Text,
        string Meaning,
        string PartOfSpeech,
        IEnumerable<string> Examples);

    public record Meaning(
        int id,
        string Text,
        IEnumerable<string> Examples);
}
