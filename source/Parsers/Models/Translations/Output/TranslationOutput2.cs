namespace CopyWords.Parsers.Models.Translations.Output
{
    // The output model returned by the Azure function.
    public record TranslationOutput2(Headword[] Headword, Meaning[] Meanings);

    public record Headword(string Language, IEnumerable<string> HeadwordTranslations);

    public record Meaning(int id, MeaningTranslation[] MeaningTranslations);

    public record MeaningTranslation(string Language, string Text);
}
