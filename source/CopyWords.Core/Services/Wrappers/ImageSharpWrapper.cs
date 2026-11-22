using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace CopyWords.Core.Services.Wrappers
{
    public interface IImageSharpWrapper
    {
        Task<SixLabors.ImageSharp.Image> ResizeImageAsync(Stream stream, CancellationToken cancellationToken);

        Task SaveAsJpegAsync(SixLabors.ImageSharp.Image image, string path, CancellationToken cancellationToken);

        Task<Stream> SaveAsJpegAsync(SixLabors.ImageSharp.Image image, CancellationToken cancellationToken);
    }

    public class ImageSharpWrapper : IImageSharpWrapper
    {
        // Inspect image contents using only JPEG and PNG decoders. Registering no TIFF decoder
        // blocks the known ImageSharp 3.1.12 TIFF vulnerabilities, even for files named .jpg.
        private static readonly Configuration ImageConfiguration = new(
            new JpegConfigurationModule(),
            new PngConfigurationModule());

        public async Task<SixLabors.ImageSharp.Image> ResizeImageAsync(Stream stream, CancellationToken cancellationToken)
        {
            var options = new DecoderOptions { Configuration = ImageConfiguration };
            SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(options, stream, cancellationToken);

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

        public async Task<Stream> SaveAsJpegAsync(SixLabors.ImageSharp.Image image, CancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();
            await image.SaveAsJpegAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
