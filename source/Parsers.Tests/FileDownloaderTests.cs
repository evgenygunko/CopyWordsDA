// Ignore Spelling: Downloader

using System.Net;
using System.Text;
using CopyWords.Parsers.Exceptions;
using CopyWords.Parsers.Services;
using FluentAssertions;

namespace CopyWords.Parsers.Tests
{
    [TestClass]
    public class FileDownloaderTests
    {
        [TestMethod]
        public async Task DownloadPageAsync_WhenResponseIsSuccess_ReturnsDecodedContent()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "islygte "));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            string? result = await sut.DownloadPageAsync("https://gammel.ordnet.dk/ddo/ordbog?query=islygte", Encoding.UTF8, CancellationToken.None);

            result.Should().Be("islygte");
        }

        [TestMethod]
        public async Task DownloadPageAsync_WhenResponseIsNotFound_ReturnsNull()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.NotFound, "not-found "));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            string? result = await sut.DownloadPageAsync("https://gammel.ordnet.dk/ddo/ordbog?query=islygte", Encoding.UTF8, CancellationToken.None);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task DownloadPageAllowNotFoundAsync_WhenResponseIsNotFound_ReturnsContent()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.NotFound, "islygte "));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            string? result = await sut.DownloadPageAllowNotFoundAsync("https://gammel.ordnet.dk/ddo/ordbog?query=islygte", Encoding.UTF8, CancellationToken.None);

            result.Should().Be("islygte");
        }

        [TestMethod]
        public async Task DownloadPageAllowNotFoundAsync_WhenResponseIsServerError_Throws()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.InternalServerError, "error "));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            Func<Task> act = async () => await sut.DownloadPageAllowNotFoundAsync("https://gammel.ordnet.dk/ddo/ordbog?query=islygte", Encoding.UTF8, CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>();
        }

        [TestMethod]
        public async Task DownloadPageAsync_DoesNotOverrideNativeHeaders()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "islygte "));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);
            const string url = "https://gammel.ordnet.dk/ddo/ordbog?query=bedrift";

            await sut.DownloadPageAsync(url, Encoding.UTF8, CancellationToken.None);

            handler.LastRequest.Should().NotBeNull();
            HttpRequestMessage request = handler.LastRequest!;
            request.Headers.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResponseIsValid_ReturnsSuggestions()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"results\":[\"ser\",\"sera\",\"serio\"]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            IEnumerable<string> result = await sut.GetSpanishWordsSuggestionsAsync("ser", CancellationToken.None);

            result.Should().Equal("ser", "sera", "serio");
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResponseDoesNotContainResults_Throws()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"unexpected\":[]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            Func<Task> act = async () => await sut.GetSpanishWordsSuggestionsAsync("ser", CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>();
        }

        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_AddsOnlyJsonAcceptHeader()
        {
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"results\":[\"ser\"]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            await sut.GetSpanishWordsSuggestionsAsync("ser", CancellationToken.None);

            handler.LastRequest.Should().NotBeNull();
            HttpRequestMessage request = handler.LastRequest!;
            request.Headers.Should().ContainSingle();
            request.Headers.Accept.Should().ContainSingle()
                .Which.MediaType.Should().Be("application/json");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync throws ServerErrorException
        /// when the HTTP response status code indicates a client error (400 Bad Request).
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResponseIsBadRequest_ThrowsServerErrorException()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.BadRequest, "bad request"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            Func<Task> act = async () => await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("*BadRequest*");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync throws ServerErrorException
        /// when the HTTP response status code indicates a server error (500 Internal Server Error).
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResponseIsInternalServerError_ThrowsServerErrorException()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.InternalServerError, "server error"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            Func<Task> act = async () => await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("*InternalServerError*");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync throws ServerErrorException
        /// when the HTTP response status code is 404 Not Found.
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResponseIsNotFound_ThrowsServerErrorException()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.NotFound, "not found"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            Func<Task> act = async () => await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("*NotFound*");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync throws ServerErrorException
        /// when the response contains invalid JSON content.
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResponseIsInvalidJson_ThrowsServerErrorException()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "not valid json {{{"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            Func<Task> act = async () => await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("*invalid JSON*");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync returns an empty list
        /// when the "results" array is empty.
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResultsArrayIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"results\":[]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            IEnumerable<string> result = await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync filters out empty strings
        /// and whitespace-only strings from the results.
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenResultsContainEmptyAndWhitespace_FiltersThemOut()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"results\":[\"word1\",\"\",\"word2\",\" \",\"word3\",\"  \"]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            IEnumerable<string> result = await sut.GetSpanishWordsSuggestionsAsync("test", CancellationToken.None);

            // Assert
            result.Should().Equal("word1", "word2", "word3");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync correctly handles
        /// an empty input text string.
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenInputTextIsEmpty_ReturnsResults()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"results\":[\"suggestion1\",\"suggestion2\"]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            IEnumerable<string> result = await sut.GetSpanishWordsSuggestionsAsync(string.Empty, CancellationToken.None);

            // Assert
            result.Should().Equal("suggestion1", "suggestion2");
        }

        /// <summary>
        /// Tests that GetSpanishWordsSuggestionsAsync correctly handles
        /// input text with special characters that need URL encoding.
        /// </summary>
        [TestMethod]
        public async Task GetSpanishWordsSuggestionsAsync_WhenInputTextHasSpecialCharacters_ReturnsResults()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(CreateResponse(HttpStatusCode.OK, "{\"results\":[\"niño\",\"niña\"]}"));
            using var httpClient = new HttpClient(handler);
            var sut = CreateSut(httpClient);

            // Act
            IEnumerable<string> result = await sut.GetSpanishWordsSuggestionsAsync("niño & café", CancellationToken.None);

            // Assert
            result.Should().Equal("niño", "niña");
        }

        private static FileDownloader CreateSut(HttpClient httpClient)
        {
            return new FileDownloader(httpClient);
        }

        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return new HttpResponseMessage(statusCode)
            {
                Content = new ByteArrayContent(bytes)
            };
        }

        private sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response = response;

            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(_response);
            }
        }
    }
}
