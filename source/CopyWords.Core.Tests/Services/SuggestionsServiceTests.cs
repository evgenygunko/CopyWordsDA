using System.Net;
using System.Text.Json;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SuggestionsServiceTests
    {
        #region Tests for GetDanishWordsSuggestionsAsync

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_WhenSuccess_ReturnsSuggestions()
        {
            // Arrange
            var suggestions = new[] { "haj", "hus", "have" };
            var json = JsonSerializer.Serialize(suggestions);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetDanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(suggestions);
        }

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_WhenResponseIsNull_ThrowsServerErrorException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "null");

            // Act
            var sut = new SuggestionsService(httpClient);
            var act = async () => await sut.GetDanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned a successful status code but the response content was null.");
        }

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_WhenServerError_ThrowsServerErrorException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var sut = new SuggestionsService(httpClient);
            var act = async () => await sut.GetDanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_WhenTaskCanceled_ReturnsEmptyCollection()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException());

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetDanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_WhenEmptyArray_ReturnsEmptyCollection()
        {
            // Arrange
            var suggestions = Array.Empty<string>();
            var json = JsonSerializer.Serialize(suggestions);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetDanishWordsSuggestionsAsync("xyz", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_Should_EscapeInputText()
        {
            // Arrange
            var suggestions = new[] { "test" };
            var json = JsonSerializer.Serialize(suggestions);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            string? actualUrl = null;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    actualUrl = request.RequestUri?.OriginalString;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var sut = new SuggestionsService(httpClient);
            await sut.GetDanishWordsSuggestionsAsync("hello world", CancellationToken.None);

            // Assert
            actualUrl.Should().Contain("hello%20world");
        }

        #endregion

        #region Tests for GetSpanishWordsSuggestionsAsync

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenSuccess_ReturnsSuggestions()
        {
            // Arrange
            var suggestions = new[] { "hola", "casa", "perro" };
            var responseObject = new { results = suggestions };

            var json = JsonSerializer.Serialize(responseObject);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetSpanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(suggestions);
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResultsPropertyMissing_ThrowsServerErrorException()
        {
            // Arrange
            var responseObject = new { data = new[] { "test" } };
            var json = JsonSerializer.Serialize(responseObject);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var act = async () => await sut.GetSpanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned a successful status code but the response content was invalid or missing 'results'.");
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResultsIsNotArray_ThrowsServerErrorException()
        {
            // Arrange
            var responseObject = new { results = "not an array" };
            var json = JsonSerializer.Serialize(responseObject);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var act = async () => await sut.GetSpanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned a successful status code but the response content was invalid or missing 'results'.");
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenArrayContainsNonStrings_FiltersOutNonStrings()
        {
            // Arrange
            var json = """{"results": ["hola", 123, "casa", null, "perro"]}""";
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetSpanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new[] { "hola", "casa", "perro" });
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenServerError_ThrowsServerErrorException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");

            // Act
            var sut = new SuggestionsService(httpClient);
            var act = async () => await sut.GetSpanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenTaskCanceled_ReturnsEmptyCollection()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException());

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetSpanishWordsSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenEmptyResultsArray_ReturnsEmptyCollection()
        {
            // Arrange
            var responseObject = new { results = Array.Empty<string>() };
            var json = JsonSerializer.Serialize(responseObject);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            // Act
            var sut = new SuggestionsService(httpClient);
            var result = await sut.GetSpanishWordsSuggestionsAsync("xyz", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_Should_EscapeInputText()
        {
            // Arrange
            var responseObject = new { results = new[] { "test" } };
            var json = JsonSerializer.Serialize(responseObject);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            string? actualUrl = null;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    actualUrl = request.RequestUri?.OriginalString;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var sut = new SuggestionsService(httpClient);
            await sut.GetSpanishWordsSuggestionsAsync("hello world", CancellationToken.None);

            // Assert
            actualUrl.Should().Contain("hello%20world");
        }

        #endregion

        #region Tests for HTTP Request Headers

        [TestMethod]
        public async Task GetDanishWordsSuggestionsAsync_Should_SetCorrectHeaders()
        {
            // Arrange
            var suggestions = new[] { "test" };
            var json = JsonSerializer.Serialize(suggestions);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpRequestMessage? capturedRequest = null;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    capturedRequest = request;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var sut = new SuggestionsService(httpClient);
            await sut.GetDanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Headers.Accept.Should().Contain(h => h.MediaType == "application/json");
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_Should_SetCorrectHeaders()
        {
            // Arrange
            var responseObject = new { results = new[] { "test" } };
            var json = JsonSerializer.Serialize(responseObject);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpRequestMessage? capturedRequest = null;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    capturedRequest = request;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var sut = new SuggestionsService(httpClient);
            await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Headers.Accept.Should().Contain(h => h.MediaType == "application/json");
        }

        #endregion

        #region Private Methods

        private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
            return new HttpClient(handlerMock.Object);
        }

        #endregion
    }
}