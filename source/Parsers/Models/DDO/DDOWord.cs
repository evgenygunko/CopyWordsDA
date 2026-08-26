using CopyWords.Parsers.Models;

namespace CopyWords.Parsers.Models.DDO
{
    public sealed record DDOWord(
        string Headword,
        string PartOfSpeech,
        string Endings,
        string Pronunciation,
        string SoundUrl,
        IReadOnlyList<DDODefinition> Definitions,
        IReadOnlyList<Variant> Variants,
        IReadOnlyList<Variant> Expressions);
}
