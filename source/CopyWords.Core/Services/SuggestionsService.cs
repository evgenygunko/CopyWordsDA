using System.Net.Http.Json;
using System.Text.Json;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.Services
{
    public interface ISuggestionsService
    {
        Task<IEnumerable<string>> GetSuggestionsAsync(string inputText, CancellationToken cancellationToken);
    }

    public class SuggestionsService : ISuggestionsService
    {
        private const string ChromeUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36";

        private readonly HttpClient _httpClient;
        private readonly IBuildConfiguration _buildConfiguration;
        private readonly IGlobalSettings _globalSettings;
        private readonly ISettingsService _settingsService;

        public SuggestionsService(
            HttpClient httpClient,
            IBuildConfiguration buildConfiguration,
            IGlobalSettings globalSettings,
            ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _buildConfiguration = buildConfiguration;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
        }

        public async Task<IEnumerable<string>> GetSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            if (ContainsCyrillicCharacters(inputText))
            {
                return await GetRussianWordsSuggestionsAsync(inputText, cancellationToken);
            }

            return _settingsService.GetSelectedParser() switch
            {
                nameof(SourceLanguage.Danish) => await GetDanishWordsSuggestionsAsync(inputText, cancellationToken),
                nameof(SourceLanguage.Spanish) => await GetSpanishWordsSuggestionsAsync(inputText, cancellationToken),
                _ => Enumerable.Empty<string>()
            };
        }

        private async Task<IEnumerable<string>> GetDanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            string normalizedInput = inputText.Replace('/', '~').Replace(',', '_');
            string url = "https://ordnet.dk/api/ac/ddo/" + Uri.EscapeDataString(normalizedInput);

            try
            {
                using var request = CreateJsonGetRequest(url);
                request.Headers.TryAddWithoutValidation("api-key", _globalSettings.OrdnetApiKey);
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                    if (jsonDoc.ValueKind == JsonValueKind.Object
                        && jsonDoc.TryGetProperty("ddo", out var results)
                        && results.ValueKind == JsonValueKind.Array)
                    {
                        var suggestions = new List<string>();
                        var uniqueSuggestions = new HashSet<string>(StringComparer.Ordinal);

                        foreach (var item in results.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object
                                && item.TryGetProperty("iddel", out var iddel)
                                && iddel.ValueKind == JsonValueKind.Object
                                && iddel.TryGetProperty("lemma", out var lemma)
                                && lemma.ValueKind == JsonValueKind.String)
                            {
                                string? suggestion = lemma.GetString();
                                if (!string.IsNullOrWhiteSpace(suggestion)
                                    && uniqueSuggestions.Add(suggestion))
                                {
                                    suggestions.Add(suggestion);
                                    if (suggestions.Count == 20)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        return suggestions;
                    }

                    if (_buildConfiguration.IsDebug)
                    {
                        throw new ServerErrorException("The server returned a successful status code but the response content was invalid or missing 'ddo'.");
                    }
                }

                //if (_buildConfiguration.IsDebug)
                //{
                //    throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
                //}
            }
            catch (TaskCanceledException)
            {
            }

            return Enumerable.Empty<string>();
        }

        private async Task<IEnumerable<string>> GetSpanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            string url = "https://suggest1.spanishdict.com/dictionary/translate_es_suggest?q=" + Uri.EscapeDataString(inputText) + "&v=0";

            try
            {
                using var request = CreateJsonGetRequest(url);
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

        private async Task<IEnumerable<string>> GetRussianWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            string url = "https://ru.wiktionary.org/w/rest.php/v1/search/title?q="
                + Uri.EscapeDataString(inputText)
                + "&limit=20";

            try
            {
                using var request = CreateJsonGetRequest(url);
                using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                    if (jsonDoc.TryGetProperty("pages", out var results)
                        && results.ValueKind == JsonValueKind.Array)
                    {
                        var suggestions = new List<string>();
                        foreach (var item in results.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object
                                && item.TryGetProperty("title", out var title)
                                && title.ValueKind == JsonValueKind.String)
                            {
                                suggestions.Add(title.GetString()!);
                            }
                        }

                        return suggestions;
                    }

                    if (_buildConfiguration.IsDebug)
                    {
                        throw new ServerErrorException("The server returned a successful status code but the response content was invalid or missing 'pages'.");
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

        private static HttpRequestMessage CreateJsonGetRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("User-Agent", ChromeUserAgent);
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            return request;
        }

        private static bool ContainsCyrillicCharacters(string inputText)
        {
            return inputText.Any(ch => ch is >= '\u0400' and <= '\u04FF');
        }
    }
}
