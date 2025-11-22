// Ignore Spelling: Downloader

using System.Net;
using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

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

        #region Tests for DownloadSoundFileAsync

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenAnkiSoundsFolderDoesNotExist_DisplaysAlertAndReturnsFalse()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = "invalid_folder_path";

            string url = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(false);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();

            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Path to Anki folder is incorrect",
                "Cannot find path to Anki folder 'invalid_folder_path'. Please update it in Settings.",
                "OK"));
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenDestinationFileExistsAndUserDoesNotWantToOverwrite_ReturnsTrue()
        {
            // Arrange
            const bool overwriteFile = false;
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\Anki\Sounds";

            string url = _fixture.Create<Uri>().ToString();
            string word = "existing_sound";

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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlertAsync("File already exists", It.IsAny<string>(), "Yes", "No"))
                .ReturnsAsync(overwriteFile)
                .Verifiable();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(appSettings.AnkiSoundsFolder, $"{word}.mp3")))
                .Returns(true);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.Verify();
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenDestinationFileExistsAndUserWantsToOverwrite_DownloadsAndReturnsTrue()
        {
            // Arrange
            const bool overwriteFile = true;
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\Anki\Sounds";

            string url = "https://example.com/audio.mp3";
            string word = "existing_sound";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            string expectedUrl = $"{translatorAppUrl.TrimEnd('/')}/api/DownloadSound?soundUrl={Uri.EscapeDataString(url)}&word={Uri.EscapeDataString(word)}&version=1&code={requestCode}";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })),
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlertAsync("File already exists", It.IsAny<string>(), "Yes", "No"))
                .ReturnsAsync(overwriteFile)
                .Verifiable();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(appSettings.AnkiSoundsFolder, $"{word}.mp3")))
                .Returns(true);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();


            // Act
            bool result = await sut.DownloadSoundFileAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.Verify();
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenFileDoesNotExist_DownloadsSuccessfullyAndReturnsTrue()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\Anki\Sounds";

            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "new_sound";

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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(appSettings.AnkiSoundsFolder, $"{word}.mp3")))
                .Returns(false);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            settingsServiceMock.Verify(x => x.LoadSettings());
            dialogServiceMock.VerifyNoOtherCalls();
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAsync_WhenServerReturnsError_ThrowsServerErrorException()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\Anki\Sounds";

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

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

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
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\Anki\Sounds";

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

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

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
        public async Task DownloadSoundFileAsync_SendsCorrectNormalizeSoundRequest()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\Anki\Sounds";

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

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(appSettings.AnkiSoundsFolder, $"{word}.mp3")))
                .Returns(false);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Get);
            capturedRequest.RequestUri!.ToString().Should().Be(expectedUrl);
        }

        #endregion

        #region Tests for DownloadSoundFileAndSaveWithFileSaverAsync

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_WhenFileSavedSuccessfully_ReturnsTrue()
        {
            // Arrange
            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            var fileSaverResult = new FileSaverResult(_fixture.Create<string>(), null);
            fileSaverResult.IsSuccessful.Should().BeTrue();

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
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IFileSaver> fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult)
                .Verifiable();

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileSaverMock.Verify();
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_WhenFileSaveFails_ReturnsFalse()
        {
            // Arrange
            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            var fileSaverResult = new FileSaverResult(null, new Exception("Save failed"));
            fileSaverResult.IsSuccessful.Should().BeFalse();

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
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IFileSaver> fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult)
                .Verifiable();

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            fileSaverMock.Verify();
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_WhenServerReturnsError_ThrowsServerErrorException()
        {
            // Arrange
            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "test_word";

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
                    StatusCode = HttpStatusCode.BadRequest,
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            Func<Task> act = async () => await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'BadRequest'.");
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "test_word";

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
            Func<Task> act = async () => await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_SendsCorrectNormalizeSoundRequest()
        {
            // Arrange
            string soundUrl = "https://example.com/audio.mp3";
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            string expectedUrl = $"{translatorAppUrl.TrimEnd('/')}/api/DownloadSound?soundUrl={Uri.EscapeDataString(soundUrl)}&word={Uri.EscapeDataString(word)}&version=1&code={requestCode}";

            var fileSaverResult = new FileSaverResult(_fixture.Create<string>(), null);

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

            Mock<IFileSaver> fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Get);
            capturedRequest.RequestUri!.ToString().Should().Be(expectedUrl);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_SavesFileWithCorrectFileName()
        {
            // Arrange
            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "example_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            var fileSaverResult = new FileSaverResult(_fixture.Create<string>(), null);

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
                    Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IFileSaver> fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult)
                .Verifiable();

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileSaverMock.Verify(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_PassesStreamToFileSaver()
        {
            // Arrange
            string soundUrl = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            byte[] expectedBytes = new byte[] { 10, 20, 30, 40, 50 };
            var fileSaverResult = new FileSaverResult(_fixture.Create<string>(), null);

            Stream? capturedStream = null;

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
                    Content = new StreamContent(new MemoryStream(expectedBytes)),
                });

            var httpClient = new HttpClient(handlerMock.Object);

            Mock<IFileSaver> fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((fileName, stream, ct) => capturedStream = stream)
                .ReturnsAsync(fileSaverResult);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            // Act
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(soundUrl, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            capturedStream.Should().NotBeNull();
        }

        #endregion
    }
}
