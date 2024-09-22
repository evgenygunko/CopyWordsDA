using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Parsers.Models;
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
        private const string LanguageRU = "ru";
        private const string LanguageEN = "en";
        private readonly string[] DestinationLanguages = [LanguageRU, LanguageEN];

        private readonly IDDOPageParser _ddoPageParser;
        private readonly ISpanishDictPageParser _spanishDictPageParser;
        private readonly IFileDownloader _fileDownloader;
        private readonly ITranslatorAPIClient _translatorAPIClient;

        private readonly Regex lookupRegex = new Regex(@"^[\w ]+$");

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
                isValid = lookupRegex.IsMatch(lookUp);

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

            List<Models.DDO.DDODefinition> ddoDefinitions = _ddoPageParser.ParseDefinitions();

            // If TranslatorAPI URL is configured, call translator app and add returned translations to word model.
            IEnumerable<TranslationOutput>? translations = await GetTranslationAsync(options?.TranslatorApiURL, headWordDA, ddoDefinitions.Select(x => x.Meaning));
            Headword headword = new Headword(
                headWordDA,
                translations.FirstOrDefault(x => x.Language == LanguageEN)?.HeadWord,
                translations.FirstOrDefault(x => x.Language == LanguageRU)?.HeadWord);

            // For DDO, we create one Definition with one Context and several Meanings.
            List<Meaning> meanings = new List<Meaning>();
            int pos = 0;
            foreach (var ddoDefinition in ddoDefinitions)
            {
                meanings.Add(new Meaning(ddoDefinition.Meaning, AlphabeticalPosition: (pos++).ToString(), ddoDefinition.Tag, ImageUrl: null, Examples: ddoDefinition.Examples));
            }

            Context context = new Context(ContextEN: "", Position: 1, meanings);
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
            string? sound = _spanishDictPageParser.ParseSound(wordObj);

            string? soundUrl = null;
            string? soundFileName = null;
            if (!string.IsNullOrEmpty(sound))
            {
                soundUrl = _spanishDictPageParser.CreateSoundURL(sound);
                soundFileName = $"{headwordES}.mp4";
            }

            // SpanishDict can return several definitions (e.g. for a "transitive verb" and "reflexive verb").
            IEnumerable<Models.SpanishDict.SpanishDictDefinition> spanishDictDefinitions = _spanishDictPageParser.ParseDefinitions(wordObj);
            if (spanishDictDefinitions == null)
            {
                return null;
            }

            List<Definition> definitions = new();
            foreach (var spanishDictDefinition in spanishDictDefinitions)
            {
                List<Context> contexts = new();
                foreach (var spanishDictContext in spanishDictDefinition.Contexts)
                {
                    contexts.Add(new Context(spanishDictContext.ContextEN, spanishDictContext.Position, spanishDictContext.Meanings));

                    // If TranslatorAPI URL is configured, call translator app and add returned translations to word model.
                    IEnumerable<string> meaningsToTranslate = spanishDictContext.Meanings.Select(x => x.Description);
                    IEnumerable<TranslationOutput>? translations = await GetTranslationAsync(options?.TranslatorApiURL, headwordES, meaningsToTranslate);

                    Headword headword = new Headword(
                        headwordES,
                        translations.FirstOrDefault(x => x.Language == LanguageEN)?.HeadWord,
                        translations.FirstOrDefault(x => x.Language == LanguageRU)?.HeadWord);

                    // Spanish words don't have endings, this property only makes sense for Danish
                    definitions.Add(new Definition(headword, PartOfSpeech: spanishDictDefinition.Type, Endings: "", contexts));
                }
            }

            var wordModel = new WordModel(
                Word: headwordES,
                SoundUrl: soundUrl,
                SoundFileName: soundFileName,
                Definitions: definitions,
                Variations: Enumerable.Empty<Variant>() // there are no word variants in SpanishDict 
            );

            return wordModel;
        }

        internal async Task<IEnumerable<TranslationOutput>> GetTranslationAsync(string? translatorApiURL, string headWord, IEnumerable<string> meanings)
        {
            IEnumerable<TranslationOutput>? translations = null;

            if (!string.IsNullOrEmpty(translatorApiURL))
            {
                var translationInput = new TranslationInput(headWord, meanings, LanguageDA, DestinationLanguages);

                translations = await _translatorAPIClient.TranslateAsync(translatorApiURL, translationInput);
            }

            if (translations is null)
            {
                translations = Enumerable.Empty<TranslationOutput>();
            }

            return translations;
        }

        #endregion
    }
}
