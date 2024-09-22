﻿namespace CopyWords.Parsers.Models
{
    public record WordModel(
        string Word,
        string? SoundUrl,
        string? SoundFileName,
        IEnumerable<Definition> Definitions,
        IEnumerable<Variant> Variations); // only for Danish dictionary

    public record Definition(
        Headword Headword,
        string PartOfSpeech,
        string Endings, // only for Danish dictionary
        IEnumerable<Context> Contexts);

    public record Headword(
        string Original,
        string? English,
        string? Russian);

    public record Context(
        string ContextEN,
        int Position,
        IEnumerable<Meaning> Meanings);

    public record Meaning(
        string Description,
        string AlphabeticalPosition,
        string? Tag,
        string? ImageUrl,
        IEnumerable<Example> Examples);

    public record Example(
        string Original,
        string? English,
        string? Russian);

    /// <summary>
    /// List of related words (only for Danish dictionary)
    /// </summary>
    public record Variant(
        string Word,
        string Url);
}
