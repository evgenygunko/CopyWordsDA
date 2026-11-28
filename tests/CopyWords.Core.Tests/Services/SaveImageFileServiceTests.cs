// Ignore Spelling: Downloader

using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SaveImageFileServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for SaveImagesAsync

        [TestMethod]
        public async Task SaveImagesAsync_WhenAnkiFolderDoesNotExist_DisplaysAlertAndReturnsFalse()
        {
            var imageFile = new ImageFile("image.jpg", "https://example.com/image.png");
            var imageFiles = new List<ImageFile> { imageFile };

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = "nonexistent-folder";

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(false);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SaveImageFileService>();

            bool result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            result.Should().BeFalse();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{appSettings.AnkiSoundsFolder}'. Please update it in Settings.", "OK"));
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenDownloadSucceedsAndFileDoesNotExist_SavesResizedImage()
        {
            var imageFile = new ImageFile("image.jpg", "https://example.com/image.png");
            var imageFiles = new List<ImageFile> { imageFile };

            AppSettings appSettings = _fixture.Create<AppSettings>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

            var imageSharpWrapperMock = _fixture.Freeze<Mock<IImageSharpWrapper>>();
            imageSharpWrapperMock
                .Setup(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SixLabors.ImageSharp.Image)null!);
            imageSharpWrapperMock
                .Setup(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = _fixture.Create<SaveImageFileService>();

            bool result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(imageFile.ImageUrl, It.IsAny<CancellationToken>()), Times.Once);
            imageSharpWrapperMock.Verify(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
            imageSharpWrapperMock.Verify(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenFileExistsAndUserDoesNotWantToOverwrite_ReturnsTrue()
        {
            var imageFile1 = new ImageFile("image1.jpg", "https://example.com/image1.jpg");
            var imageFile2 = new ImageFile("image2.jpg", "https://example.com/image2.jpg");
            var imageFiles = new List<ImageFile> { imageFile1, imageFile2 };

            AppSettings appSettings = _fixture.Create<AppSettings>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.SetupSequence(x => x.FileExists(It.IsAny<string>()))
                .Returns(true)
                .Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync("File already exists", It.IsAny<string>(), "Yes", "No"))
                .ReturnsAsync(false);

            var imageSharpWrapperMock = _fixture.Freeze<Mock<IImageSharpWrapper>>();
            imageSharpWrapperMock
                .Setup(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SixLabors.ImageSharp.Image)null!);

            var sut = _fixture.Create<SaveImageFileService>();

            bool result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            result.Should().BeTrue();
            imageSharpWrapperMock.Verify(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);

            // Skip first file, but save second file
            imageSharpWrapperMock.Verify(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(),
                It.Is<string>(str => str.EndsWith(imageFile1.FileName)), It.IsAny<CancellationToken>()), Times.Never);

            imageSharpWrapperMock.Verify(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(),
                It.Is<string>(str => str.EndsWith(imageFile2.FileName)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenFileExistsAndUserWantsToOverwrite_SavesResizedImage()
        {
            var imageFile1 = new ImageFile("image1.jpg", "https://example.com/image1.jpg");
            var imageFile2 = new ImageFile("image2.jpg", "https://example.com/image2.jpg");
            var imageFiles = new List<ImageFile> { imageFile1, imageFile2 };

            AppSettings appSettings = _fixture.Create<AppSettings>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.SetupSequence(x => x.FileExists(It.IsAny<string>()))
                .Returns(true)
                .Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync("File already exists", It.IsAny<string>(), "Yes", "No"))
                .ReturnsAsync(true);

            var imageSharpWrapperMock = _fixture.Freeze<Mock<IImageSharpWrapper>>();
            imageSharpWrapperMock
                .Setup(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SixLabors.ImageSharp.Image)null!);
            imageSharpWrapperMock
                .Setup(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = _fixture.Create<SaveImageFileService>();

            bool result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            result.Should().BeTrue();
            imageSharpWrapperMock.Verify(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            // Save both files
            imageSharpWrapperMock.Verify(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(),
                It.Is<string>(str => str.EndsWith(imageFile1.FileName)), It.IsAny<CancellationToken>()), Times.Once);

            imageSharpWrapperMock.Verify(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(),
                It.Is<string>(str => str.EndsWith(imageFile2.FileName)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenDownloadThrowsServerErrorException_DisplaysAlertAndReturnsFalse()
        {
            var imageFile = new ImageFile("image.jpg", "https://example.com/image.png");
            var imageFiles = new List<ImageFile> { imageFile };

            AppSettings appSettings = _fixture.Create<AppSettings>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServerErrorException("Server error"));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SaveImageFileService>();

            bool result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            result.Should().BeFalse();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot download image", $"Cannot download image file from '{imageFile.ImageUrl}'. Error: Server error", "OK"));
        }

        #endregion

        #region Tests for DownloadAndResizeImageAsync

        [TestMethod]
        public async Task DownloadAndResizeImageAsync_WhenCalled_DownloadsResizesAndReturnsStream()
        {
            string imageUrl = "https://example.com/image.png";
            var expectedStream = new MemoryStream(new byte[] { 10, 20, 30 });

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(imageUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

            var imageSharpWrapperMock = _fixture.Freeze<Mock<IImageSharpWrapper>>();
            imageSharpWrapperMock
                .Setup(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SixLabors.ImageSharp.Image)null!);
            imageSharpWrapperMock
                .Setup(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStream);

            var sut = _fixture.Create<SaveImageFileService>();

            Stream result = await sut.DownloadAndResizeImageAsync(imageUrl, CancellationToken.None);

            result.Should().BeSameAs(expectedStream);
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(imageUrl, It.IsAny<CancellationToken>()), Times.Once);
            imageSharpWrapperMock.Verify(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
            imageSharpWrapperMock.Verify(x => x.SaveAsJpegAsync(It.IsAny<SixLabors.ImageSharp.Image>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadAndResizeImageAsync_WhenDownloadFails_ThrowsException()
        {
            string imageUrl = "https://example.com/image.png";

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(imageUrl, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServerErrorException("Download failed"));

            var sut = _fixture.Create<SaveImageFileService>();

            Func<Task> act = async () => await sut.DownloadAndResizeImageAsync(imageUrl, CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>().WithMessage("Download failed");
        }

        [TestMethod]
        public async Task DownloadAndResizeImageAsync_WhenResizeFails_ThrowsException()
        {
            string imageUrl = "https://example.com/image.png";

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(imageUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

            var imageSharpWrapperMock = _fixture.Freeze<Mock<IImageSharpWrapper>>();
            imageSharpWrapperMock
                .Setup(x => x.ResizeImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Resize failed"));

            var sut = _fixture.Create<SaveImageFileService>();

            Func<Task> act = async () => await sut.DownloadAndResizeImageAsync(imageUrl, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Resize failed");
        }

        #endregion
    }
}
