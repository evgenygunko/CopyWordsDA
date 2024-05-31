// Ignore Spelling: Downloader

using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Services;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CopyWords.Parsers
{
    public interface ILookUpWord
    {
        (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp);

        Task<WordModel?> LookUpWordAsync(string wordToLookUp);
    }

    public class LookUpWord : ILookUpWord
    {
        public const string SoundBaseUrl = "https://d10gt6izjc94x0.cloudfront.net/desktop/";
        public const string ImageBaseUrl = "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/";
        private const string SpanishDictBaseUrl = "https://www.spanishdict.com/translate/";

        private readonly ISpanishDictPageParser _spanishDictPageParser;
        private readonly IFileDownloader _fileDownloader;

        public LookUpWord()
            : this(new SpanishDictPageParser(), new FileDownloader())
        {
        }

        public LookUpWord(ISpanishDictPageParser spanishDictPageParser, IFileDownloader fileDownloader)
        {
            _spanishDictPageParser = spanishDictPageParser;
            _fileDownloader = fileDownloader;
        }

        #region Public Methods

        public (bool isValid, string? errorMessage) CheckThatWordIsValid(string lookUp)
        {
            var regex = new Regex(@"^[\w ]+$");
            bool isValid = regex.IsMatch(lookUp);

            if (isValid)
            {
                return (true, null);
            }

            return (false, "Search can only contain alphanumeric characters and spaces.");
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
                throw new ArgumentException(errorMessage);
            }

            string url = SpanishDictBaseUrl + wordToLookUp;

            string? downloadedPageHtml = await _fileDownloader.DownloadPageAsync(url, Encoding.UTF8);
            if (string.IsNullOrEmpty(downloadedPageHtml))
            {
                return null;
            }

            Models.SpanishDict.WordJsonModel? wordObj = ParseWordJson(downloadedPageHtml);
            if (wordObj == null)
            {
                return null;
            }

            return ParseWord(wordObj);
        }

        #endregion

        #region Internal Methods

        internal Models.SpanishDict.WordJsonModel? ParseWordJson(string htmlPage)
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

        private WordModel? ParseWord(Models.SpanishDict.WordJsonModel wordObj)
        {
            string word = _spanishDictPageParser.ParseWord(wordObj);
            string? sound = _spanishDictPageParser.ParseSound(wordObj);

            string? soundUrl = null;
            string? soundFileName = null;
            if (!string.IsNullOrEmpty(sound))
            {
                soundUrl = $"{SoundBaseUrl}lang_es_pron_{sound}_speaker_7_syllable_all_version_50.mp4";
                soundFileName = $"{word}.mp4";
            }

            IEnumerable<WordVariant> translations = _spanishDictPageParser.ParseTranslations(wordObj);
            if (translations == null)
            {
                return null;
            }

            var wordModel = new WordModel(word, soundUrl, soundFileName, translations);
            return wordModel;
        }

        #endregion
    }
}
