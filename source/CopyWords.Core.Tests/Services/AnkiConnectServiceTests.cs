using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class AnkiConnectServiceTests
    {
        private Fixture _fixture = default!;

        [TestInitialize]
        public void SetUp()
        {
            _fixture = FixtureFactory.CreateFixture();
        }

        #region Tests for AddNoteAsync

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenCanAddNote_ReturnsNoteId()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: "[sound:file.mp3]",
                Tags: new[] { "tag1", "tag2" });

            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                // Return success response for both addNote and guiEditNote
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":123,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(123);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenNoteExists_ShowsAlert()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: "[sound:file.mp3]",
                Tags: new[] { "tag1", "tag2" });

            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                // Return success response for both addNote and guiEditNote
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":\"cannot create note because it is a duplicate\"}")
                };
            });

            _fixture.Inject(httpClient);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(0);

            dialogServiceMock.Verify(ds => ds.DisplayAlertAsync(
                "Cannot add note",
                $"Cannot add '{note.Front}' because it already exists",
                "OK"), Times.Once);
        }

        #endregion

        #region Tests for AddNoteWithAnkiConnectAsync

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenSuccess_ReturnsNoteId()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: "[sound:file.mp3]",
                Tags: new[] { "tag1", "tag2" });

            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                // Return success response for both addNote and guiEditNote
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":123,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.AddNoteWithAnkiConnectAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(123);
            capturedBodies.Should().HaveCount(1);
            capturedUris.Should().HaveCount(1);

            // Verify that requests went to the correct endpoint
            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));

            // Verify the request (addNote)
            capturedBodies[0].Should().NotBeNull();
            JObject addNotePayload = JObject.Parse(capturedBodies[0]!);
            addNotePayload["action"]!.Value<string>().Should().Be("addNote");
            addNotePayload["version"]!.Value<int>().Should().Be(6);

            JObject noteObject = (JObject)addNotePayload["params"]!["note"]!;
            noteObject["deckName"]!.Value<string>().Should().Be(note.DeckName);
            noteObject["modelName"]!.Value<string>().Should().Be(note.ModelName);

            JObject fields = (JObject)noteObject["fields"]!;
            fields["Front"]!.Value<string>().Should().Be(note.Front);
            fields["Back"]!.Value<string>().Should().Be(note.Back);
            fields["PartOfSpeech"]!.Value<string>().Should().Be(note.PartOfSpeech);
            fields["Forms"]!.Value<string>().Should().Be(note.Forms);
            fields["Example"]!.Value<string>().Should().Be(note.Example);
            fields["Sound"]!.Value<string>().Should().Be(note.Sound);

            noteObject["tags"]!.Values<string>().Should().BeEquivalentTo(note.Tags);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":\"deck not found\"}")
            });
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.AddNoteWithAnkiConnectAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to add note to Anki: deck not found");
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error",
                Content = new StringContent("{}")
            });
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.AddNoteWithAnkiConnectAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to add note to Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenResultMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":null}")
            });
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.AddNoteWithAnkiConnectAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect did not return a note id.");
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenDuplicateNote_ThrowsAnkiNoteExistsException()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Test Word",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: "[sound:file.mp3]",
                Tags: new[] { "tag1", "tag2" });

            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":\"cannot create note because it is a duplicate\"}")
            });
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteWithAnkiConnectAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiNoteExistsException>()
                .WithMessage("cannot create note because it is a duplicate");
        }

        #endregion

        #region Tests for CheckThatAnkiConnectIsRunningAsync

        [TestMethod]
        public async Task CheckThatAnkiConnectIsRunningAsync_WhenAnkiConnectResponds_DoesNotThrow()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("AnkiConnect is running")
            });
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.CheckThatAnkiConnectIsRunningAsync(CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task CheckThatAnkiConnectIsRunningAsync_WhenHttpRequestExceptionOccurs_ThrowsAnkiConnectNotRunningException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            var httpClient = new HttpClient(handlerMock.Object);
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.CheckThatAnkiConnectIsRunningAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("Connection refused");
        }

        [TestMethod]
        public async Task CheckThatAnkiConnectIsRunningAsync_WhenTaskCanceledExceptionOccurs_ThrowsAnkiConnectNotRunningException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Request timed out"));

            var httpClient = new HttpClient(handlerMock.Object);
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.CheckThatAnkiConnectIsRunningAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("Request timed out");
        }

        #endregion

        #region Private Methods

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) => responseFactory(request, ct));

            return new HttpClient(handlerMock.Object);
        }

        #endregion
    }
}
