using System.Net.Http.Json;
using System.Text;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Models.Translations.Input;
using CopyWords.Parsers.Models.Translations.Output;
using Newtonsoft.Json;

namespace CopyWords.Parsers.Services
{
    public interface ITranslatorAPIClient
    {
        Task<TranslationOutput?> TranslateAsync(string url, TranslationInput input);
    }

    public class TranslatorAPIClient : ITranslatorAPIClient
    {
        private readonly HttpClient _httpClient;

        public TranslatorAPIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TranslationOutput?> TranslateAsync(string url, TranslationInput input)
        {
            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // We are calling function app and it might take time to start it
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using HttpResponseMessage response = await _httpClient.PostAsync(url, content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TranslationOutput>();
            }

            throw new ServerErrorException($"Server returned error code '{response.StatusCode}' when requesting URL '{url}'.");
        }
    }
}
