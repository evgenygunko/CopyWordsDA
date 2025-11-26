// Ignore Spelling: Downloader

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

        public SaveImageFileService(
            IFileDownloaderService fileDownloaderService,
            ISettingsService settingsService,
            IDialogService dialogService,
            IFileIOService fileIOService)
        {
            _fileDownloaderService = fileDownloaderService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileIOService = fileIOService;
        }

        internal bool IsUnitTest { get; set; }

        #region Public Methods

        public async Task<bool> SaveImageFileAsync(string url, string fileNameWithoutExtension)
        {
            string fileExtension = Path.GetExtension(url);
            string fileName = Path.ChangeExtension(fileNameWithoutExtension, fileExtension);

            // download file from web into temp folder
            string imgFile = Path.Combine(Path.GetTempPath(), fileName);
            CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;

            bool downloadSucceeded = await _fileDownloaderService.DownloadFileAsync(url, imgFile, cancellationToken);
            if (!downloadSucceeded)
            {
                return false;
            }

            // Resize to 150x150 px and save as JPEG
            string jpgFile = await ResizeFileAsync(imgFile);

            // copy file into Anki's sounds folder
            return await CopyFileToAnkiFolderAsync(jpgFile);
        }

        #endregion

        internal async Task<bool> CopyFileToAnkiFolderAsync(string sourceFile)
        {
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

        #region Private Methods

        private async Task<string> ResizeFileAsync(string imgFile)
        {
            // This library doesn't have interfaces, so skip this method when running in test context.
            if (IsUnitTest)
            {
                return imgFile;
            }

            string jpgFile = Path.ChangeExtension(imgFile, "jpg");

            using (SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(imgFile))
            {
                if (image.Size.Width > 150 || image.Size.Height > 150)
                {
                    // We want to keep aspect ratio.
                    // From documentation: If you pass 0 as any of the values for width and height dimensions,
                    // ImageSharp will automatically determine the correct opposite dimensions size to preserve the original aspect ratio.
                    int newWidth;
                    int newHeigth;

                    if (image.Size.Height > image.Size.Width && image.Size.Height > 150)
                    {
                        newWidth = 0;
                        newHeigth = 150;
                    }
                    else
                    {
                        newWidth = 150;
                        newHeigth = 0;
                    }

                    image.Mutate(x => x.Resize(width: newWidth, height: newHeigth));
                }

                await image.SaveAsJpegAsync(imgFile);
            }

            return jpgFile;
        }

        #endregion
    }
}
