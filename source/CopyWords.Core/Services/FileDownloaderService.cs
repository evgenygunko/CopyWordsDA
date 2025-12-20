// Ignore Spelling: Downloader

using CopyWords.Core.Exceptions;

namespace CopyWords.Core.Services
{
    public interface IFileDownloaderService
    {
        Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken);
    }

    public class FileDownloaderService : IFileDownloaderService
    {
        private readonly HttpClient _httpClient;

        public FileDownloaderService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
            }

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
    }
}
