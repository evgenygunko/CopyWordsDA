using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace CopyWords.Core.Services.Tests
{
    [TestClass]
    public class UpdateServiceTests
    {
        [TestMethod]
        public async Task GetLatestReleaseVersionAsync_WhenCanParseVersion_ReturnsVersion()
        {
            // Arrange
            string jsonResponse = "{\"tag_name\": \"v1.2.3\"}";

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
                    Content = new StringContent(jsonResponse),
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sur = new UpdateService(httpClient);

            // Act
            Version result = await sur.GetLatestReleaseVersionAsync();

            // Assert
            result.Should().BeEquivalentTo(new Version(1, 2, 3));
        }

        [TestMethod]
        public async Task GetLatestReleaseVersionAsync_WhenCannotParseVersionFromTag_ReturnsDefaultVersion()
        {
            // Arrange
            string jsonResponse = "{\"tag_name\": \"invalid_version\"}";

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
                    Content = new StringContent(jsonResponse),
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sut = new UpdateService(httpClient);

            // Act
            Version result = await sut.GetLatestReleaseVersionAsync();

            // Assert
            result.Should().BeEquivalentTo(new Version(0, 0));
        }

        [TestMethod]
        public void GetLatestReleaseVersionAsync_WhenHttpError_ThrowsException()
        {
            // Arrange
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
            var sut = new UpdateService(httpClient);

            // Act & Assert
            _ = sut.Invoking(x => x.GetLatestReleaseVersionAsync())
                .Should().ThrowAsync<HttpRequestException>();
        }
    }
}
