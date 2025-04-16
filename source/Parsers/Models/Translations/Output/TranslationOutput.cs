namespace CopyWords.Parsers.Models.Translations.Output
{
    // The output model returned by the Azure function.
    public record TranslationOutput(
        DefinitionOutput[] Definitions);

    public record DefinitionOutput(
        int id,
        string HeadwordTranslation,
        string HeadwordTranslationEnglish,
        ContextOutput[] Contexts);

    public record ContextOutput(
        int id,
        MeaningOutput[] Meanings);

    public record MeaningOutput(
        int id,
        string MeaningTranslation);
}
