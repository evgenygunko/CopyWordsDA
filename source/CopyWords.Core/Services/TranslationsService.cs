using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using Newtonsoft.Json;

namespace CopyWords.Core.Services
{
    public interface ITranslationsService
    {
        Task<WordModel?> LookUpWordAsync(string wordToLookUp, string sourceLanguage, CancellationToken cancellationToken);
    }

    public class TranslationsService : ITranslationsService
    {
        private readonly HttpClient _httpClient;
        private readonly IGlobalSettings _globalSettings;

        public TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, string sourceLanguage, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            if (string.IsNullOrEmpty(_globalSettings.TranslatorAppUrl))
            {
                throw new ArgumentException("TranslatorApp URL cannot be null or empty");
            }

            // Construct full URL by appending the API path and code
            string lookupUrl = $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/LookUpWord?code={_globalSettings.TranslatorAppRequestCode}";

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: "Russian",
                Version: "2");

            WordModel? wordModel = await TranslateAsync(lookupUrl, input, cancellationToken);
            return wordModel;
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
                return null;
            }

            throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
        }
    }
}
