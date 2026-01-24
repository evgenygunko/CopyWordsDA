// Ignore Spelling: Downloader

using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using FluentAssertions;
using Moq;

#pragma warning disable CA1416 // Validate platform compatibility

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SaveSoundFileServiceTests
    {
        private Fixture _fixture = default!;
        private Mock<IGlobalSettings> _globalSettingsMock = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns("http://fake-translator-app-url");
            _globalSettingsMock.SetupGet(x => x.TranslatorAppRequestCode).Returns("fake-request-code");
        }

        #region Tests for CreateDownloadSoundFileUrl

        [TestMethod]
        public void CreateDownloadSoundFileUrl_Should_AddParametersToRequest()
        {
            // Arrange
            string soundUrl = "https://example.com/audio.mp3";
            string word = "test_word";

            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            globalSettingsMock.Setup(x => x.TranslatorAppRequestCode).Returns(requestCode);

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            string result = sut.CreateDownloadSoundFileUrl(soundUrl, word);

            // Assert
            result.Should().NotBeNull();

            string expectedUrl = $"{translatorAppUrl.TrimEnd('/')}/api/v1/Sound/DownloadSound?soundUrl={Uri.EscapeDataString(soundUrl)}&word={Uri.EscapeDataString(word)}&version=1&code={requestCode}";
            result.Should().Be(expectedUrl);
        }

        #endregion

        #region Tests for SaveSoundFileToAnkiFolderAsync

        [TestMethod]
        public async Task SaveSoundFileToAnkiFolderAsync_WhenAnkiFolderDoesNotExist_DisplaysAlertAndReturnsFalse()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = @"C:\Invalid\Path"
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(@"C:\Invalid\Path")).Returns(false);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Path to Anki folder is incorrect",
                @"Cannot find path to Anki folder 'C:\Invalid\Path'. Please update it in Settings.",
                "OK"), Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundFileToAnkiFolderAsync_WhenFileDoesNotExist_DownloadsAndSavesFile()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "test_word";
            string ankiFolder = @"C:\Anki\Sounds";

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = ankiFolder
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(ankiFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(ankiFolder, $"{word}.mp3"))).Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), Path.Combine(ankiFolder, $"{word}.mp3"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundFileToAnkiFolderAsync_WhenFileExistsAndUserWantsToOverwrite_DownloadsAndOverwritesFile()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "existing_word";
            string ankiFolder = @"C:\Anki\Sounds";

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = ankiFolder
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(ankiFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(ankiFolder, $"{word}.mp3"))).Returns(true);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync("File already exists", $"File '{word}.mp3' already exists. Overwrite?", "Yes", "No"))
                .ReturnsAsync(true);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("File already exists", $"File '{word}.mp3' already exists. Overwrite?", "Yes", "No"), Times.Once);
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), Path.Combine(ankiFolder, $"{word}.mp3"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundFileToAnkiFolderAsync_WhenFileExistsAndUserDoesNotWantToOverwrite_ReturnsTrue()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "existing_word";
            string ankiFolder = @"C:\Anki\Sounds";

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = ankiFolder
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(ankiFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(Path.Combine(ankiFolder, $"{word}.mp3"))).Returns(true);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync("File already exists", $"File '{word}.mp3' already exists. Overwrite?", "Yes", "No"))
                .ReturnsAsync(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("File already exists", $"File '{word}.mp3' already exists. Overwrite?", "Yes", "No"), Times.Once);

            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            // but the user refused to overwrite existing file
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
