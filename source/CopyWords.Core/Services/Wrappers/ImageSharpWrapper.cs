using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CopyWords.Core.Services.Wrappers
{
    public interface IImageSharpWrapper
    {
        Task<SixLabors.ImageSharp.Image> ResizeImageAsync(Stream stream, CancellationToken cancellationToken);

        Task SaveAsJpegAsync(SixLabors.ImageSharp.Image image, string path, CancellationToken cancellationToken);
    }

    public class ImageSharpWrapper : IImageSharpWrapper
    {
        public async Task<SixLabors.ImageSharp.Image> ResizeImageAsync(Stream stream, CancellationToken cancellationToken)
        {
            SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(stream, cancellationToken);

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

            return image;
        }

        public async Task SaveAsJpegAsync(SixLabors.ImageSharp.Image image, string path, CancellationToken cancellationToken)
        {
            await image.SaveAsJpegAsync(path, cancellationToken);
        }
    }
}
