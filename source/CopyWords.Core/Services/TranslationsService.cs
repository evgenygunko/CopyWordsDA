using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Models;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models;
using Newtonsoft.Json;

namespace CopyWords.Core.Services
{
    public interface ITranslationsService
    {
        Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options options);
    }

    public class TranslationsService : ITranslationsService
    {
        private readonly HttpClient _httpClient;

        public TranslationsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, Options options)
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

            var translationInput = new TranslationInput(
                Version: "1",
                SourceLanguage: options.SourceLang.ToString(),
                DestinationLanguage: "Russian",
                Definitions: [],
                Text: wordToLookUp);

            WordModel? wordModel = await TranslateAsync(options.TranslatorApiURL, translationInput);

            return wordModel;
        }

        internal async Task<WordModel?> TranslateAsync(string url, TranslationInput input)
        {
            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // The Translation app is calling OpenAI API, so it can take time to return result.
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using HttpResponseMessage response = await _httpClient.PostAsync(url, content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WordModel>();
            }

            throw new ServerErrorException($"Server returned error code '{response.StatusCode}' when requesting URL '{url}'.");
        }
    }
}
