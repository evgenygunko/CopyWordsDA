// Ignore Spelling: Downloader

using AutoFixture;
using CopyWords.Core.Services;
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
        public async Task SaveSoundFileAsync_WhenCannotCopyFileToAnkiFolder_ShouldStillCopyToClipboard()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string soundFileName = _fixture.Create<string>();
            string filePath = Path.Combine(Path.GetTempPath(), soundFileName);

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock.Setup(x => x.DownloadFileAsync(url, filePath))
                .ReturnsAsync(true);

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();

            // Act
            var sut = _fixture.Create<SaveSoundFileService>();
            bool result = await sut.SaveSoundFileAsync(url, soundFileName, It.IsAny<CancellationToken>());

            // Assert
            result.Should().BeFalse();
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync($"[sound:{Path.GetFileNameWithoutExtension(filePath)}.mp3]"));
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_OnAndroid_CallsFileDownloaderService()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            string soundFileName = _fixture.Create<string>();
            string filePath = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock.Setup(x => x.SaveFileWithFileSaverAsync(url, soundFileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            bool result = await sut.SaveSoundFileAsync(url, soundFileName, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify();
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_OnWindows_CallsFileDownloaderService()
        {
            // Arrange
            string url = _fixture.Create<Uri>().ToString();
            const string soundFileName = "sound_file.mp3";

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<SaveSoundFileService>();

            // Act
            _ = await sut.SaveSoundFileAsync(url, soundFileName, CancellationToken.None);

            // Assert
            fileDownloaderServiceMock.Verify(x => x.DownloadFileAsync(url, It.Is<string>(s => s.EndsWith(soundFileName))));
        }

        #endregion
    }
}
