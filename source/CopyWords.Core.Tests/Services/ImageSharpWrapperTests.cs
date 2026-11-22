using CopyWords.Core.Services.Wrappers;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class ImageSharpWrapperTests
    {
        [TestMethod]
        [DataRow("jpeg")]
        [DataRow("png")]
        public async Task ResizeImageAsync_SupportedImage_ResizesAndSavesAsJpeg(string format)
        {
            using var source = new Image<Rgba32>(300, 100);
            using var input = new MemoryStream();
            if (format == "jpeg")
            {
                await source.SaveAsync(input, new JpegEncoder());
            }
            else
            {
                await source.SaveAsync(input, new PngEncoder());
            }

            input.Position = 0;
            var sut = new ImageSharpWrapper();
            using Image resized = await sut.ResizeImageAsync(input, CancellationToken.None);
            resized.Width.Should().Be(150);
            resized.Height.Should().Be(50);

            using Stream output = await sut.SaveAsJpegAsync(resized, CancellationToken.None);
            Image.DetectFormat(output).Should().Be(JpegFormat.Instance);
        }

        [TestMethod]
        public async Task ResizeImageAsync_ValidTiff_RejectsImage()
        {
            using var source = new Image<Rgba32>(1, 1);
            using var input = new MemoryStream();
            await source.SaveAsync(input, new TiffEncoder());
            input.Position = 0;
            var sut = new ImageSharpWrapper();

            Func<Task> action = async () =>
            {
                using Image image = await sut.ResizeImageAsync(input, CancellationToken.None);
            };

            await action.Should().ThrowAsync<UnknownImageFormatException>();
        }

        [TestMethod]
        [DataRow("49492A0008000000")]
        [DataRow("4D4D002A00000008")]
        [DataRow("49492B0008000000100000000000000000F2052A01000000")]
        [DataRow("4D4D002B0008000000000000000000100000000000000000")]
        public async Task ResizeImageAsync_TiffOrBigTiffHeader_RejectsBeforeDecoding(string hex)
        {
            using var input = new MemoryStream(Convert.FromHexString(hex));
            var sut = new ImageSharpWrapper();

            Func<Task> action = async () =>
            {
                using Image image = await sut.ResizeImageAsync(input, CancellationToken.None);
            };

            await action.Should().ThrowAsync<UnknownImageFormatException>();
        }
    }
}
