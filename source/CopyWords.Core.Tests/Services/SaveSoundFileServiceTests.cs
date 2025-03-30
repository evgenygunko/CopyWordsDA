using System.Net;
using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

#pragma warning disable CA1416 // Validate platform compatibility

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SaveSoundFileServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for SaveSoundFileAsync

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenCannotCopyFileToAnkiFolder_StillPutsTextInClipboard()
        {
            string url = _fixture.Create<Uri>().ToString();
            string soundFileName = _fixture.Create<string>();
            string filePath = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock.Setup(x => x.DownloadFileAsync(url, soundFileName))
                .ReturnsAsync(filePath);

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();

            var sut = _fixture.Create<SaveSoundFileService>();
            bool result = await sut.SaveSoundFileAsync(url, soundFileName, It.IsAny<CancellationToken>());

            result.Should().BeFalse();
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync($"[sound:{Path.GetFileNameWithoutExtension(filePath)}.mp3]"));
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
            var sut = _fixture.Create<SaveSoundFileService>();
            bool result = await sut.SaveFileWithFileSaverAsync(url, soundFileName, It.IsAny<CancellationToken>());

            result.Should().BeTrue();
            fileSaverMock.Verify();
        }

        #endregion
    }
}
