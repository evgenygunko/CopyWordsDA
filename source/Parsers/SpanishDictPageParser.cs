using System.Globalization;
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

        string? ParseSound(WordJsonModel? wordObj);

        string CreateSoundURL(string sound);

        IEnumerable<SpanishDictDefinition> ParseDefinitions(WordJsonModel? wordObj);
    }

    public class SpanishDictPageParser : ISpanishDictPageParser
    {
        internal const string SpanishDictBaseUrl = "https://www.spanishdict.com/translate/";
        internal const string SoundBaseUrl = "https://d10gt6izjc94x0.cloudfront.net/desktop/";
        internal const string ImageBaseUrl = "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/";

        #region Public Methods

        public WordJsonModel? ParseWordJson(string htmlPage)
        {
            WordJsonModel? wordObj = null;

            IEnumerable<HtmlNode> scripts = FindScripts(htmlPage);

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
            if (wordObj == null)
            {
                throw new ArgumentNullException(nameof(wordObj));
            }

            return wordObj
                .resultCardHeaderProps
                .headwordAndQuickdefsProps
                .headword
                .displayText;
        }

        /// <summary>
        /// Gets ID of the sound file (which would be part of URL).
        /// </summary>
        public string? ParseSound(WordJsonModel? wordObj)
        {
            if (wordObj == null)
            {
                return null;
            }

            string? soundId = null;

            Models.SpanishDict.Pronunciation[] pronunciations = wordObj
                .resultCardHeaderProps
                .headwordAndQuickdefsProps
                .headword
                .pronunciations;

            foreach (Models.SpanishDict.Pronunciation pronunciation in pronunciations)
            {
                if (string.Equals(pronunciation.region, "SPAIN", StringComparison.OrdinalIgnoreCase)
                    && pronunciation.speakerId == 7)
                {
                    soundId = pronunciation.id.ToString(CultureInfo.InvariantCulture);
                    break;
                }
            }

            return soundId;
        }

        public string CreateSoundURL(string sound) => $"{SoundBaseUrl}lang_es_pron_{sound}_speaker_7_syllable_all_version_50.mp4";

        public IEnumerable<SpanishDictDefinition> ParseDefinitions(WordJsonModel? wordObj)
        {
            if (wordObj == null)
            {
                return Enumerable.Empty<SpanishDictDefinition>();
            }

            var spanishDictDefinitions = new List<SpanishDictDefinition>();

            Neodict[]? neodicts = wordObj.sdDictionaryResultsProps.entry?.neodict;

            if (neodicts == null)
            {
                return spanishDictDefinitions;
            }

            foreach (Neodict neodict in neodicts)
            {
                // WordVariant: WordES + WortType
                //      Contexts
                //          Translations
                //              Examples
                Posgroup posgroup = neodict.posGroups[0];
                string wordES = neodict.subheadword;
                string partOfSpeech = posgroup.posDisplay.name;

                // Flatten the list  - some words have 2 elements in neodict (hipócrita) and some just one (afeitar)
                var senses = neodict.posGroups.SelectMany(x => x.senses);

                var contexts = new List<SpanishDictContext>();
                int contextPosition = 1;
                foreach (Sens sens in senses)
                {
                    var meanings = new List<Models.Meaning>();
                    int translationPosition = 0;

                    foreach (Translation tr in sens.translations)
                    {
                        var examples = new List<Models.Example>();
                        foreach (Example ex in tr.examples)
                        {
                            examples.Add(new Models.Example(Original: ex.textEs, English: ex.textEn, Russian: null));
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
                            string encoded = decoded.Replace(",", "%2C")
                                .Replace(";", "%3B")
                                .Replace(" ", "%20")
                                .Replace("%", "%25");

                            imageUrl = ImageBaseUrl + encoded;
                        }

                        meanings.Add(new Models.Meaning(fullTranslation, alphabeticalPosition, Tag: null, imageUrl, examples));
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

                spanishDictDefinitions.Add(new SpanishDictDefinition(wordES, partOfSpeech, contexts));
            }

            return spanishDictDefinitions;
        }

        #endregion

        #region Private Methods

        private static HtmlNodeCollection FindScripts(string htmlPage)
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
