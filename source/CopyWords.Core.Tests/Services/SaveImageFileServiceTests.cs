using AutoFixture;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SaveImageFileServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public async Task SaveImageFileAsync_Should_CallCopyFileToAnkiFolderAsync()
        {
            const string url = "https://example.com/image.png";
            string fileNameWithoutExtension = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            fileDownloaderServiceMock.Setup(x => x.CopyFileToAnkiFolderAsync(It.IsAny<string>())).ReturnsAsync(true);

            var sut = _fixture.Create<SaveImageFileService>();
            sut.IsUnitTest = true;

            bool result = await sut.SaveImageFileAsync(url, fileNameWithoutExtension);

            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(url, It.Is<string>(s => s.EndsWith($"{fileNameWithoutExtension}.png"))));
            fileDownloaderServiceMock.Verify(x => x.CopyFileToAnkiFolderAsync(It.Is<string>(s => s.EndsWith($"{fileNameWithoutExtension}.png"))));
        }

        [TestMethod]
        public async Task SaveImageFileAsync_WhenCannotDownloadFile_ReturnsFalse()
        {
            const string url = "https://example.com/image.png";
            string fileNameWithoutExtension = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var sut = _fixture.Create<SaveImageFileService>();
            sut.IsUnitTest = true;

            bool result = await sut.SaveImageFileAsync(url, fileNameWithoutExtension);

            result.Should().BeFalse();
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(url, It.Is<string>(s => s.EndsWith($"{fileNameWithoutExtension}.png"))));
            fileDownloaderServiceMock.Verify(x => x.CopyFileToAnkiFolderAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
