using System.Net.Http.Json;
using System.Text.Json;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface ISuggestionsService
    {
        Task<IEnumerable<string>> GetDanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetSpanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken);
    }

    public class SuggestionsService : ISuggestionsService
    {
        private readonly HttpClient _httpClient;
        private readonly IBuildConfiguration _buildConfiguration;

        public SuggestionsService(
            HttpClient httpClient,
            IBuildConfiguration buildConfiguration)
        {
            _httpClient = httpClient;
            _buildConfiguration = buildConfiguration;
        }

        public async Task<IEnumerable<string>> GetDanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            string url = "https://ordnet.dk/ws/ddo/livesearch?text=" + Uri.EscapeDataString(inputText) + "&size=20";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");

                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var suggestions = await response.Content.ReadFromJsonAsync<string[]>(cancellationToken);
                    if (suggestions is not null)
                    {
                        return suggestions;
                    }

                    if (_buildConfiguration.IsDebug)
                    {
                        throw new ServerErrorException("The server returned a successful status code but the response content was null.");
                    }
                }

                if (_buildConfiguration.IsDebug)
                {
                    throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
                }
            }
            catch (TaskCanceledException)
            {
            }

            return Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<string>> GetSpanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            string url = "https://suggest1.spanishdict.com/dictionary/translate_es_suggest?q=" + Uri.EscapeDataString(inputText) + "&v=0";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");

                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                    if (jsonDoc.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                    {
                        var suggestions = new List<string>();
                        foreach (var item in results.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                suggestions.Add(item.GetString()!);
                            }
                        }
                        return suggestions;
                    }

                    if (_buildConfiguration.IsDebug)
                    {
                        throw new ServerErrorException("The server returned a successful status code but the response content was invalid or missing 'results'.");
                    }
                }

                if (_buildConfiguration.IsDebug)
                {
                    throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
                }
            }
            catch (TaskCanceledException)
            {
            }

            return Enumerable.Empty<string>();
        }
    }
}
