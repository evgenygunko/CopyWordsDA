// Ignore Spelling: Downloader

using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
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
    }
}
