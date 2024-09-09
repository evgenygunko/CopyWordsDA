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
            string headWord = _ddoPageParser.ParseHeadword();

            string soundUrl = _ddoPageParser.ParseSound();
            string soundFileName = string.IsNullOrEmpty(soundUrl) ? string.Empty : $"{headWord}.mp3";

            List<Definition> definitions = _ddoPageParser.ParseDefinitions();

            // If TranslatorAPI URL is configured, call translator app and add returned translation to word model.
            string? translation = await GetTranslationAsync(options?.TranslatorApiURL, headWord, definitions);

            var wordModel = new WordModel(
                Headword: headWord,
                PartOfSpeech: _ddoPageParser.ParsePartOfSpeech(),
                Endings: _ddoPageParser.ParseEndings(),
                SoundUrl: soundUrl,
                SoundFileName: soundFileName,
                Translation: translation,
                Definitions: definitions,
                Variations: _ddoPageParser.ParseVariants()
            );

            return wordModel;
        }

        #endregion

        #region Internal Methods

        internal async Task<string?> GetTranslationAsync(string? translatorApiURL, string headWord, IEnumerable<Definition> definitions)
        {
            string? translation = null;

            if (!string.IsNullOrEmpty(translatorApiURL))
            {
                var translationInput = new TranslationInput(headWord, definitions.Select(x => x.Meaning), LanguageDA, DestinationLanguages);

                IEnumerable<TranslationOutput>? translations = await _translatorAPIClient.TranslateAsync(translatorApiURL, translationInput);
                translation = translations?.FirstOrDefault(x => x.Language == LanguageRU)?.HeadWord;
            }

            return translation;
        }

        #endregion
    }
}
