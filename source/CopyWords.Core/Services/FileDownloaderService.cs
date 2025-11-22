// Ignore Spelling: Downloader

using System.Diagnostics;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface IFileDownloaderService
    {
        Task<bool> DownloadFileAsync(string url, string filePath);

        Task<bool> CopyFileToAnkiFolderAsync(string sourceFile);

        Task<Stream> DownloadSoundFileAsync(string soundUrl, string word, CancellationToken cancellationToken);
    }

    public class FileDownloaderService : IFileDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly IDialogService _dialogService;
        private readonly IFileIOService _fileIOService;
        private readonly ISettingsService _settingsService;
        private readonly IGlobalSettings _globalSettings;

        public FileDownloaderService(
            HttpClient httpClient,
            IDialogService dialogService,
            IFileIOService fileIOService,
            ISettingsService settingsService,
            IGlobalSettings globalSettings)
        {
            _httpClient = httpClient;
            _dialogService = dialogService;
            _fileIOService = fileIOService;
            _settingsService = settingsService;
            _globalSettings = globalSettings;
        }

        public async Task<bool> DownloadFileAsync(string url, string filePath)
        {
            Uri? fileUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out fileUri))
            {
                await _dialogService.DisplayAlertAsync("Cannot download file", $"URL for file '{url}' is invalid.", "OK");
                return false;
            }

            Debug.WriteLine($"Will save '{fileUri}' as '{filePath}'");

            using (var result = await _httpClient.GetAsync(fileUri))
            {
                if (result.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await result.Content.ReadAsByteArrayAsync();
                    await _fileIOService.WriteAllBytesAsync(filePath, fileBytes);
                }
            }

            if (!_fileIOService.FileExists(filePath))
            {
                await _dialogService.DisplayAlertAsync("Cannot download file", $"Cannot find the file '{filePath}'. It may not have been downloaded.", "OK");
                return false;
            }

            return true;
        }

        public async Task<bool> CopyFileToAnkiFolderAsync(string sourceFile)
        {
            Debug.Assert(_fileIOService.FileExists(sourceFile));

            string ankiSoundsFolder = _settingsService.LoadSettings().AnkiSoundsFolder;
            if (!_fileIOService.DirectoryExists(ankiSoundsFolder))
            {
                await _dialogService.DisplayAlertAsync("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{ankiSoundsFolder}'. Please update it in Settings.", "OK");
                return false;
            }

            string destinationFile = Path.Combine(ankiSoundsFolder, Path.GetFileName(sourceFile));

            if (_fileIOService.FileExists(destinationFile))
            {
                bool answer = await _dialogService.DisplayAlertAsync("File already exists", $"File '{Path.GetFileName(sourceFile)}' already exists. Overwrite?", "Yes", "No");
                if (!answer)
                {
                    // User doesn't want to overwrite the file, so we can skip the copy. But the file already exists, so we return true.
                    return true;
                }
            }

            _fileIOService.CopyFile(sourceFile, destinationFile, true);

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
