// Ignore Spelling: Downloader

using System.Diagnostics;

namespace CopyWords.Core.Services
{
    public interface IFileDownloaderService
    {
        Task<string?> DownloadFileAsync(string url, string fileName);

        Task<bool> CopyFileToAnkiFolderAsync(string sourceFile);
    }

    public class FileDownloaderService : IFileDownloaderService
    {
        private readonly HttpClient _httpClient;
        private readonly IDialogService _dialogService;
        private readonly IFileIOService _fileIOService;
        private readonly ISettingsService _settingsService;

        public FileDownloaderService(
            HttpClient httpClient,
            IDialogService dialogService,
            IFileIOService fileIOService,
            ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _dialogService = dialogService;
            _fileIOService = fileIOService;
            _settingsService = settingsService;
        }

        public async Task<string?> DownloadFileAsync(string url, string fileName)
        {
            Uri? fileUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out fileUri))
            {
                await _dialogService.DisplayAlertAsync("Cannot download file", $"URL for file '{url}' is invalid.", "OK");
                return null;
            }

            string destFileFullPath = Path.Combine(Path.GetTempPath(), fileName);
            Debug.WriteLine($"Will save '{fileUri}' as '{destFileFullPath}'");

            using (var result = await _httpClient.GetAsync(fileUri))
            {
                if (result.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await result.Content.ReadAsByteArrayAsync();
                    await _fileIOService.WriteAllBytesAsync(destFileFullPath, fileBytes);
                }
            }

            if (!_fileIOService.FileExists(destFileFullPath))
            {
                await _dialogService.DisplayAlertAsync("Cannot download file", $"Cannot find file in a temp folder '{destFileFullPath}'. It probably hasn't been downloaded.", "OK");
                return null;
            }

            return destFileFullPath;
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
    }
}
