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
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for SaveSoundFileAsync

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenOnAndroid_CallsDownloadSoundFileAndSaveWithFileSaverAsync()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            const string soundFileName = "sound_file.mp3";

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock
                .Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileSaverResult("test_path", null));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileAsync(url, soundFileName, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, "sound_file", It.IsAny<CancellationToken>()), Times.Once);
            fileSaverMock.Verify(x => x.SaveAsync("sound_file.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenOnWindows_CallsDownloadSoundFileAndCopyToAnkiFolderAsync()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            const string soundFileName = "sound_file.mp3";

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = @"C:\Anki\Sounds"
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileAsync(url, soundFileName, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync("[sound:sound_file.mp3]"), Times.Once);
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, "sound_file", It.IsAny<CancellationToken>()), Times.Once);
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenOnMacCatalyst_CallsDownloadSoundFileAndCopyToAnkiFolderAsync()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            const string soundFileName = "test_sound.mp3";

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.MacCatalyst);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = "/Users/test/Anki/Sounds"
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileAsync(url, soundFileName, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync("[sound:test_sound.mp3]"), Times.Once);
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, "test_sound", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenOnWindows_CopiesCorrectTextToClipboard()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            const string soundFileName = "my_word.mp3";

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings
            {
                AnkiSoundsFolder = @"C:\Anki\Sounds"
            });

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            await sut.SaveSoundFileAsync(url, soundFileName, CancellationToken.None);

            // Assert
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync("[sound:my_word.mp3]"), Times.Once);
        }

        #endregion

        #region Tests for DownloadSoundFileAndCopyToAnkiFolderAsync

        [TestMethod]
        public async Task DownloadSoundFileAndCopyToAnkiFolderAsync_WhenAnkiFolderDoesNotExist_DisplaysAlertAndReturnsFalse()
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
            bool result = await sut.DownloadSoundFileAndCopyToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Path to Anki folder is incorrect",
                @"Cannot find path to Anki folder 'C:\Invalid\Path'. Please update it in Settings.",
                "OK"), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndCopyToAnkiFolderAsync_WhenFileDoesNotExist_DownloadsAndSavesFile()
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
                .Setup(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.DownloadSoundFileAndCopyToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()), Times.Once);
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), Path.Combine(ankiFolder, $"{word}.mp3"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndCopyToAnkiFolderAsync_WhenFileExistsAndUserWantsToOverwrite_DownloadsAndOverwritesFile()
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
                .Setup(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.DownloadSoundFileAndCopyToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("File already exists", $"File '{word}.mp3' already exists. Overwrite?", "Yes", "No"), Times.Once);
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()), Times.Once);
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), Path.Combine(ankiFolder, $"{word}.mp3"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndCopyToAnkiFolderAsync_WhenFileExistsAndUserDoesNotWantToOverwrite_ReturnsTrue()
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

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.DownloadSoundFileAndCopyToAnkiFolderAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("File already exists", $"File '{word}.mp3' already exists. Overwrite?", "Yes", "No"), Times.Once);

            // we download file anyway
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            // but the user refused to overwrite existing file
            fileIOServiceMock.Verify(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Tests for DownloadSoundFileAndSaveWithFileSaverAsync

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_WhenFileSaverSucceeds_ReturnsTrue()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock
                .Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileSaverResult("test_path", null));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()), Times.Once);
            fileSaverMock.Verify(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_WhenFileSaverFails_ReturnsFalse()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "test_word";

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock
                .Setup(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileSaverResult(null, new Exception("Save failed")));

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.DownloadSoundFileAndSaveWithFileSaverAsync(url, word, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            fileDownloaderServiceMock.Verify(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()), Times.Once);
            fileSaverMock.Verify(x => x.SaveAsync($"{word}.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadSoundFileAndSaveWithFileSaverAsync_SavesFileWithCorrectFileName()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string word = "my_sound";

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadSoundFileAsync(url, word, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 })));

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock
                .Setup(x => x.SaveAsync("my_sound.mp3", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileSaverResult("test_path", null))
                .Verifiable();

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            await sut.DownloadSoundFileAndSaveWithFileSaverAsync(url, word, CancellationToken.None);

            // Assert
            fileSaverMock.Verify();
        }

        #endregion
    }
}
