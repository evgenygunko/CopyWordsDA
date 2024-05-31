using System.Diagnostics;

namespace CopyWords.Core.Services
{
    public abstract class SaveFileServiceBase
    {
        protected readonly ISettingsService _settingsService;
        protected readonly HttpClient _httpClient;
        protected readonly IDialogService _dialogService;

        protected SaveFileServiceBase(
            ISettingsService settingsService,
            HttpClient httpClient,
            IDialogService dialogService)
        {
            _settingsService = settingsService;
            _httpClient = httpClient;
            _dialogService = dialogService;
        }

        protected async Task<string> DownloadFileAsync(string url, string fileName)
        {
            Uri fileUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out fileUri))
            {
                await _dialogService.DisplayAlert("Cannot download file", $"URL for file '{url}' is invalid.", "OK");
                return null;
            }

            string destFileFullPath = Path.Combine(Path.GetTempPath(), fileName);
            Debug.WriteLine($"Will save '{fileUri}' as '{destFileFullPath}'");

            using (var result = await _httpClient.GetAsync(fileUri))
            {
                if (result.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await result.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(destFileFullPath, fileBytes);
                }
            }

            if (!File.Exists(destFileFullPath))
            {
                await _dialogService.DisplayAlert("Cannot download file", $"Cannot find file in a temp folder '{destFileFullPath}'. It probably hasn't been downloaded.", "OK");
                return null;
            }

            return destFileFullPath;
        }

        protected async Task<bool> CopyFileToAnkiFolderAsync(string sourceFile)
        {
            Debug.Assert(File.Exists(sourceFile));

            string ankiSoundsFolder = _settingsService.GetAnkiSoundsFolder();
            if (!Directory.Exists(ankiSoundsFolder))
            {
                await _dialogService.DisplayAlert("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{ankiSoundsFolder}'. Please update it in Settings.", "OK");
                return false;
            }

            string destinationFile = Path.Combine(ankiSoundsFolder, Path.GetFileName(sourceFile));

            if (File.Exists(destinationFile))
            {
                bool answer = await _dialogService.DisplayAlert("File already exists", $"File '{Path.GetFileName(sourceFile)}' already exists. Overwrite?", "Yes", "No");
                if (!answer)
                {
                    return false;
                }
            }

            File.Copy(sourceFile, destinationFile, true);

            return true;
        }
    }
}
