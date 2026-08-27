using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Parsers;
using CopyWords.Parsers.Models;
using Newtonsoft.Json;
using ParserServerErrorException = CopyWords.Parsers.Exceptions.ServerErrorException;

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
        private readonly ILookUpWord _lookUpWord;

        public TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings,
            ISettingsService settingsService,
            ILookUpWord lookUpWord)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
            _lookUpWord = lookUpWord;
        }

        public string CreateLookUpWordUrl()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v3/Translation/LookUpWord?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public string CreateSuggestedWordsUrl()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v3/Translation/SuggestedWords?code={_globalSettings.TranslatorAppRequestCode}";
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
            WordModel? parsedWordModel;
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

            return await TranslateAsync(CreateLookUpWordUrl(), parsedWordModel, cancellationToken);
        }

        public async Task<SuggestedWordsModel> GetSuggestedWordsAsync(string wordToLookUp, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            string sourceLanguage = _settingsService.GetSelectedParser();
            if (CheckLanguageSpecificCharacters(wordToLookUp) is (true, string detectedLanguage))
            {
                if (string.Equals(detectedLanguage, "Russian", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetAISuggestedWordsAsync(wordToLookUp, sourceLanguage, cancellationToken);
                }

                sourceLanguage = detectedLanguage;
            }

            try
            {
                IEnumerable<string> suggestions = await _lookUpWord.GetSuggestedWordsAsync(
                    wordToLookUp,
                    sourceLanguage,
                    cancellationToken);
                return new SuggestedWordsModel(suggestions);
            }
            catch (ParserServerErrorException ex)
            {
                throw new ServerErrorException(ex.Message, ex);
            }
        }

        private async Task<SuggestedWordsModel> GetAISuggestedWordsAsync(
            string wordToLookUp,
            string destinationLanguage,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_globalSettings.TranslatorAppUrl))
            {
                throw new ArgumentException("TranslatorApp URL cannot be null or empty");
            }

            string suggestedWordsUrl = CreateSuggestedWordsUrl();
            var input = new SuggestionsRequest(wordToLookUp, destinationLanguage);

            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // The Translator app may use OpenAI API for request processing, so it can take time to return result.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            using HttpResponseMessage response = await _httpClient.PostAsync(suggestedWordsUrl, content, combinedCts.Token);

            if (response.IsSuccessStatusCode)
            {
                var suggestedWords = await response.Content.ReadFromJsonAsync<SuggestedWordsModel>(combinedCts.Token);
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

        private static (bool hasLanguageSpecificCharacters, string language) CheckLanguageSpecificCharacters(string text)
        {
            var danishCharacters = new HashSet<char> { 'æ', 'ø', 'å', 'Æ', 'Ø', 'Å' };
            if (text.Any(danishCharacters.Contains))
            {
                return (true, SourceLanguage.Danish.ToString());
            }

            var spanishCharacters = new HashSet<char> { 'ñ', 'Ñ', 'í', 'Í', 'á', 'Á', 'é', 'É', 'ó', 'Ó', 'ú', 'Ú', 'ü', 'Ü' };
            if (text.Any(spanishCharacters.Contains))
            {
                return (true, SourceLanguage.Spanish.ToString());
            }

            if (text.Any(character => character is >= '\u0400' and <= '\u04FF'))
            {
                return (true, "Russian");
            }

            return (false, string.Empty);
        }

        private async Task<WordModel?> TranslateAsync(string url, WordModel input, CancellationToken cancellationToken)
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
                throw new WordNotFoundException(input.Word);
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
    }
}
