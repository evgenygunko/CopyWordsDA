// Ignore Spelling: Downloader

using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface ISaveImageFileService
    {
        Task<Stream> DownloadAndResizeImageAsync(string imageUrl, CancellationToken cancellationToken);

        Task<bool> SaveImagesAsync(IEnumerable<ImageFile> imageFiles, CancellationToken cancellationToken);
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

        public async Task<Stream> DownloadAndResizeImageAsync(string imageUrl, CancellationToken cancellationToken)
        {
            using Stream stream = await _fileDownloaderService.DownloadFileAsync(imageUrl, cancellationToken);
            using SixLabors.ImageSharp.Image image = await _imageSharpWrapper.ResizeImageAsync(stream, cancellationToken);

            return await _imageSharpWrapper.SaveAsJpegAsync(image, cancellationToken);
        }

        public async Task<bool> SaveImagesAsync(IEnumerable<ImageFile> imageFiles, CancellationToken cancellationToken)
        {
            string ankiSoundsFolder = _settingsService.LoadSettings().AnkiSoundsFolder;
            if (!_fileIOService.DirectoryExists(ankiSoundsFolder))
            {
                await _dialogService.DisplayAlertAsync("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{ankiSoundsFolder}'. Please update it in Settings.", "OK");
                return false;
            }

            foreach (var imageFile in imageFiles)
            {
                string destinationFile = Path.Combine(ankiSoundsFolder, imageFile.FileName);

                try
                {
                    using Stream stream = await _fileDownloaderService.DownloadFileAsync(imageFile.ImageUrl, cancellationToken);

                    if (_fileIOService.FileExists(destinationFile))
                    {
                        bool answer = await _dialogService.DisplayAlertAsync("File already exists", $"File '{destinationFile}' already exists. Overwrite?", "Yes", "No");
                        if (!answer)
                        {
                            // User doesn't want to overwrite the file, so we skip this one and continue with the next.
                            continue;
                        }
                    }

                    // Resize to 150x150 px and save as JPEG
                    cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;
                    using SixLabors.ImageSharp.Image image = await _imageSharpWrapper.ResizeImageAsync(stream, cancellationToken);
                    await _imageSharpWrapper.SaveAsJpegAsync(image, destinationFile, cancellationToken);
                }
                catch (ServerErrorException ex)
                {
                    await _dialogService.DisplayAlertAsync($"Cannot download image", $"Cannot download image file from '{imageFile.ImageUrl}'. Error: {ex.Message}", "OK");
                    return false;
                }
            }

            return true;
        }
    }
}
