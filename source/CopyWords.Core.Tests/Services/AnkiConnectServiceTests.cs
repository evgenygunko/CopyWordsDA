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
                string? requestBody = request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null;
                capturedBodies.Add(requestBody);

                // Return different responses based on the action in the request
                if (requestBody != null && requestBody.Contains("\"action\":\"addNote\""))
                {
                    // Return duplicate error for addNote
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":null,\"error\":\"cannot create note because it is a duplicate\"}")
                    };
                }
                else if (requestBody != null && requestBody.Contains("\"action\":\"findNotes\""))
                {
                    // Return empty result for findNotes (note not found)
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":[],\"error\":null}")
                    };
                }

                // Default success response for other actions (like guiEditNote)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
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
                $"Cannot add '{note.Front}' because it already exists.",
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

        #region Tests for FindExistingNoteIdAsync

        [TestMethod]
        public async Task FindExistingNoteIdAsync_WhenNoteFound_ReturnsFirstNoteId()
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

            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":[456,789],\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.FindExistingNoteIdAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(456);
            capturedBodies.Should().HaveCount(1);
            capturedUris.Should().HaveCount(1);

            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));

            capturedBodies[0].Should().NotBeNull();
            JObject payload = JObject.Parse(capturedBodies[0]!);
            payload["action"]!.Value<string>().Should().Be("findNotes");
            payload["version"]!.Value<int>().Should().Be(6);
            payload["params"]!["query"]!.Value<string>().Should().Be($"\"deck:{note.DeckName}\" \"Front:{note.Front}\"");
        }

        [TestMethod]
        public async Task FindExistingNoteIdAsync_WhenNoNoteFound_ReturnsZero()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Nonexistent Word",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: "[sound:file.mp3]",
                Tags: new[] { "tag1", "tag2" });

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":[],\"error\":null}")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.FindExistingNoteIdAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task FindExistingNoteIdAsync_WhenResultIsNull_ReturnsZero()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":null}")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.FindExistingNoteIdAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task FindExistingNoteIdAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":\"deck not found\"}")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.FindExistingNoteIdAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to find notes in Anki: deck not found");
        }

        [TestMethod]
        public async Task FindExistingNoteIdAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error",
                Content = new StringContent("{\"result\":null,\"error\":null}")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.FindExistingNoteIdAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to find notes in Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task FindExistingNoteIdAsync_WhenEmptyResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.FindExistingNoteIdAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect returned an empty response.");
        }

        #endregion

        #region Tests for GetDeckNamesAsync

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenSuccess_ReturnsDeckNames()
        {
            // Arrange
            var expectedDeckNames = new[] { "Default", "Spanish", "German", "French" };
            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                // First request is the connectivity check (GET), second is deckNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":[\"Default\",\"Spanish\",\"German\",\"French\"],\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedDeckNames);
            capturedBodies.Should().HaveCount(2);
            capturedUris.Should().HaveCount(2);

            // Both requests should go to the same endpoint
            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));
            capturedUris[1].Should().Be(new Uri("http://127.0.0.1:8765"));

            // First request is connectivity check (no body)
            capturedBodies[0].Should().BeNull();

            // Second request is deckNames
            capturedBodies[1].Should().NotBeNull();
            JObject payload = JObject.Parse(capturedBodies[1]!);
            payload["action"]!.Value<string>().Should().Be("deckNames");
            payload["version"]!.Value<int>().Should().Be(6);
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenEmptyDeckList_ReturnsEmptyCollection()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is deckNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":[],\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenResultIsNull_ReturnsEmptyCollection()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is deckNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is deckNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":\"collection is not available\"}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to get deck names from Anki: collection is not available");
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is deckNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "Internal Server Error",
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to get deck names from Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenEmptyResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is deckNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect returned an empty response.");
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenAnkiConnectNotRunning_ThrowsAnkiConnectNotRunningException()
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
            var act = async () => await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("Connection refused");
        }

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenCancellationRequested_ThrowsAnkiConnectNotRunningException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("The operation was canceled."));

            var httpClient = new HttpClient(handlerMock.Object);
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("The operation was canceled.");
        }

        #endregion

        #region Tests for GetModelNamesAsync

        [TestMethod]
        public async Task GetModelNamesAsync_WhenSuccess_ReturnsModelNames()
        {
            // Arrange
            var expectedModelNames = new[] { "Basic", "Basic (and reversed card)", "Cloze", "Custom Model" };
            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                // First request is the connectivity check (GET), second is modelNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":[\"Basic\",\"Basic (and reversed card)\",\"Cloze\",\"Custom Model\"],\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedModelNames);
            capturedBodies.Should().HaveCount(2);
            capturedUris.Should().HaveCount(2);

            // Both requests should go to the same endpoint
            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));
            capturedUris[1].Should().Be(new Uri("http://127.0.0.1:8765"));

            // First request is connectivity check (no body)
            capturedBodies[0].Should().BeNull();

            // Second request is modelNames
            capturedBodies[1].Should().NotBeNull();
            JObject payload = JObject.Parse(capturedBodies[1]!);
            payload["action"]!.Value<string>().Should().Be("modelNames");
            payload["version"]!.Value<int>().Should().Be(6);
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenEmptyModelList_ReturnsEmptyCollection()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is modelNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":[],\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenResultIsNull_ReturnsEmptyCollection()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is modelNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is modelNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":\"collection is not available\"}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to get model names from Anki: collection is not available");
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is modelNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "Internal Server Error",
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to get model names from Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenEmptyResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is modelNames (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect returned an empty response.");
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenAnkiConnectNotRunning_ThrowsAnkiConnectNotRunningException()
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
            var act = async () => await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("Connection refused");
        }

        [TestMethod]
        public async Task GetModelNamesAsync_WhenCancellationRequested_ThrowsAnkiConnectNotRunningException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("The operation was canceled."));

            var httpClient = new HttpClient(handlerMock.Object);
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("The operation was canceled.");
        }

        #endregion

        #region Tests for GetAnkiMediaDirectoryPathAsync

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenSuccess_ReturnsMediaDirectoryPath()
        {
            // Arrange
            var expectedPath = "/home/user/.local/share/Anki2/Main/collection.media";
            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                // First request is the connectivity check (GET), second is getMediaDirPath (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"/home/user/.local/share/Anki2/Main/collection.media\",\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            result.Should().Be(expectedPath);
            capturedBodies.Should().HaveCount(2);
            capturedUris.Should().HaveCount(2);

            // Both requests should go to the same endpoint
            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));
            capturedUris[1].Should().Be(new Uri("http://127.0.0.1:8765"));

            // First request is connectivity check (no body)
            capturedBodies[0].Should().BeNull();

            // Second request is getMediaDirPath
            capturedBodies[1].Should().NotBeNull();
            JObject payload = JObject.Parse(capturedBodies[1]!);
            payload["action"]!.Value<string>().Should().Be("getMediaDirPath");
            payload["version"]!.Value<int>().Should().Be(6);
        }

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenResultIsNull_ReturnsNull()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is getMediaDirPath (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is getMediaDirPath (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":\"collection is not available\"}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to get media directory path from Anki: collection is not available");
        }

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is getMediaDirPath (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "Internal Server Error",
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to get media directory path from Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenEmptyResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpClient httpClient = CreateHttpClient((request, _) =>
            {
                // First request is the connectivity check (GET), second is getMediaDirPath (POST)
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect returned an empty response.");
        }

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenAnkiConnectNotRunning_ReturnsNull()
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
            var result = await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetAnkiMediaDirectoryPathAsync_WhenCancellationRequested_ReturnsNull()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("The operation was canceled."));

            var httpClient = new HttpClient(handlerMock.Object);
            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var result = await sut.GetAnkiMediaDirectoryPathAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
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
