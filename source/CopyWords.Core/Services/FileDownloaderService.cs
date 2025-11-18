// Ignore Spelling: Downloader

using System.Diagnostics;
using System.Runtime.Versioning;
using CommunityToolkit.Maui.Storage;

namespace CopyWords.Core.Services
{
    public interface IFileDownloaderService
    {
        Task<bool> DownloadFileAsync(string url, string filePath);

        Task<bool> CopyFileToAnkiFolderAsync(string sourceFile);

        Task<bool> SaveFileWithFileSaverAsync(string url, string soundFileName, CancellationToken cancellationToken);
    }

    public class FileDownloaderService : IFileDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly IDialogService _dialogService;
        private readonly IFileIOService _fileIOService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSaver _fileSaver;

        public FileDownloaderService(
            HttpClient httpClient,
            IDialogService dialogService,
            IFileIOService fileIOService,
            ISettingsService settingsService,
            IFileSaver fileSaver)
        {
            _httpClient = httpClient;
            _dialogService = dialogService;
            _fileIOService = fileIOService;
            _settingsService = settingsService;
            _fileSaver = fileSaver;
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

        /// <summary>
        /// Downloads a file from the specified URL and saves it using the file saver dialog.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="soundFileName">The suggested name for the file to be saved. Examples: "billede.mp3", "tiburón.mp4"</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file was saved successfully.</returns>
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [SupportedOSPlatform("android")]
        public async Task<bool> SaveFileWithFileSaverAsync(string url, string soundFileName, CancellationToken cancellationToken)
        {
            using var ctsHttpRequest = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ctsHttpRequest.Token, cancellationToken);

            await using var stream = await _httpClient.GetStreamAsync(url, ctsHttpRequest.Token);

            var fileSaverResult = await _fileSaver.SaveAsync(soundFileName, stream, cancellationToken);
            return fileSaverResult.IsSuccessful;
        }
    }
}
