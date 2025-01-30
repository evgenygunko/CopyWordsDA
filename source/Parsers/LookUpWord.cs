// Ignore Spelling: Downloader Dict ddo

using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Models.DDO;
using CopyWords.Parsers.Services;

namespace CopyWords.Parsers
{
    public interface ILookUpWord
    {
        (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp);

        Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options options);

        Task<WordModel?> GetWordByUrlAsync(string url, Options options);
    }

    public class LookUpWord : ILookUpWord
    {
        private const string LanguageDA = "da";
        private const string LanguageES = "es";
        private const string LanguageRU = "ru";
        private const string LanguageEN = "en";
        private readonly string[] DestinationLanguages = [LanguageRU, LanguageEN];

        private readonly IDDOPageParser _ddoPageParser;
        private readonly ISpanishDictPageParser _spanishDictPageParser;
        private readonly IFileDownloader _fileDownloader;
        private readonly ITranslatorAPIClient _translatorAPIClient;

        private readonly Regex _lookupRegex = new Regex(@"^[\w ]+$");

        public LookUpWord(
            IDDOPageParser ddoPageParser,
            ISpanishDictPageParser spanishDictPageParser,
            IFileDownloader fileDownloader,
            ITranslatorAPIClient translatorAPIClient)
        {
            _ddoPageParser = ddoPageParser;
            _spanishDictPageParser = spanishDictPageParser;
            _fileDownloader = fileDownloader;
            _translatorAPIClient = translatorAPIClient;
        }

        #region Public Methods

        public (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp)
        {
            bool isValid = false;
            string? errorMessage = null;

            if (string.IsNullOrEmpty(lookUp))
            {
                errorMessage = "LookUp text cannot be null or empty.";
            }
            else
            {
                isValid = _lookupRegex.IsMatch(lookUp);

                if (!isValid)
                {
                    errorMessage = "Search can only contain alphanumeric characters and spaces.";
                }
            }

            return (isValid, errorMessage);
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options options)
        {
            (bool isValid, string? errorMessage) = CheckThatWordIsValid(wordToLookUp);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(wordToLookUp));
            }

            string url;
            if (options.SourceLang == SourceLanguage.Danish)
            {
                url = DDOPageParser.DDOBaseUrl + $"?query={wordToLookUp}&search=S%C3%B8g";
            }
            else
            {
                url = SpanishDictPageParser.SpanishDictBaseUrl + wordToLookUp;
            }

            var wordModel = await GetWordByUrlAsync(url, options);
            return wordModel;
        }

        public async Task<WordModel?> GetWordByUrlAsync(string url, Options options)
        {
            // Download and parse a page from the online dictionary
            string? html = await _fileDownloader.DownloadPageAsync(url, Encoding.UTF8);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }

            switch (options.SourceLang)
            {
                case SourceLanguage.Danish:
                    return await ParseDanishWordAsync(html, options);

                case SourceLanguage.Spanish:
                    return await ParseSpanishWordAsync(html, options);

                default:
                    throw new ArgumentException($"Source language '{options.SourceLang}' is not supported");
            }
        }

        #endregion

        #region Internal Methods

        internal async Task<WordModel?> ParseDanishWordAsync(string html, Options options)
        {
            // Download and parse a page from DDO
            _ddoPageParser.LoadHtml(html);
            string headWordDA = _ddoPageParser.ParseHeadword();

            string partOfSpeech = _ddoPageParser.ParsePartOfSpeech();
            string endings = _ddoPageParser.ParseEndings();

            string soundUrl = _ddoPageParser.ParseSound();
            string soundFileName = string.IsNullOrEmpty(soundUrl) ? string.Empty : $"{headWordDA}.mp3";

            List<DDODefinition> ddoDefinitions = _ddoPageParser.ParseDefinitions();

            Headword headword = await CreateHeadwordModelForDanishAsync(options, headWordDA, partOfSpeech, ddoDefinitions);

            // For DDO, we create one Definition with one Context and several Meanings.
            List<Meaning> meanings = new List<Meaning>();
            int pos = 1;
            foreach (var ddoDefinition in ddoDefinitions)
            {
                // Safe check - some words have a lot of meanings, we will only translate first 5.
                string? meaningTranslation = null;
                if (options.TranslateMeanings && pos <= 5)
                {
                    meaningTranslation = await TranslateMeaningAsync(options.TranslatorApiURL, LanguageDA, ddoDefinition.Meaning, ddoDefinition.Examples.Select(x => x.Original));
                }
                meanings.Add(new Meaning(Original: ddoDefinition.Meaning, Translation: meaningTranslation, AlphabeticalPosition: (pos++).ToString(), ddoDefinition.Tag, ImageUrl: null, Examples: ddoDefinition.Examples));
            }

            Context context = new Context(ContextEN: "", Position: "", meanings);
            Definition definition = new Definition(headword, partOfSpeech, endings, [context]);

            var wordModel = new WordModel(
                Word: headWordDA,
                SoundUrl: soundUrl,
                SoundFileName: soundFileName,
                Definitions: [definition],
                Variations: _ddoPageParser.ParseVariants()
            );

            return wordModel;
        }

        internal async Task<WordModel?> ParseSpanishWordAsync(string html, Options options)
        {
            Models.SpanishDict.WordJsonModel? wordObj = _spanishDictPageParser.ParseWordJson(html);
            if (wordObj == null)
            {
                return null;
            }

            string headwordES = _spanishDictPageParser.ParseHeadword(wordObj);

            string? soundUrl = _spanishDictPageParser.ParseSoundURL(wordObj);
            string? soundFileName = null;

            if (!string.IsNullOrEmpty(soundUrl))
            {
                soundFileName = $"{headwordES}.mp4";
            }

            // SpanishDict can return several definitions (e.g. for a "transitive verb" and "reflexive verb").
            IEnumerable<Models.SpanishDict.SpanishDictDefinition> spanishDictDefinitions = _spanishDictPageParser.ParseDefinitions(wordObj);

            List<Definition> definitions = new();
            foreach (var spanishDictDefinition in spanishDictDefinitions)
            {
                List<Context> contexts = new();
                foreach (var spanishDictContext in spanishDictDefinition.Contexts)
                {
                    // We don't want to translate meanings for Spanish words. They usually are very short and consist of one word.
                    IEnumerable<Meaning> meanings = spanishDictContext.Meanings.Select(
                        x => new Meaning(Original: x.Original, Translation: null, AlphabeticalPosition: x.AlphabeticalPosition, Tag: null, ImageUrl: x.ImageUrl, Examples: x.Examples));
                    contexts.Add(new Context(spanishDictContext.ContextEN, spanishDictContext.Position.ToString(), meanings));
                }

                Headword headword = await CreateHeadwordModelForSpanishAsync(options, spanishDictDefinition, contexts);

                // Spanish words don't have endings, this property only makes sense for Danish
                definitions.Add(new Definition(headword, spanishDictDefinition.PartOfSpeech, Endings: "", contexts));
            }

            var wordModel = new WordModel(
                Word: headwordES,
                SoundUrl: soundUrl,
                SoundFileName: soundFileName,
                Definitions: definitions,
                Variations: [] // there are no word variants in SpanishDict
            );

            return wordModel;
        }

        internal async Task<Headword> CreateHeadwordModelForDanishAsync(Options options, string headWordDA, string partOfSpeech, List<DDODefinition> ddoDefinitions)
        {
            string? translationsEN = null;
            string? translationsRU = null;

            // If TranslatorAPI URL is configured, call translator app and add returned translations to the headword model.
            if (options.TranslateHeadword && !string.IsNullOrEmpty(options.TranslatorApiURL))
            {
                var firstDefinition = ddoDefinitions.FirstOrDefault();
                IEnumerable<string> examples = firstDefinition?.Examples.Select(x => x.Original) ?? [];

                TranslationOutput translationOutput = await GetTranslationAsync(
                            options.TranslatorApiURL,
                            sourceLanguage: LanguageDA,
                            word: headWordDA,
                            meaning: firstDefinition?.Meaning ?? "",
                            partOfSpeech: partOfSpeech,
                            examples: examples);

                IEnumerable<string>? translationVariantsEN = translationOutput.Translations.FirstOrDefault(x => x.Language == LanguageEN)?.TranslationVariants;
                translationsEN = string.Join(", ", translationVariantsEN ?? []);

                IEnumerable<string>? translationVariantsRU = translationOutput.Translations.FirstOrDefault(x => x.Language == LanguageRU)?.TranslationVariants;
                translationsRU = string.Join(", ", translationVariantsRU ?? []);
            }

            return new Headword(headWordDA, translationsEN, translationsRU);
        }

        internal async Task<Headword> CreateHeadwordModelForSpanishAsync(Options options, Models.SpanishDict.SpanishDictDefinition spanishDictDefinition, List<Context> contexts)
        {
            string? translationsEN = null;
            string? translationsRU = null;

            // If TranslatorAPI URL is configured, call translator app and add returned translations to word model.
            if (options.TranslateHeadword && !string.IsNullOrEmpty(options.TranslatorApiURL))
            {
                string meaningToTranslate = contexts.FirstOrDefault()?.Meanings.FirstOrDefault()?.Original + " " + contexts.FirstOrDefault()?.ContextEN;
                IEnumerable<string> examplesToTranslate = contexts.FirstOrDefault()?.Meanings.FirstOrDefault()?.Examples.Select(x => x.Original)
                    ?? [];

                TranslationOutput translationOutput = await GetTranslationAsync(
                    options?.TranslatorApiURL,
                    sourceLanguage: LanguageES,
                    word: spanishDictDefinition.WordES,
                    meaning: meaningToTranslate,
                    partOfSpeech: spanishDictDefinition.PartOfSpeech,
                    examples: examplesToTranslate);

                IEnumerable<string>? translationVariantsEN = translationOutput.Translations.FirstOrDefault(x => x.Language == LanguageEN)?.TranslationVariants;
                translationsEN = string.Join(", ", translationVariantsEN ?? []);

                IEnumerable<string>? translationVariantsRU = translationOutput.Translations.FirstOrDefault(x => x.Language == LanguageRU)?.TranslationVariants;
                translationsRU = string.Join(", ", translationVariantsRU ?? []);
            }

            return new Headword(spanishDictDefinition.WordES, translationsEN, translationsRU);
        }

        internal async Task<string?> TranslateMeaningAsync(
            string? translatorApiURL,
            string sourceLanguage,
            string meaning,
            IEnumerable<string> examples)
        {
            TranslationOutput translationOutput = await GetTranslationAsync(
                translatorApiURL,
                sourceLanguage: sourceLanguage,
                word: "",
                meaning: meaning,
                partOfSpeech: "",
                examples: examples);

            string? translationsRU = null;
            if (translationOutput != null)
            {
                IEnumerable<string>? translationVariantsRU = translationOutput.Translations.FirstOrDefault(x => x.Language == LanguageRU)?.TranslationVariants;
                translationsRU = string.Join(", ", translationVariantsRU ?? []);
            }

            return translationsRU;
        }

        internal async Task<TranslationOutput> GetTranslationAsync(
            string? translatorApiURL,
            string sourceLanguage,
            string word,
            string meaning,
            string partOfSpeech,
            IEnumerable<string> examples)
        {
            TranslationOutput? translationOutput = null;

            if (!string.IsNullOrEmpty(translatorApiURL))
            {
                var translationInput = new TranslationInput(sourceLanguage, DestinationLanguages, word, meaning, partOfSpeech, examples);

                translationOutput = await _translatorAPIClient.TranslateAsync(translatorApiURL, translationInput);
            }

            return translationOutput ?? new TranslationOutput([]);
        }

        #endregion
    }
}
