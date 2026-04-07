namespace CopyWords.Core.Models
{
    public sealed record DictionaryDefinition(
        SourceLanguage Language,
        string DisplayName,
        string ImageName);
}
