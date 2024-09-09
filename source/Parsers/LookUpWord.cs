using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Services;

namespace CopyWords.Parsers
{
    public interface ILookUpWord
    {
        (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp);

        Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options? options = null);

        Task<WordModel?> GetWordByUrlAsync(string url, Options? options = null);
    }

    public class LookUpWord : ILookUpWord
    {
        private const string DDOBaseUrl = "https://ordnet.dk/ddo/ordbog";
        private const string LanguageDA = "da";
        private const string LanguageRU = "ru";
        private const string LanguageEN = "en";
        private readonly string[] DestinationLanguages = [LanguageRU, LanguageEN];

        private readonly IDDOPageParser _ddoPageParser;
        private readonly IFileDownloader _fileDownloader;
        private readonly ITranslatorAPIClient _translatorAPIClient;

        private readonly Regex lookupRegex = new Regex(@"^[\w ]+$");

        public LookUpWord(
            IDDOPageParser ddoPageParser,
            IFileDownloader fileDownloader,
            ITranslatorAPIClient translatorAPIClient)
        {
            _ddoPageParser = ddoPageParser;
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

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options? options = null)
        {
            (bool isValid, string? errorMessage) = CheckThatWordIsValid(wordToLookUp);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(wordToLookUp));
            }

            string url = DDOBaseUrl + $"?query={wordToLookUp}&search=S%C3%B8g";

            var wordModel = await GetWordByUrlAsync(url, options);
            return wordModel;
        }

        public async Task<WordModel?> GetWordByUrlAsync(string url, Options? options = null)
        {
            // Download and parse a page from DDO
            string? ddoPageHtml = await _fileDownloader.DownloadPageAsync(url, Encoding.UTF8);
            if (string.IsNullOrEmpty(ddoPageHtml))
            {
                return null;
            }

            _ddoPageParser.LoadHtml(ddoPageHtml);
            string headWordDA = _ddoPageParser.ParseHeadword();

            string soundUrl = _ddoPageParser.ParseSound();
            string soundFileName = string.IsNullOrEmpty(soundUrl) ? string.Empty : $"{headWordDA}.mp3";

            List<Definition> definitions = _ddoPageParser.ParseDefinitions();

            // If TranslatorAPI URL is configured, call translator app and add returned translations to word model.
            IEnumerable<TranslationOutput>? translations = await GetTranslationAsync(options?.TranslatorApiURL, headWordDA, definitions);
            Headword headword = new Headword(
                headWordDA,
                translations.FirstOrDefault(x => x.Language == LanguageEN)?.HeadWord,
                translations.FirstOrDefault(x => x.Language == LanguageRU)?.HeadWord);

            var wordModel = new WordModel(
                Headword: headword,
                PartOfSpeech: _ddoPageParser.ParsePartOfSpeech(),
                Endings: _ddoPageParser.ParseEndings(),
                SoundUrl: soundUrl,
                SoundFileName: soundFileName,
                Definitions: definitions,
                Variations: _ddoPageParser.ParseVariants()
            );

            return wordModel;
        }

        #endregion

        #region Internal Methods

        internal async Task<IEnumerable<TranslationOutput>> GetTranslationAsync(string? translatorApiURL, string headWord, IEnumerable<Definition> definitions)
        {
            IEnumerable<TranslationOutput>? translations = null;

            if (!string.IsNullOrEmpty(translatorApiURL))
            {
                var translationInput = new TranslationInput(headWord, definitions.Select(x => x.Meaning), LanguageDA, DestinationLanguages);

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
