// Ignore Spelling: Downloader

using AutoFixture;
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

        #region Tests for SaveImageFileAsync

        [TestMethod]
        public async Task SaveImageFileAsync_Should_CallFileDownloaderService()
        {
            const string url = "https://example.com/image.png";
            string fileNameWithoutExtension = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<SaveImageFileService>();
            sut.IsUnitTest = true;

            _ = await sut.SaveImageFileAsync(url, fileNameWithoutExtension);

            fileDownloaderServiceMock
                .Verify(x => x.DownloadFileAsync(url, It.Is<string>(s => s.EndsWith($"{fileNameWithoutExtension}.png")), It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SaveImageFileAsync_WhenCannotDownloadFile_ReturnsFalse()
        {
            const string url = "https://example.com/image.png";
            string fileNameWithoutExtension = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock
                .Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = _fixture.Create<SaveImageFileService>();
            sut.IsUnitTest = true;

            bool result = await sut.SaveImageFileAsync(url, fileNameWithoutExtension);

            result.Should().BeFalse();
            fileDownloaderServiceMock
                .Verify(x => x.DownloadFileAsync(url, It.Is<string>(s => s.EndsWith($"{fileNameWithoutExtension}.png")), It.IsAny<CancellationToken>()));
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

            var sut = _fixture.Create<SaveImageFileService>();
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

            var sut = _fixture.Create<SaveImageFileService>();
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

            var sut = _fixture.Create<SaveImageFileService>();
            bool result = await sut.CopyFileToAnkiFolderAsync(sourceFile);

            // We want to return true, because the file already exists and so we can create a link to it in the back card
            result.Should().BeTrue();

            settingsServiceMock.Verify();
            dialogServiceMock.Verify();
            fileIOServiceMock.Verify(x => x.CopyFile(sourceFile, It.IsAny<string>(), true));
        }

        #endregion
    }
}
