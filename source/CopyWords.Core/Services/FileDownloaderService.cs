// Ignore Spelling: Downloader

using System.Diagnostics;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface IFileDownloaderService
    {
        Task<bool> DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken);

        Task<Stream> DownloadSoundFileAsync(string soundUrl, string word, CancellationToken cancellationToken);
    }

    public class FileDownloaderService : IFileDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly IDialogService _dialogService;
        private readonly IFileIOService _fileIOService;
        private readonly IGlobalSettings _globalSettings;

        public FileDownloaderService(
            HttpClient httpClient,
            IDialogService dialogService,
            IFileIOService fileIOService,
            IGlobalSettings globalSettings)
        {
            _httpClient = httpClient;
            _dialogService = dialogService;
            _fileIOService = fileIOService;
            _globalSettings = globalSettings;
        }

        public async Task<bool> DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken)
        {
            Uri? fileUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out fileUri))
            {
                await _dialogService.DisplayAlertAsync("Cannot download file", $"URL for file '{url}' is invalid.", "OK");
                return false;
            }

            Debug.WriteLine($"Will save '{fileUri}' as '{filePath}'");

            using (var result = await _httpClient.GetAsync(fileUri, cancellationToken))
            {
                if (result.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
                    await _fileIOService.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);
                }
            }

            if (!_fileIOService.FileExists(filePath))
            {
                await _dialogService.DisplayAlertAsync("Cannot download file", $"Cannot find the file '{filePath}'. It may not have been downloaded.", "OK");
                return false;
            }

            return true;
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
