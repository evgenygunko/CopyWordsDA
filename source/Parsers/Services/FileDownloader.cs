// Ignore Spelling: Downloader

using System.Net;
using System.Text;
using System.Text.Json;
using CopyWords.Parsers.Exceptions;

namespace CopyWords.Parsers.Services
{
    public interface IFileDownloader
    {
        Task<string?> DownloadPageAsync(string url, Encoding encoding, CancellationToken cancellationToken);

        Task<string?> DownloadPageAllowNotFoundAsync(string url, Encoding encoding, CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetSpanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken);

        Task<byte[]> DownloadSoundFileAsync(string url, CancellationToken cancellationToken);
    }

    public class FileDownloader : IFileDownloader
    {
        private readonly HttpClient _httpClient;

        private const string SpanishSuggestionsApiUrl = "https://suggest1.spanishdict.com/dictionary/translate_es_suggest?q=";

        public FileDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> DownloadPageAsync(string url, Encoding encoding, CancellationToken cancellationToken)
        {
            return await DownloadPageInternalAsync(url, encoding, returnContentOnNotFound: false, cancellationToken);
        }

        public async Task<string?> DownloadPageAllowNotFoundAsync(string url, Encoding encoding, CancellationToken cancellationToken)
        {
            return await DownloadPageInternalAsync(url, encoding, returnContentOnNotFound: true, cancellationToken);
        }

        public async Task<IEnumerable<string>> GetSpanishWordsSuggestionsAsync(string inputText, CancellationToken cancellationToken)
        {
            string url = SpanishSuggestionsApiUrl + Uri.EscapeDataString(inputText) + "&v=0";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ServerErrorException(
                    $"Server returned error code '{response.StatusCode}' when requesting URL '{url}'.",
                    response.StatusCode,
                    url);
            }

            try
            {
                await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using JsonDocument jsonDoc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

                if (!jsonDoc.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
                {
                    throw new ServerErrorException($"The server returned a successful status code but the response content was invalid or missing 'results'.");
                }

                return results
                    .EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToList();
            }
            catch (JsonException ex)
            {
                throw new ServerErrorException("The server returned invalid JSON content.", ex);
            }
        }

        public async Task<byte[]> DownloadSoundFileAsync(string url, CancellationToken cancellationToken)
        {
            byte[]? content = null;
            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ServerErrorException($"Server returned error code '{response.StatusCode}' when requesting URL '{url}'.");
            }

            content = await response.Content.ReadAsByteArrayAsync();

            if (content == null)
            {
                throw new ServerErrorException($"Downloaded sound file from URL '{url}' is null.");
            }

            return content;
        }

        private async Task<string?> DownloadPageInternalAsync(string url, Encoding encoding, bool returnContentOnNotFound, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode || (returnContentOnNotFound && response.StatusCode == HttpStatusCode.NotFound))
            {
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return encoding.GetString(bytes, 0, bytes.Length - 1);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new ServerErrorException(
                $"Server returned error code '{response.StatusCode}' when requesting URL '{url}'.",
                response.StatusCode,
                url);
        }
    }
}
