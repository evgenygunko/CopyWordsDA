using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Services;

namespace CopyWords.Parsers
{
    public interface ILookUpWord
    {
        (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp);

        Task<WordModel?> LookUpWordAsync(string wordToLookUp);
    }

    public class LookUpWord : ILookUpWord
    {
        private const string DDOBaseUrl = "http://ordnet.dk/";
        private const string SlovarDKUrl = "http://www.slovar.dk/";

        private readonly IDDOPageParser _ddoPageParser;
        private readonly IFileDownloader _fileDownloader;

        private readonly Regex lookupRegex = new Regex(@"^[\w ]+$");

        public LookUpWord(
            IDDOPageParser ddoPageParser,
            IFileDownloader fileDownloader)
        {
            _ddoPageParser = ddoPageParser;
            _fileDownloader = fileDownloader;
        }

        #region Public Methods

        public (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp)
        {
            bool isValid = lookupRegex.IsMatch(lookUp);

            return (isValid, isValid ? null : "Search can only contain alphanumeric characters and spaces.");
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("LookUp text cannot be null or empty.");
            }

            (bool isValid, string? errorMessage) = CheckThatWordIsValid(wordToLookUp);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(wordToLookUp));
            }

            string url = DDOBaseUrl + $"ordbog?query={wordToLookUp}&search=S%C3%B8g";

            WordModel? wordModel = await DownloadPageAndParseWordAsync(url);
            return wordModel;
        }

        #endregion

        #region Internal Methods

        internal static string GetSlovardkUri(string wordToLookUp)
        {
            wordToLookUp = wordToLookUp.ToLower()
                .Replace("å", "'aa")
                .Replace("æ", "'ae")
                .Replace("ø", "'oe")
                .Replace(" ", "-");

            return SlovarDKUrl + $"tdansk/{wordToLookUp}/?";
        }

        internal async Task<WordModel?> DownloadPageAndParseWordAsync(string url)
        {
            // Download and parse a page from DDO
            string? ddoPageHtml = await _fileDownloader.DownloadPageAsync(url, Encoding.UTF8);
            if (string.IsNullOrEmpty(ddoPageHtml))
            {
                return null;
            }

            _ddoPageParser.LoadHtml(ddoPageHtml);

            WordModel wordModel = new WordModel(
                VariationUrls: _ddoPageParser.ParseVariationUrls(),
                Word: _ddoPageParser.ParseWord(),
                Endings: _ddoPageParser.ParseEndings(),
                Pronunciation: _ddoPageParser.ParsePronunciation(),
                Sound: _ddoPageParser.ParseSound(),
                Definitions: _ddoPageParser.ParseDefinitions(),
                Examples: _ddoPageParser.ParseExamples()
            );

            return wordModel;
        }

        #endregion
    }
}
