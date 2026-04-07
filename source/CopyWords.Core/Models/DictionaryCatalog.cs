namespace CopyWords.Core.Models
{
    public static class DictionaryCatalog
    {
        public static IReadOnlyList<DictionaryDefinition> All { get; } =
        [
            new(SourceLanguage.Danish, nameof(SourceLanguage.Danish), "flag_of_denmark.png"),
            new(SourceLanguage.Spanish, nameof(SourceLanguage.Spanish), "flag_of_spain.png")
        ];

        public static IReadOnlyList<string> AllKeys { get; } = All.Select(x => x.Language.ToString()).ToList();

        public static DictionaryDefinition GetRequired(string languageKey)
        {
            return All.FirstOrDefault(x => x.Language.ToString() == languageKey)
                ?? throw new NotSupportedException($"Source language '{languageKey}' is not supported.");
        }

        public static IReadOnlyList<string> NormalizeKeys(IEnumerable<string>? languageKeys)
        {
            List<string> normalized = [];

            foreach (string key in languageKeys ?? Enumerable.Empty<string>())
            {
                if (All.Any(x => x.Language.ToString() == key) && !normalized.Contains(key))
                {
                    normalized.Add(key);
                }
            }

            if (normalized.Count == 0)
            {
                return AllKeys;
            }

            return normalized;
        }
    }
}
