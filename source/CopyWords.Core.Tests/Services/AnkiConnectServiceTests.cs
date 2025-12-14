using System.Net;
using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
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

        [TestMethod]
        public async Task AddNoteAsync_WhenSuccess_PostsExpectedPayloadAndReturnsNoteId()
        {
            // Arrange
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

            var sut = new AnkiConnectService(httpClient);
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

            // Act
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(123);
            capturedBodies.Should().HaveCount(2, "AddNoteAsync makes two HTTP calls: addNote and guiEditNote");
            capturedUris.Should().HaveCount(2);

            // Verify both requests went to the correct endpoint
            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));
            capturedUris[1].Should().Be(new Uri("http://127.0.0.1:8765"));

            // Verify the first request (addNote)
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

            // Verify the second request (guiEditNote)
            capturedBodies[1].Should().NotBeNull();
            JObject guiEditPayload = JObject.Parse(capturedBodies[1]!);
            guiEditPayload["action"]!.Value<string>().Should().Be("guiEditNote");
            guiEditPayload["version"]!.Value<int>().Should().Be(6);
            guiEditPayload["params"]!["note"]!.Value<long>().Should().Be(123);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":\"deck not found\"}")
            });
            var sut = new AnkiConnectService(httpClient);

            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            // Act
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to add note to Anki: deck not found");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error",
                Content = new StringContent("{}")
            });
            var sut = new AnkiConnectService(httpClient);
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            // Act
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to add note to Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenResultMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":null}")
            });
            var sut = new AnkiConnectService(httpClient);
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            // Act
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect did not return a note id.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenDeckNameEmpty_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
            var sut = new AnkiConnectService(httpClient);
            var note = _fixture.Create<AnkiNote>() with { DeckName = string.Empty, ModelName = "model" };

            // Act
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("note");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenModelNameEmpty_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
            var sut = new AnkiConnectService(httpClient);
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = string.Empty };

            // Act
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("note");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenNoteIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = CreateHttpClient((_, _) => new HttpResponseMessage(HttpStatusCode.OK));
            var sut = new AnkiConnectService(httpClient);

            // Act
            var act = async () => await sut.AddNoteAsync(null!, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

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
    }
}
