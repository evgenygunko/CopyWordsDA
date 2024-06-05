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
        private const string DDOBaseUrl = "http://ordnet.dk/ddo/ordbog";

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

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp)
        {
            (bool isValid, string? errorMessage) = CheckThatWordIsValid(wordToLookUp);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage, nameof(wordToLookUp));
            }

            string url = DDOBaseUrl + $"?query={wordToLookUp}&search=S%C3%B8g";

            // Download and parse a page from DDO
            string? ddoPageHtml = await _fileDownloader.DownloadPageAsync(url, Encoding.UTF8);
            if (string.IsNullOrEmpty(ddoPageHtml))
            {
                return null;
            }

            _ddoPageParser.LoadHtml(ddoPageHtml);

            List<Definition> definitions = _ddoPageParser.ParseDefinitions();

            var wordModel = new WordModel(
                Headword: _ddoPageParser.ParseHeadword(),
                SoundUrl: _ddoPageParser.ParseSound(),
                SoundFileName: null,
                Definitions: definitions
            );

            return wordModel;
        }

        #endregion
    }
}
