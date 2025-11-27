// Ignore Spelling: Downloader

using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface IFileDownloaderService
    {
        Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken);

        Task<Stream> DownloadSoundFileAsync(string soundUrl, string word, CancellationToken cancellationToken);
    }

    public class FileDownloaderService : IFileDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly IGlobalSettings _globalSettings;

        public FileDownloaderService(
            HttpClient httpClient,
            IGlobalSettings globalSettings)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
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

        public async Task<Stream> DownloadSoundFileAsync(string soundUrl, string word, CancellationToken cancellationToken)
        {
            string downloadSoundUrl = $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/DownloadSound?" +
                $"soundUrl={Uri.EscapeDataString(soundUrl)}&word={Uri.EscapeDataString(word)}&version=1&code={_globalSettings.TranslatorAppRequestCode}";

            using var combinedCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(combinedCts.Token, cancellationToken);

            HttpResponseMessage response = await _httpClient.GetAsync(downloadSoundUrl, combinedCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
            }

            return await response.Content.ReadAsStreamAsync(combinedCts.Token);
        }
    }
}
