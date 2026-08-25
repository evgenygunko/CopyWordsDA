using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Parsers;
using Newtonsoft.Json;
using ParserServerErrorException = CopyWords.Parsers.Exceptions.ServerErrorException;
using ParserWordModel = CopyWords.Parsers.Models.WordModel;

namespace CopyWords.Core.Services
{
    public interface ITranslationsService
    {
        Task<WordModel?> LookUpWordAsync(string wordToLookUp, CancellationToken cancellationToken);

        Task<SuggestedWordsModel> GetSuggestedWordsAsync(string wordToLookUp, CancellationToken cancellationToken);
    }

    public class TranslationsService : ITranslationsService
    {
        private readonly HttpClient _httpClient;
        private readonly IGlobalSettings _globalSettings;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchDarklyService? _launchDarklyService;
        private readonly ILookUpWord? _lookUpWord;

        public TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings,
            ISettingsService settingsService,
            ILaunchDarklyService launchDarklyService,
            ILookUpWord lookUpWord)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
            _launchDarklyService = launchDarklyService;
            _lookUpWord = lookUpWord;
        }

        internal TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings,
            ISettingsService settingsService,
            ILaunchDarklyService launchDarklyService)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
            _launchDarklyService = launchDarklyService;
        }

        internal TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings,
            ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
        }

        public string CreateLookUpWordUrl()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v2/Translation/LookUpWord?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public string CreateLookUpWordV3Url()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v3/Translation/LookUpWord?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public string CreateSuggestedWordsUrl()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v2/Translation/SuggestedWords?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            if (string.IsNullOrEmpty(_globalSettings.TranslatorAppUrl))
            {
                throw new ArgumentException("TranslatorApp URL cannot be null or empty");
            }

            string sourceLanguage = _settingsService.GetSelectedParser();

            if (_launchDarklyService?.GetBooleanFlag("client-side-parsing", false) == true)
            {
                if (_lookUpWord == null)
                {
                    throw new InvalidOperationException("Client-side word parser is not configured.");
                }

                ParserWordModel? parsedWordModel;
                try
                {
                    parsedWordModel = await _lookUpWord.LookUpWordAsync(wordToLookUp, sourceLanguage, cancellationToken);
                }
                catch (ParserServerErrorException ex)
                {
                    throw new ServerErrorException(ex.Message, ex);
                }

                if (parsedWordModel == null)
                {
                    throw new WordNotFoundException(wordToLookUp);
                }

                WordModel wordModel = ConvertWordModel(parsedWordModel);

                return await TranslateAsync(CreateLookUpWordV3Url(), wordModel, cancellationToken);
            }

            string lookupUrl = CreateLookUpWordUrl();
            string destinationLanguage = _settingsService.GetDestinationLanguage();
            IReadOnlyList<string> activeDictionaries = _settingsService.GetActiveDictionaries();
            if (string.IsNullOrWhiteSpace(destinationLanguage))
            {
                destinationLanguage = "Russian";
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: destinationLanguage,
                ActiveDictionaries: activeDictionaries,
                Version: "2");

            return await TranslateAsync(lookupUrl, input, cancellationToken);
        }

        public async Task<SuggestedWordsModel> GetSuggestedWordsAsync(string wordToLookUp, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            if (string.IsNullOrEmpty(_globalSettings.TranslatorAppUrl))
            {
                throw new ArgumentException("TranslatorApp URL cannot be null or empty");
            }

            string suggestedWordsUrl = CreateSuggestedWordsUrl();
            string sourceLanguage = _settingsService.GetSelectedParser();
            string destinationLanguage = _settingsService.GetDestinationLanguage();
            IReadOnlyList<string> activeDictionaries = _settingsService.GetActiveDictionaries();
            if (string.IsNullOrWhiteSpace(destinationLanguage))
            {
                destinationLanguage = "Russian";
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: destinationLanguage,
                ActiveDictionaries: activeDictionaries,
                Version: "2");

            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // The Translator app may use OpenAI API for request processing, so it can take time to return result.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            using HttpResponseMessage response = await _httpClient.PostAsync(suggestedWordsUrl, content, combinedCts.Token);

            if (response.IsSuccessStatusCode)
            {
                SuggestedWordsModel? suggestedWords = await response.Content.ReadFromJsonAsync<SuggestedWordsModel>(combinedCts.Token);
                return suggestedWords ?? new SuggestedWordsModel([]);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                throw new InvalidInputException(errorContent);
            }

            if (response.StatusCode is System.Net.HttpStatusCode.InternalServerError
                or System.Net.HttpStatusCode.BadGateway
                or System.Net.HttpStatusCode.ServiceUnavailable
                or System.Net.HttpStatusCode.GatewayTimeout)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    throw new ServerErrorException(errorContent);
                }
            }

            throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
        }

        internal async Task<WordModel?> TranslateAsync(string url, LookUpWordRequest input, CancellationToken cancellationToken)
        {
            return await TranslateAsync(url, input, input.Text, cancellationToken);
        }

        internal async Task<WordModel?> TranslateAsync(string url, WordModel input, CancellationToken cancellationToken)
        {
            return await TranslateAsync(url, input, input.Word, cancellationToken);
        }

        private async Task<WordModel?> TranslateAsync(string url, object input, string requestedWord, CancellationToken cancellationToken)
        {
            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // The Translator app is calling OpenAI API, so it can take time to return result.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            using HttpResponseMessage response = await _httpClient.PostAsync(url, content, combinedCts.Token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WordModel>(combinedCts.Token);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                throw new InvalidInputException(errorContent);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new WordNotFoundException(requestedWord);
            }

            if (response.StatusCode is System.Net.HttpStatusCode.InternalServerError
                or System.Net.HttpStatusCode.BadGateway
                or System.Net.HttpStatusCode.ServiceUnavailable
                or System.Net.HttpStatusCode.GatewayTimeout)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    throw new ServerErrorException(errorContent);
                }
            }

            throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
        }

        private static WordModel ConvertWordModel(ParserWordModel parserWordModel)
        {
            CopyWords.Parsers.Models.Definition parserDefinition = parserWordModel.Definition;
            var definition = new Definition(
                new Headword(
                    parserDefinition.Headword.Original,
                    parserDefinition.Headword.English,
                    parserDefinition.Headword.Translation),
                parserDefinition.PartOfSpeech,
                parserDefinition.Endings,
                parserDefinition.Contexts.Select(context => new Context(
                    context.ContextEN,
                    context.Position,
                    context.Meanings.Select(meaning => new Meaning(
                        meaning.Original,
                        meaning.Translation,
                        meaning.AlphabeticalPosition,
                        meaning.Tag,
                        meaning.ImageUrl,
                        meaning.LookupUrl,
                        meaning.Examples.Select(example => new Example(example.Original, example.Translation)).ToArray())).ToArray())).ToArray());

            return new WordModel(
                parserWordModel.Word,
                Enum.Parse<SourceLanguage>(parserWordModel.SourceLanguage.ToString()),
                parserWordModel.SoundUrl,
                parserWordModel.SoundFileName,
                definition,
                parserWordModel.Variants.Select(variant => new Variant(variant.Word, variant.Url)).ToArray(),
                parserWordModel.Expressions.Select(expression => new Variant(expression.Word, expression.Url)).ToArray());
        }
    }
}
