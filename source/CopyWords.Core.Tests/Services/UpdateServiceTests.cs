using System.Net;
using CopyWords.Core.Models;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace CopyWords.Core.Services.Tests
{
    [TestClass]
    public class UpdateServiceTests
    {
        #region Tests for GetLatestReleaseVersionAsync

        [TestMethod]
        public async Task GetLatestReleaseVersionAsync_Should_TrimVFromVersion()
        {
            // Arrange
            const string jsonResponse = @"
            {
                ""tag_name"": ""v1.2.3"",
                ""body"": ""Release description here""
            }";

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
            ReleaseInfo result = await sut.GetLatestReleaseVersionAsync();

            // Assert
            result.LatestVersion.Should().Be("1.2.3");
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

        #endregion

        #region Tests for IsUpdateAvailableAsync

        [TestMethod]
        public async Task IsUpdateAvailableAsync_WhenCannotParseCurrentVersion_ReturnsFalse()
        {
            const string currentVersion = "invalid_version";

            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);

            var sut = new UpdateService(httpClient);
            bool result = await sut.IsUpdateAvailableAsync(currentVersion);

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task IsUpdateAvailableAsync_WhenCannotParseLatestVersion_ReturnsFalse()
        {
            const string currentVersion = "1.2.3";

            const string jsonResponse = @"
            {
                ""tag_name"": ""invalid_version"",
                ""body"": ""Release description here""
            }";

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
            bool result = await sut.IsUpdateAvailableAsync(currentVersion);

            result.Should().BeFalse();

        }

        [TestMethod]
        public async Task IsUpdateAvailableAsync_WhenCurrentVersionIsGreaterThanOrEqualToLatestVersion_ReturnsFalse()
        {
            const string currentVersion = "1.2.3";

            const string jsonResponse = @"
            {
                ""tag_name"": ""v1.2.3"",
                ""body"": ""Release description here""
            }";

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
            bool result = await sut.IsUpdateAvailableAsync(currentVersion);

            result.Should().BeFalse();

        }

        [TestMethod]
        public async Task IsUpdateAvailableAsync_WhenCurrentVersionIsLessThanLatestVersion_ReturnsTeue()
        {
            const string currentVersion = "1.2.2";

            const string jsonResponse = @"
            {
                ""tag_name"": ""v1.2.3"",
                ""body"": ""Release description here""
            }";

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
            bool result = await sut.IsUpdateAvailableAsync(currentVersion);

            result.Should().BeTrue();


        }
        #endregion
    }
}
