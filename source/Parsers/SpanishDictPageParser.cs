// Ignore Spelling: Json Dict

using System.Web;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models.SpanishDict;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CopyWords.Parsers
{
    public interface ISpanishDictPageParser
    {
        WordJsonModel? ParseWordJson(string htmlPage);

        string ParseHeadword(WordJsonModel wordObj);

        string? ParseSoundURL(WordJsonModel wordObj);

        SpanishDictDefinition ParseDefinition(WordJsonModel wordObj, string? n = null, string? p = null);

        List<Models.Variant> ParseVariants(WordJsonModel wordObj);
    }

    public class SpanishDictPageParser : ISpanishDictPageParser
    {
        public const string SpanishDictBaseUrl = "https://www.spanishdict.com/translate/";
        internal const string SoundBaseUrl = "https://d10gt6izjc94x0.cloudfront.net/desktop/";
        internal const string ImageBaseUrl = "https://sd-hegemone-production.sdcdns.com/dictionary-images/300/";

        #region Public Methods

        public WordJsonModel? ParseWordJson(string htmlPage)
        {
            WordJsonModel? wordObj = null;

            var scripts = FindScripts(htmlPage);

            if (scripts != null)
            {
                // find a script with details about word
                HtmlNode? htmlNode = scripts.FirstOrDefault(x =>
                    (x.ChildNodes.Count == 1)
                    && (x.FirstChild.InnerHtml.TrimStart().StartsWith("window.SD_COMPONENT_DATA", StringComparison.InvariantCulture)));

                if (htmlNode != null)
                {
                    string json = htmlNode.InnerHtml
                        .TrimStart()
                        .Replace("window.SD_COMPONENT_DATA", string.Empty, StringComparison.InvariantCulture)
                        .TrimStart()
                        .TrimStart('=')
                        .TrimEnd()
                        .TrimEnd(';');

                    wordObj = JsonConvert.DeserializeObject<WordJsonModel>(json);

                    // Now SpanishDict returns a page with a widget from Microsoft Translator when it can't find a word in its database.
                    // We want to return "not found" in this case.
                    if (wordObj?.resultCardHeaderProps == null)
                    {
                        wordObj = null;
                    }
                }
            }

            return wordObj;
        }

        /// <summary>
        /// Gets a string which contains Spanish word.
        /// </summary>
        public string ParseHeadword(WordJsonModel wordObj)
        {
            return wordObj
                .resultCardHeaderProps
                .headwordAndQuickdefsProps
                .headword
                .displayText;
        }

        /// <summary>
        /// Gets ID of the sound file (which would be part of URL).
        /// </summary>
        public string? ParseSoundURL(WordJsonModel wordObj)
        {
            Models.SpanishDict.Pronunciation[] pronunciations = wordObj
                .resultCardHeaderProps
                .headwordAndQuickdefsProps
                .headword
                .pronunciations;

            Pronunciation? pronunciation = pronunciations.FirstOrDefault(x => x.region == "SPAIN" && x.hasVideo == 1);
            if (pronunciation == null)
            {
                // If can't find a sound for the Spanish region, try to find one for Latin America.
                pronunciation = pronunciations.FirstOrDefault(x => x.region == "LATAM" && x.hasVideo == 1);
            }

            string? soundURL = null;
            if (pronunciation != null)
            {
                soundURL = $"{SoundBaseUrl}lang_es_pron_{pronunciation.id}_speaker_{pronunciation.speakerId}_syllable_all_version_{pronunciation.version}.mp4";
            }

            return soundURL;
        }

        public SpanishDictDefinition ParseDefinition(WordJsonModel wordObj, string? n = null, string? p = null)
        {
            Neodict[]? neodicts = wordObj.sdDictionaryResultsProps.entry?.neodict;
            if (neodicts == null)
            {
                throw new SpanishDictPageParserException("No 'neodict' entries found in the provided WordJsonModel.");
            }

            int neodictIndex;
            if (!int.TryParse(n, out neodictIndex))
            {
                neodictIndex = 0;
            }

            Neodict neodict = neodicts[neodictIndex];

            // WordVariant: WordES + WortType
            //      Contexts
            //          Translations
            //              Examples
            int posGroupsIndex;
            if (!int.TryParse(p, out posGroupsIndex))
            {
                posGroupsIndex = 0;
            }
            Posgroup posgroup = neodict.posGroups[posGroupsIndex];

            string wordES = neodict.subheadword;
            string partOfSpeech = posgroup.posDisplay.name;

            var senses = posgroup.senses;

            var contexts = new List<SpanishDictContext>();
            int contextPosition = 1;
            foreach (Sens sens in senses)
            {
                var meanings = new List<Meaning>();
                int translationPosition = 0;

                foreach (Translation tr in sens.translations)
                {
                    var examples = new List<Models.Example>();
                    foreach (Example ex in tr.examples)
                    {
                        examples.Add(new Models.Example(Original: ex.textEs, Translation: ex.textEn));
                    }

                    string alphabeticalPosition = char.ConvertFromUtf32((int)'a' + translationPosition++);

                    // Add translation, e.g. "cool (colloquial)"
                    string fullTranslation = tr.translation;

                    string? label = tr.registerLabels.FirstOrDefault()?.nameEn;
                    if (!string.IsNullOrEmpty(label))
                    {
                        fullTranslation = $"{fullTranslation} ({label})";
                    }

                    if (!string.IsNullOrEmpty(tr.contextEn))
                    {
                        fullTranslation = $"{fullTranslation} ({tr.contextEn})";
                    }

                    string? imageUrl = null;
                    if (!string.IsNullOrEmpty(tr.imagePath))
                    {
                        string imageId = tr.imagePath.Split('/').Last();

                        // Sometimes images are encoded, but with some very strict rules. Try to decode and then encode again.
                        string decoded = HttpUtility.UrlDecode(imageId);
                        string encoded = decoded
                            .Replace(",", "%2C")
                            .Replace(";", "%3B")
                            .Replace("'", "%27")
                            .Replace("(", "%28")
                            .Replace(")", "%29")
                            .Replace(" ", "%20")
                            .Replace("%", "%25");

                        imageUrl = ImageBaseUrl + encoded;
                    }

                    // https://www.spanishdict.com/translate/to%20shave?langFrom=en
                    string lookupUrl = $"{SpanishDictBaseUrl}{HttpUtility.UrlEncode(tr.translation)}?langFrom={wordObj.langTo}";

                    meanings.Add(new Meaning(Original: fullTranslation, AlphabeticalPosition: alphabeticalPosition, ImageUrl: imageUrl, LookupUrl: lookupUrl, Examples: examples));
                }

                // Add context, e.g. "(colloquial) (extremely good) (Spain)"
                string? contextLabel = sens.registerLabels.FirstOrDefault()?.nameEn;
                string? contextRegion = sens.regions.FirstOrDefault()?.nameEn;

                // context always has parenthesis in spanishdict.com UI
                string fullContext = $"({sens.context})";

                if (!string.IsNullOrEmpty(contextLabel))
                {
                    fullContext = $"({contextLabel}) " + fullContext;
                }

                if (!string.IsNullOrEmpty(contextRegion))
                {
                    fullContext += $" ({contextRegion})";
                }

                contexts.Add(new SpanishDictContext(fullContext, contextPosition++, meanings));
            }

            return new SpanishDictDefinition(wordES, partOfSpeech, contexts);
        }

        public List<Models.Variant> ParseVariants(WordJsonModel wordObj)
        {
            var variants = new List<Models.Variant>();

            Neodict[]? neodicts = wordObj.sdDictionaryResultsProps.entry?.neodict;

            if (neodicts == null)
            {
                return variants;
            }

            string baseUrl = wordObj.siteUrlBase ?? SpanishDictBaseUrl.TrimEnd('/');
            string searchTerm = wordObj
                .resultCardHeaderProps
                .headwordAndQuickdefsProps
                .headword
                .displayText;
            string encodedSearchTerm = HttpUtility.UrlEncode(searchTerm);

            for (int neodictIndex = 0; neodictIndex < neodicts.Length; neodictIndex++)
            {
                var neodict = neodicts[neodictIndex];

                for (int posgroupIndex = 0; posgroupIndex < neodict.posGroups.Length; posgroupIndex++)
                {
                    Posgroup posgroup = neodict.posGroups[posgroupIndex];

                    string wordES = posgroup.senses[0].subheadword;
                    string partOfSpeech = posgroup.posDisplay.name;

                    string variantText = $"{wordES} ({partOfSpeech})";
                    string variantUrl = $"{baseUrl}/translate/{encodedSearchTerm}?n={neodictIndex}&p={posgroupIndex}";

                    variants.Add(new Models.Variant(variantText, variantUrl));
                }
            }

            return variants;
        }

        #endregion

        #region Private Methods

        private static HtmlNodeCollection? FindScripts(string htmlPage)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlPage);

            if (htmlDocument.DocumentNode == null)
            {
                throw new PageParserException("DocumentNode is null for the loaded page, please check that it has a valid html content.");
            }

            return htmlDocument.DocumentNode.SelectNodes("//script");
        }

        #endregion
    }
}
