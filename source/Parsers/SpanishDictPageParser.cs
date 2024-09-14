﻿using System.Globalization;
using System.Web;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CopyWords.Parsers
{
    public interface ISpanishDictPageParser
    {
        Models.SpanishDict.WordJsonModel? ParseWordJson(string htmlPage);

        string ParseHeadword(Models.SpanishDict.WordJsonModel wordObj);

        string? ParseSound(Models.SpanishDict.WordJsonModel? wordObj);

        string CreateSoundURL(string sound);

        IEnumerable<WordVariant> ParseTranslations(Models.SpanishDict.WordJsonModel? wordObj);
    }

    public class SpanishDictPageParser : ISpanishDictPageParser
    {
        internal const string SpanishDictBaseUrl = "https://www.spanishdict.com/translate/";
        internal const string SoundBaseUrl = "https://d10gt6izjc94x0.cloudfront.net/desktop/";
        internal const string ImageBaseUrl = "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/";

        #region Public Methods

        public Models.SpanishDict.WordJsonModel? ParseWordJson(string htmlPage)
        {
            Models.SpanishDict.WordJsonModel? wordObj = null;

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

                    wordObj = JsonConvert.DeserializeObject<Models.SpanishDict.WordJsonModel>(json);

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
        public string ParseHeadword(Models.SpanishDict.WordJsonModel wordObj)
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
        public string? ParseSound(Models.SpanishDict.WordJsonModel? wordObj)
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

        public IEnumerable<WordVariant> ParseTranslations(Models.SpanishDict.WordJsonModel? wordObj)
        {
            if (wordObj == null)
            {
                return Enumerable.Empty<WordVariant>();
            }

            var variants = new List<WordVariant>();

            Models.SpanishDict.Neodict[]? neodicts = wordObj.sdDictionaryResultsProps.entry?.neodict;

            if (neodicts == null)
            {
                return variants;
            }

            foreach (Models.SpanishDict.Neodict neodict in neodicts)
            {
                // WordVariant: WordES + WortType
                //      Contexts
                //          Translations
                //              Examples
                Models.SpanishDict.Posgroup posgroup = neodict.posGroups[0];
                string wordES = neodict.subheadword;
                string wordType = posgroup.posDisplay.name;

                // Flatten the list  - some words have 2 elements in neodict (hipócrita) and some just one (afeitar)
                var senses = neodict.posGroups.SelectMany(x => x.senses);

                var contexts = new List<Context>();
                int contextPosition = 1;
                foreach (Models.SpanishDict.Sens sens in senses)
                {
                    var translations = new List<Translation>();
                    int translationPosition = 0;

                    foreach (Models.SpanishDict.Translation tr in sens.translations)
                    {
                        var examples = new List<Example>();
                        foreach (Models.SpanishDict.Example ex in tr.examples)
                        {
                            examples.Add(new Example(Original: ex.textEs, English: ex.textEn, Russian: null));
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
                                .Replace(" ", "%20")
                                .Replace("%", "%25");

                            imageUrl = ImageBaseUrl + encoded;
                        }

                        translations.Add(new Translation(fullTranslation, alphabeticalPosition, imageUrl, examples));
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

                    contexts.Add(new Context(fullContext, contextPosition++, translations));
                }

                variants.Add(new WordVariant(wordES, wordType, contexts));
            }

            return variants;
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