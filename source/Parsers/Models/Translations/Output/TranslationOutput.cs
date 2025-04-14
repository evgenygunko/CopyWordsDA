namespace CopyWords.Parsers.Models.Translations.Output
{
    // The output model returned by the Azure function.
    public record TranslationOutput(DefinitionTranslations[] Definitions);

    public record DefinitionTranslations(
        int id,
        Headword[] Headword,
        Meaning[] Meanings);

    public record Headword(string Language, IEnumerable<string> HeadwordTranslations);

    public record Meaning(int id, MeaningTranslation[] MeaningTranslations);

    public record MeaningTranslation(string Language, string Text);
}
