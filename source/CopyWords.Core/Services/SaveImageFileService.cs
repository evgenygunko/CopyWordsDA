using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CopyWords.Core.Services
{
    public interface ISaveImageFileService
    {
        Task<bool> SaveImageFileAsync(string url, string fileNameWithoutExtension);
    }

    public class SaveImageFileService : SaveFileServiceBase, ISaveImageFileService
    {
        public SaveImageFileService(
            ISettingsService settingsService,
            HttpClient httpClient,
            IDialogService dialogService,
            IFileIOService fileIOService)
            : base(settingsService, httpClient, dialogService, fileIOService)
        {
        }

        #region Public Methods

        public async Task<bool> SaveImageFileAsync(string url, string fileNameWithoutExtension)
        {
            string fileExtension = Path.GetExtension(url);
            string fileName = Path.ChangeExtension(fileNameWithoutExtension, fileExtension);

            // download file from web into temp folder
            string? imgFile = await DownloadFileAsync(url, fileName);
            if (string.IsNullOrEmpty(imgFile))
            {
                return false;
            }

            // Resize to 150x150 px and save as JPEG
            string jpgFile = await ResizeFileAsync(imgFile);

            // copy file into Anki's sounds folder
            await CopyFileToAnkiFolderAsync(jpgFile);

            return true;
        }

        #endregion

        #region Private Methods

        private async Task<string> ResizeFileAsync(string imgFile)
        {
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
