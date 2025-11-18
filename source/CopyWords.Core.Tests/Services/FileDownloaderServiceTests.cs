// Ignore Spelling: Downloader

using System.Net;
using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

#pragma warning disable CA1416 // Validate platform compatibility

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class FileDownloaderServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for DownloadFileAsync

        [TestMethod]
        public async Task DownloadFileAsync_WhenUrlIsInvalid_ReturnsFalse()
        {
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>());

            result.Should().BeFalse();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot download file", It.IsAny<string>(), "OK"));
        }

        [TestMethod]
        public async Task DownloadFileAsync_WhenCouldNotSaveTempFile_ReturnsFalse()
        {
            string url = _fixture.Create<Uri>().ToString();
            string filePath = Path.Combine(Path.GetTempPath(), "sound.mp3");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("abc"),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = new FileDownloaderService(
                httpClient,
                dialogServiceMock.Object,
                Mock.Of<IFileIOService>(),
                Mock.Of<ISettingsService>(),
                Mock.Of<IFileSaver>());
            bool result = await sut.DownloadFileAsync(url, filePath);

            result.Should().BeFalse();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot download file", It.IsAny<string>(), "OK"));
        }

        [TestMethod]
        public async Task DownloadFileAsync_WhenFileIsDownloaded_ReturnsTrue()
        {
            string url = _fixture.Create<Uri>().ToString();
            string filePath = Path.Combine(Path.GetTempPath(), "sound.mp3");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("abc"),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = new FileDownloaderService(
                httpClient,
                dialogServiceMock.Object,
                Mock.Of<IFileIOService>(x => x.FileExists(It.IsAny<string?>()) == true),
                Mock.Of<ISettingsService>(),
                Mock.Of<IFileSaver>());
            bool result = await sut.DownloadFileAsync(url, filePath);

            result.Should().BeTrue();
            dialogServiceMock.VerifyNoOtherCalls();
        }

        #endregion

        #region Tests for CopyFileToAnkiFolderAsync

        [TestMethod]
        public async Task CopyFileToAnkiFolderAsync_WhenAnkiSoundsFolderDoesNotExist_DisplaysAlertAndReturnsFalse()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = "abc";

            string sourceFile = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.FileExists(sourceFile)).Returns(true);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(false);

            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.CopyFileToAnkiFolderAsync(sourceFile);

            result.Should().BeFalse();

            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Path to Anki folder is incorrect", "Cannot find path to Anki folder 'abc'. Please update it in Settings.", "OK"));
        }

        [TestMethod]
        public async Task CopyFileToAnkiFolderAsync_WhenDestinationFileExistsAndUserDoesNotWantToOverwrite_SkipsCopying()
        {
            const bool overwriteFile = false;
            AppSettings appSettings = _fixture.Create<AppSettings>();

            string sourceFile = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings).Verifiable();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlertAsync("File already exists", It.IsAny<string>(), "Yes", "No"))
                .ReturnsAsync(overwriteFile)
                .Verifiable();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.SetupSequence(x => x.FileExists(It.IsAny<string>()))
                .Returns(true) // source file
                .Returns(true); // destination file
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);

            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.CopyFileToAnkiFolderAsync(sourceFile);

            // We want to return true, because the file already exists and so we can create a link to it in the back card
            result.Should().BeTrue();

            settingsServiceMock.Verify();
            dialogServiceMock.Verify();
            fileIOServiceMock.Verify(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task CopyFileToAnkiFolderAsync_WhenDestinationFileExistsAndUserWantsToOverwrite_CopiesFile()
        {
            const bool overwriteFile = true;
            AppSettings appSettings = _fixture.Create<AppSettings>();

            string sourceFile = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings).Verifiable();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlertAsync("File already exists", It.IsAny<string>(), "Yes", "No"))
                .ReturnsAsync(overwriteFile)
                .Verifiable();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.SetupSequence(x => x.FileExists(It.IsAny<string>()))
                .Returns(true) // source file
                .Returns(true); // destination file
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);

            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.CopyFileToAnkiFolderAsync(sourceFile);

            // We want to return true, because the file already exists and so we can create a link to it in the back card
            result.Should().BeTrue();

            settingsServiceMock.Verify();
            dialogServiceMock.Verify();
            fileIOServiceMock.Verify(x => x.CopyFile(sourceFile, It.IsAny<string>(), true));
        }

        #endregion

        #region Tests for SaveFileWithFileSaverAsync

        [TestMethod]
        public async Task SaveFileWithFileSaverAsync_Should_CallFileSaver()
        {
            string url = _fixture.Create<Uri>().ToString();
            string soundFileName = _fixture.Create<string>();

            var fileSaverResult = new FileSaverResult(_fixture.Create<string>(), null);
            fileSaverResult.IsSuccessful.Should().BeTrue();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("abc"),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync(soundFileName, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult)
                .Verifiable();

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.SaveFileWithFileSaverAsync(url, soundFileName, It.IsAny<CancellationToken>());

            result.Should().BeTrue();
            fileSaverMock.Verify();
        }

        #endregion
    }
}
