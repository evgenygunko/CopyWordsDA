using System.Net;
using System.Text.Json;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SuggestionsServiceTests
    {
        #region Tests for Danish Suggestions

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanish_ReturnsSuggestions()
        {
            // Arrange
            var suggestions = new[] { "haj", "hus", "have" };
            var json = JsonSerializer.Serialize(suggestions);

            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), nameof(SourceLanguage.Danish));

            // Act
            var result = await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(suggestions);
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanishAndResponseIsNull_ThrowsServerErrorException()
        {
            // Arrange
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, "null"), nameof(SourceLanguage.Danish), isDebug: true);

            // Act
            var act = async () => await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned a successful status code but the response content was null.");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanishAndServerErrorInDebug_ThrowsServerErrorException()
        {
            // Arrange
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error"), nameof(SourceLanguage.Danish), isDebug: true);

            // Act
            var act = async () => await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanishAndServerErrorInRelease_ReturnsEmptyCollection()
        {
            // Arrange
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error"), nameof(SourceLanguage.Danish), isDebug: false);

            // Act
            var result = await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanishAndTaskCanceled_ReturnsEmptyCollection()
        {
            // Arrange
            var httpClient = CreateCancelledHttpClient();
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Danish));

            // Act
            var result = await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanishAndInputContainsSpaces_EscapesInputText()
        {
            // Arrange
            string? actualUrl = null;
            var httpClient = CreateCapturedRequestHttpClient("""["test"]""", request => actualUrl = request.RequestUri?.OriginalString);
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Danish));

            // Act
            await sut.GetSuggestionsAsync("hello world", CancellationToken.None);

            // Assert
            actualUrl.Should().Contain("hello%20world");
        }

        #endregion

        #region Tests for Spanish Suggestions

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanish_ReturnsSuggestions()
        {
            // Arrange
            var responseObject = new { results = new[] { "hola", "casa", "perro" } };
            var json = JsonSerializer.Serialize(responseObject);

            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), nameof(SourceLanguage.Spanish));

            // Act
            var result = await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(responseObject.results);
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanishAndResultsPropertyMissing_ThrowsServerErrorException()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { data = new[] { "test" } });
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), nameof(SourceLanguage.Spanish), isDebug: true);

            // Act
            var act = async () => await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned a successful status code but the response content was invalid or missing 'results'.");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanishAndArrayContainsNonStrings_FiltersOutNonStrings()
        {
            // Arrange
            var json = """{"results":["hola",123,"casa",null,"perro"]}""";
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), nameof(SourceLanguage.Spanish));

            // Act
            var result = await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(new[] { "hola", "casa", "perro" });
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanishAndServerErrorInDebug_ThrowsServerErrorException()
        {
            // Arrange
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error"), nameof(SourceLanguage.Spanish), isDebug: true);

            // Act
            var act = async () => await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanishAndTaskCanceled_ReturnsEmptyCollection()
        {
            // Arrange
            var sut = CreateSut(CreateCancelledHttpClient(), nameof(SourceLanguage.Spanish));

            // Act
            var result = await sut.GetSuggestionsAsync("h", CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanishAndInputContainsSpaces_EscapesInputText()
        {
            // Arrange
            string? actualUrl = null;
            var json = JsonSerializer.Serialize(new { results = new[] { "test" } });
            var httpClient = CreateCapturedRequestHttpClient(json, request => actualUrl = request.RequestUri?.OriginalString);
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Spanish));

            // Act
            await sut.GetSuggestionsAsync("hello world", CancellationToken.None);

            // Assert
            actualUrl.Should().Contain("hello%20world");
        }

        #endregion

        #region Tests for Russian Helper Suggestions

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenInputContainsCyrillic_ReturnsRussianSuggestions()
        {
            // Arrange
            var json = """{"pages":[{"title":"дом"},{"title":"домен"},{"title":"домик"}]}""";
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), nameof(SourceLanguage.Danish));

            // Act
            var result = await sut.GetSuggestionsAsync("дом", CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(new[] { "дом", "домен", "домик" });
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenInputContainsCyrillicAndSelectedParserIsInvalid_StillUsesRussianSuggestions()
        {
            // Arrange
            var json = """{"pages":[{"title":"дом"}]}""";
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), "InvalidLanguage");

            // Act
            var result = await sut.GetSuggestionsAsync("дом", CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(["дом"]);
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenInputContainsCyrillicAndPagesMissing_ThrowsServerErrorException()
        {
            // Arrange
            var json = """{"query":{"other":[]}}""";
            var sut = CreateSut(CreateMockHttpClient(HttpStatusCode.OK, json), nameof(SourceLanguage.Spanish), isDebug: true);

            // Act
            var act = async () => await sut.GetSuggestionsAsync("дом", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned a successful status code but the response content was invalid or missing 'pages'.");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenInputContainsCyrillicAndTaskCanceled_ReturnsEmptyCollection()
        {
            // Arrange
            var sut = CreateSut(CreateCancelledHttpClient(), nameof(SourceLanguage.Danish));

            // Act
            var result = await sut.GetSuggestionsAsync("дом", CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenInputContainsCyrillic_EscapesInputText()
        {
            // Arrange
            string? actualUrl = null;
            var json = """{"pages":[{"title":"дом"}]}""";
            var httpClient = CreateCapturedRequestHttpClient(json, request => actualUrl = request.RequestUri?.OriginalString);
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Danish));

            // Act
            await sut.GetSuggestionsAsync("дом тест", CancellationToken.None);

            // Assert
            actualUrl.Should().Contain("%D0%B4%D0%BE%D0%BC%20%D1%82%D0%B5%D1%81%D1%82");
        }

        #endregion

        #region Tests for Shared Behavior

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsInvalidAndInputIsNotCyrillic_ReturnsEmptyWithoutCallingRemoteService()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(handlerMock.Object);
            var sut = CreateSut(httpClient, "InvalidLanguage");

            // Act
            var result = await sut.GetSuggestionsAsync("hello", CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanish_SetsAcceptHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var httpClient = CreateCapturedRequestHttpClient("""["test"]""", request => capturedRequest = request);
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Danish));

            // Act
            await sut.GetSuggestionsAsync("test", CancellationToken.None);

            // Assert
            capturedRequest!.Headers.Accept.Should().Contain(h => h.MediaType == "application/json");
            capturedRequest.Headers.GetValues("User-Agent").Should().Contain("Chrome/145.0.0.0");
            capturedRequest.Headers.GetValues("User-Agent").Should().Contain("Mozilla/5.0");
            capturedRequest.Headers.GetValues("Accept-Language").Should().Contain("en-US");
            capturedRequest.Headers.GetValues("Accept-Language").Should().Contain("en; q=0.9");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanish_SetsAcceptHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var json = JsonSerializer.Serialize(new { results = new[] { "test" } });
            var httpClient = CreateCapturedRequestHttpClient(json, request => capturedRequest = request);
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Spanish));

            // Act
            await sut.GetSuggestionsAsync("test", CancellationToken.None);

            // Assert
            capturedRequest!.Headers.Accept.Should().Contain(h => h.MediaType == "application/json");
            capturedRequest.Headers.GetValues("User-Agent").Should().Contain("Chrome/145.0.0.0");
            capturedRequest.Headers.GetValues("User-Agent").Should().Contain("Mozilla/5.0");
            capturedRequest.Headers.GetValues("Accept-Language").Should().Contain("en-US");
            capturedRequest.Headers.GetValues("Accept-Language").Should().Contain("en; q=0.9");
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenInputContainsCyrillic_SetsAcceptHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var json = """{"pages":[{"title":"дом"}]}""";
            var httpClient = CreateCapturedRequestHttpClient(json, request => capturedRequest = request);
            var sut = CreateSut(httpClient, nameof(SourceLanguage.Danish));

            // Act
            await sut.GetSuggestionsAsync("дом", CancellationToken.None);

            // Assert
            capturedRequest!.Headers.Accept.Should().Contain(h => h.MediaType == "application/json");
            capturedRequest.Headers.GetValues("User-Agent").Should().Contain("Chrome/145.0.0.0");
            capturedRequest.Headers.GetValues("User-Agent").Should().Contain("Mozilla/5.0");
            capturedRequest.Headers.GetValues("Accept-Language").Should().Contain("en-US");
            capturedRequest.Headers.GetValues("Accept-Language").Should().Contain("en; q=0.9");
        }

        #endregion

        #region Private Methods

        private static SuggestionsService CreateSut(HttpClient httpClient, string selectedParser, bool isDebug = false)
        {
            var buildConfigurationMock = new Mock<IBuildConfiguration>();
            buildConfigurationMock.SetupGet(x => x.IsDebug).Returns(isDebug);

            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(selectedParser);

            return new SuggestionsService(httpClient, buildConfigurationMock.Object, settingsServiceMock.Object);
        }

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

        private static HttpClient CreateCancelledHttpClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException());

            return new HttpClient(handlerMock.Object);
        }

        private static HttpClient CreateCapturedRequestHttpClient(string content, Action<HttpRequestMessage> captureRequest)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => captureRequest(request))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                });

            return new HttpClient(handlerMock.Object);
        }

        #endregion
    }
}
