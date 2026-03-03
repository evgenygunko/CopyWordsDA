using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using Newtonsoft.Json;

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

        public TranslationsService(
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

            string lookupUrl = CreateLookUpWordUrl();
            string sourceLanguage = _settingsService.GetSelectedParser();
            string destinationLanguage = _settingsService.GetDestinationLanguage();
            if (string.IsNullOrWhiteSpace(destinationLanguage))
            {
                destinationLanguage = "Russian";
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: destinationLanguage,
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
            if (string.IsNullOrWhiteSpace(destinationLanguage))
            {
                destinationLanguage = "Russian";
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: destinationLanguage,
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

            if (response.StatusCode is System.Net.HttpStatusCode.BadGateway
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
                throw new WordNotFoundException(input.Text);
            }

            if (response.StatusCode is System.Net.HttpStatusCode.BadGateway
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
