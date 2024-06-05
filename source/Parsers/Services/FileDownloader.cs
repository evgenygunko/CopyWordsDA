using System.Text;
using CopyWords.Parsers.Exceptions;

namespace CopyWords.Parsers.Services
{
    public interface IFileDownloader
    {
        Task<string?> DownloadPageAsync(string url, Encoding encoding);
    }

    public class FileDownloader : IFileDownloader
    {
        private readonly HttpClient _httpClient;

        public FileDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
        }

        public async Task<string?> DownloadPageAsync(string url, Encoding encoding)
        {
            string? content = null;
            HttpResponseMessage response;

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                response = await _httpClient.GetAsync(url, cts.Token);
            }

            if (response.IsSuccessStatusCode)
            {
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                content = encoding.GetString(bytes, 0, bytes.Length - 1);
            }
            else
            {
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw new ServerErrorException($"Server returned error code '{response.StatusCode}' when requesting URL '{url}'.");
                }
            }

            return content;
        }
    }
}
