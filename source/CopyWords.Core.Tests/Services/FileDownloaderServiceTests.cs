// Ignore Spelling: Downloader

using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
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
        public async Task DownloadFileAsync_WhenServerReturnsError_ThrowsServerErrorException()
        {
            string url = _fixture.Create<Uri>().ToString();

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
                    StatusCode = HttpStatusCode.InternalServerError,
                });

            var httpClient = new HttpClient(handlerMock.Object);

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            Func<Task> act = async () => await sut.DownloadFileAsync(url, CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        [TestMethod]
        public async Task DownloadFileAsync_WhenRequestIsSuccessful_ReturnsStream()
        {
            string url = _fixture.Create<Uri>().ToString();

            byte[] expectedBytes = new byte[] { 1, 2, 3, 4, 5 };

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

            _fixture.Register(() => httpClient);
            var sut = _fixture.Create<FileDownloaderService>();

            using Stream result = await sut.DownloadFileAsync(url, CancellationToken.None);

            result.Should().NotBeNull();

            // Verify we can read the stream
            using var memoryStream = new MemoryStream();
            await result.CopyToAsync(memoryStream);
            memoryStream.ToArray().Should().BeEquivalentTo(expectedBytes);
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
