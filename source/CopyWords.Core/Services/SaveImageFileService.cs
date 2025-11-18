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

        public SaveImageFileService(
            IFileDownloaderService fileDownloaderService)
        {
            _fileDownloaderService = fileDownloaderService;
        }

        internal bool IsUnitTest { get; set; }

        #region Public Methods

        public async Task<bool> SaveImageFileAsync(string url, string fileNameWithoutExtension)
        {
            string fileExtension = Path.GetExtension(url);
            string fileName = Path.ChangeExtension(fileNameWithoutExtension, fileExtension);

            // download file from web into temp folder
            string imgFile = Path.Combine(Path.GetTempPath(), fileName);
            bool downloadSucceeded = await _fileDownloaderService.DownloadFileAsync(url, imgFile);
            if (!downloadSucceeded)
            {
                return false;
            }

            // Resize to 150x150 px and save as JPEG
            string jpgFile = await ResizeFileAsync(imgFile);

            // copy file into Anki's sounds folder
            return await _fileDownloaderService.CopyFileToAnkiFolderAsync(jpgFile);
        }

        #endregion

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
