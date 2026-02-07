using System.Text.Json.Serialization;
using CopyWords.Core.Converters;

namespace CopyWords.Core.Models
{
    public record WordModel(
       string Word,
       SourceLanguage SourceLanguage,
       string? SoundUrl,
       string? SoundFileName,
       Definition Definition,
       IEnumerable<Variant> Variants,
       IEnumerable<Variant> Expressions);

    public record Definition(
        Headword Headword,
        string PartOfSpeech,
        string Endings, // only for Danish dictionary
        IEnumerable<Context> Contexts);

    /// <summary>
    /// Represents the headword with translations.
    /// The converter supports both "Translation" (new) and "Russian" (legacy) property names for backward compatibility.
    /// </summary>
    [JsonConverter(typeof(HeadwordJsonConverter))]
    public record Headword(
        string Original,
        string? English,
        string? Translation);

    public record Context(
        string ContextEN,
        string Position,
        IEnumerable<Meaning> Meanings);

    public record Meaning(
        string Original,
        string? Translation,
        string AlphabeticalPosition,
        string? Tag,
        string? ImageUrl,
        IEnumerable<Example> Examples);

    public record Example(
        string Original,
        string? Translation);

    /// <summary>
    /// List of related words (only for Danish dictionary)
    /// </summary>
    public record Variant(
        string Word,
        string Url);
}
