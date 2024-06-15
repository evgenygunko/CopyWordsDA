﻿namespace CopyWords.Parsers.Models
{
    public record WordModel(
        string Headword,
        string PartOfSpeech,
        string Endings,
        string? SoundUrl,
        string? SoundFileName,
        IEnumerable<Definition> Definitions);

    public record Definition(string Meaning, string? Tag, IEnumerable<string> Examples);
}
