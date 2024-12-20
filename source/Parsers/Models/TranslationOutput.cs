﻿namespace CopyWords.Parsers.Models
{
    public record TranslationOutput(TranslationItem[] Translations);

    public record TranslationItem(string Language, IEnumerable<string> TranslationVariants);
}
