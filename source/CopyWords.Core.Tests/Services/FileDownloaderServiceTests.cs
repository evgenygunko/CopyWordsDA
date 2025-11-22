// Ignore Spelling: Downloader

using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
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

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.WriteAllBytesAsync(filePath, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            fileIOServiceMock.Setup(x => x.FileExists(filePath)).Returns(false);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();
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

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.WriteAllBytesAsync(filePath, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            fileIOServiceMock.Setup(x => x.FileExists(filePath)).Returns(true);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();
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

        #region Tests for DownloadSoundFileAsync

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenRequestIsSuccessful_ReturnsStream()
        {
            // Arrange
            string soundUrl = "https://example.com/audio.mp3";
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            byte[] expectedBytes = new byte[] { 1, 2, 3, 4, 5 };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("/api/DownloadSound")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(new MemoryStream(expectedBytes)),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            using Stream result = await sut.DownloadSoundFileAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            // Verify we can read the stream
            using var memoryStream = new MemoryStream();
            await result.CopyToAsync(memoryStream);
            memoryStream.ToArray().Should().BeEquivalentTo(expectedBytes);
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenServerReturnsError_ThrowsServerErrorException()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "sound";

            string translatorAppUrl = "https://translator.example.com";
            string requestCode = "test-code-123";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("/api/DownloadSound")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            Func<Task> act = async () => await sut.DownloadSoundFileAsync(url, word, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "sound";

            string translatorAppUrl = "https://translator.example.com";
            string requestCode = "test-code-123";

            var cts = new CancellationTokenSource();
            cts.Cancel();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            Func<Task> act = async () => await sut.DownloadSoundFileAsync(url, word, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_SendsParametersInRequest()
        {
            // Arrange
            string soundUrl = "https://example.com/audio.mp3";
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            string expectedUrl = $"{translatorAppUrl.TrimEnd('/')}/api/DownloadSound?soundUrl={Uri.EscapeDataString(soundUrl)}&word={Uri.EscapeDataString(word)}&version=1&code={requestCode}";

            HttpRequestMessage? capturedRequest = null;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 })),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            using Stream result = await sut.DownloadSoundFileAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Get);
            capturedRequest.RequestUri!.ToString().Should().Be(expectedUrl);
        }

        #endregion
    }
}
