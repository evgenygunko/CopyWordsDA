// Ignore Spelling: Downloader

using CopyWords.Core.Exceptions;
using CopyWords.Core.Services.Wrappers;
using SixLabors.ImageSharp;

namespace CopyWords.Core.Services
{
    public interface ISaveImageFileService
    {
        Task<bool> SaveImageFileAsync(string url, string fileNameWithoutExtension);
    }

    public class SaveImageFileService : ISaveImageFileService
    {
        private readonly IFileDownloaderService _fileDownloaderService;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IFileIOService _fileIOService;
        private readonly IImageSharpWrapper _imageSharpWrapper;

        public SaveImageFileService(
            IFileDownloaderService fileDownloaderService,
            ISettingsService settingsService,
            IDialogService dialogService,
            IFileIOService fileIOService,
            IImageSharpWrapper imageSharpWrapper)
        {
            _fileDownloaderService = fileDownloaderService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileIOService = fileIOService;
            _imageSharpWrapper = imageSharpWrapper;
        }

        public async Task<bool> SaveImageFileAsync(string url, string fileNameWithoutExtension)
        {
            string ankiSoundsFolder = _settingsService.LoadSettings().AnkiSoundsFolder;
            if (!_fileIOService.DirectoryExists(ankiSoundsFolder))
            {
                await _dialogService.DisplayAlertAsync("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{ankiSoundsFolder}'. Please update it in Settings.", "OK");
                return false;
            }

            string destinationFile = Path.Combine(ankiSoundsFolder, $"{fileNameWithoutExtension}.jpg");

            try
            {
                CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
                using Stream stream = await _fileDownloaderService.DownloadFileAsync(url, cancellationToken);

                if (_fileIOService.FileExists(destinationFile))
                {
                    bool answer = await _dialogService.DisplayAlertAsync("File already exists", $"File '{destinationFile}' already exists. Overwrite?", "Yes", "No");
                    if (!answer)
                    {
                        // User doesn't want to overwrite the file, so we can skip the copy. But the file already exists, so we return true.
                        return true;
                    }
                }

                // Resize to 150x150 px and save as JPEG
                cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
                using SixLabors.ImageSharp.Image image = await _imageSharpWrapper.ResizeImageAsync(stream, cancellationToken);
                await _imageSharpWrapper.SaveAsJpegAsync(image, destinationFile, cancellationToken);
            }
            catch (ServerErrorException ex)
            {
                await _dialogService.DisplayAlertAsync($"Cannot download image", $"Cannot download image file from '{url}'. Error: {ex.Message}", "OK");
                return false;
            }

            return true;
        }
    }
}
