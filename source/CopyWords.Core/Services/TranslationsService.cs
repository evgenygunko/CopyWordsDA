using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using Newtonsoft.Json;

namespace CopyWords.Core.Services
{
    public interface ITranslationsService
    {
        Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options options, CancellationToken cancellationToken);
    }

    public class TranslationsService : ITranslationsService
    {
        private readonly HttpClient _httpClient;

        public TranslationsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options options, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Options cannot be null.");
            }

            if (string.IsNullOrEmpty(options.TranslatorApiURL))
            {
                throw new ArgumentException("Translator API URL cannot be null or empty");
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: options.SourceLang.ToString(),
                DestinationLanguage: "Russian",
                Version: "1");

            WordModel? wordModel = await TranslateAsync(options.TranslatorApiURL, input, cancellationToken);
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
