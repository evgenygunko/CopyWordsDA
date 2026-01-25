using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
        public async Task AddNoteAsync_WhenCanAddNote_ReturnsNoteIdAndShowsEditWindow()
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
        public async Task AddNoteAsync_WhenNoteExistsAndUserChoosesToUpdate_UpdatesNoteAndReturnsNoteId()
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
                    // Return existing note ID
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":[456],\"error\":null}")
                    };
                }

                // Default success response for other actions (like updateNoteFields and guiEditNote)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(ds => ds.DisplayAlertAsync(
                    "Note already exists",
                    $"Note '{note.Front}' already exists in the deck '{note.DeckName}'. Do you want to update it with current values from CopyWords?",
                    "Yes",
                    "No"))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(456);

            // Verify that the dialog was shown
            dialogServiceMock.Verify(ds => ds.DisplayAlertAsync(
                "Note already exists",
                $"Note '{note.Front}' already exists in the deck '{note.DeckName}'. Do you want to update it with current values from CopyWords?",
                "Yes",
                "No"), Times.Once);

            // Verify that updateNoteFields was called
            var updateRequest = capturedBodies.FirstOrDefault(b => b != null && b.Contains("\"action\":\"updateNoteFields\""));
            updateRequest.Should().NotBeNull();
            JObject updatePayload = JObject.Parse(updateRequest!);
            updatePayload["params"]!["note"]!["id"]!.Value<long>().Should().Be(456);

            // Verify that guiEditNote was called with the correct note ID
            var editNoteRequest = capturedBodies.FirstOrDefault(b => b != null && b.Contains("\"action\":\"guiEditNote\""));
            editNoteRequest.Should().NotBeNull();
            JObject editPayload = JObject.Parse(editNoteRequest!);
            editPayload["params"]!["note"]!.Value<long>().Should().Be(456);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenNoteExistsAndUserChoosesNotToUpdate_ReturnsZeroWithoutUpdating()
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

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                string? requestBody = request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null;
                capturedBodies.Add(requestBody);

                if (requestBody != null && requestBody.Contains("\"action\":\"addNote\""))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":null,\"error\":\"cannot create note because it is a duplicate\"}")
                    };
                }
                else if (requestBody != null && requestBody.Contains("\"action\":\"findNotes\""))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":[456],\"error\":null}")
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(ds => ds.DisplayAlertAsync(
                    "Note already exists",
                    $"Note '{note.Front}' already exists in the deck '{note.DeckName}'. Do you want to update it with current values from CopyWords?",
                    "Yes",
                    "No"))
                .ReturnsAsync(false);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(0);

            // Verify that the dialog was shown
            dialogServiceMock.Verify(ds => ds.DisplayAlertAsync(
                "Note already exists",
                $"Note '{note.Front}' already exists in the deck '{note.DeckName}'. Do you want to update it with current values from CopyWords?",
                "Yes",
                "No"), Times.Once);

            // Verify that updateNoteFields was NOT called
            var updateRequest = capturedBodies.FirstOrDefault(b => b != null && b.Contains("\"action\":\"updateNoteFields\""));
            updateRequest.Should().BeNull();

            // Verify that guiEditNote was still called (since noteId > 0, the edit window is shown regardless of update choice)
            var editNoteRequest = capturedBodies.FirstOrDefault(b => b != null && b.Contains("\"action\":\"guiEditNote\""));
            editNoteRequest.Should().NotBeNull();
            JObject editPayload = JObject.Parse(editNoteRequest!);
            editPayload["params"]!["note"]!.Value<long>().Should().Be(456);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenNoteExistsButCannotFindNoteId_ReturnsZeroAndShowsEditWindow()
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

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                string? requestBody = request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null;

                if (requestBody != null && requestBody.Contains("\"action\":\"addNote\""))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":null,\"error\":\"cannot create note because it is a duplicate\"}")
                    };
                }
                else if (requestBody != null && requestBody.Contains("\"action\":\"findNotes\""))
                {
                    // Return empty result - note not found
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"result\":[],\"error\":null}")
                    };
                }

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

            // Verify that no dialog was shown (since noteId was 0)
            dialogServiceMock.Verify(ds => ds.DisplayAlertAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenValidationFails_ThrowsArgumentException()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: string.Empty,  // Invalid - empty deck name
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: "[sound:file.mp3]",
                Tags: new[] { "tag1", "tag2" });

            var validationResult = _fixture.Create<ValidationResult>();
            validationResult.Errors.Clear();
            validationResult.Errors.Add(new ValidationFailure("DeckName", "DeckName cannot be null"));

            var ankiNoteValidatorMock = _fixture.Freeze<Mock<IValidator<AnkiNote>>>();
            ankiNoteValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenAnkiConnectNotRunning_ThrowsAnkiConnectNotRunningException()
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
            var act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AnkiConnectNotRunningException>()
                .WithMessage("Connection refused");
        }

        #endregion

        #region Tests for AddNoteToAnkiAsync

        [TestMethod]
        public async Task AddNoteToAnkiAsync_WhenSuccess_ReturnsNoteId()
        {
            // Arrange
            var audio = new AnkiMedia(Url: "http://example.com/sound.mp3", Filename: "test_sound");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Sound: null,
                Audio: new[] { audio },
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
            long result = await sut.AddNoteToAnkiAsync(note, CancellationToken.None);

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
            fields["Sound"]!.Value<string>().Should().Be($"[sound:{audio.Filename}.mp3]");

            noteObject["tags"]!.Values<string>().Should().BeEquivalentTo(note.Tags);
        }

        [TestMethod]
        public async Task AddNoteToAnkiAsync_WhenErrorReturned_ThrowsInvalidOperationException()
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
            var act = async () => await sut.AddNoteToAnkiAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to add note to Anki: deck not found");
        }

        [TestMethod]
        public async Task AddNoteToAnkiAsync_WhenHttpFailure_ThrowsInvalidOperationException()
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
            var act = async () => await sut.AddNoteToAnkiAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to add note to Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task AddNoteToAnkiAsync_WhenResultMissing_ThrowsInvalidOperationException()
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
            var act = async () => await sut.AddNoteToAnkiAsync(note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("AnkiConnect did not return a note id.");
        }

        [TestMethod]
        public async Task AddNoteToAnkiAsync_WhenDuplicateNote_ThrowsAnkiNoteExistsException()
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
            Func<Task> act = async () => await sut.AddNoteToAnkiAsync(note, CancellationToken.None);

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

        #region Tests for ShowAnkiEditNoteWindowAsync

        [TestMethod]
        public async Task ShowAnkiEditNoteWindowAsync_WhenValidNoteId_SendsCorrectRequest()
        {
            // Arrange
            long noteId = 1514547547030;
            var capturedBodies = new List<string?>();
            var capturedUris = new List<Uri?>();

            HttpClient httpClient = CreateHttpClient((request, cancellationToken) =>
            {
                capturedUris.Add(request.RequestUri);
                capturedBodies.Add(request.Content != null ? request.Content.ReadAsStringAsync(cancellationToken).Result : null);

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.ShowAnkiEditNoteWindowAsync(noteId, CancellationToken.None);

            // Assert
            capturedBodies.Should().HaveCount(1);
            capturedUris.Should().HaveCount(1);

            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));

            capturedBodies[0].Should().NotBeNull();
            JObject payload = JObject.Parse(capturedBodies[0]!);
            payload["action"]!.Value<string>().Should().Be("guiEditNote");
            payload["version"]!.Value<int>().Should().Be(6);
            payload["params"]!["note"]!.Value<long>().Should().Be(noteId);
        }

        #endregion

        #region Tests for UpdateNoteWithAnkiConnectAsync

        [TestMethod]
        public async Task UpdateNoteWithAnkiConnectAsync_WhenSuccess_UpdatesNoteFields()
        {
            // Arrange
            long noteId = 1514547547030;
            var audio = new AnkiMedia(Url: "http://example.com/sound.mp3", Filename: "test_sound");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Updated Front",
                Back: "Updated Back",
                PartOfSpeech: "verb",
                Forms: "updated forms",
                Example: "updated example",
                Sound: null,
                Audio: new[] { audio },
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
                    Content = new StringContent("{\"result\":null,\"error\":null}")
                };
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.UpdateNoteWithAnkiConnectAsync(noteId, note, CancellationToken.None);

            // Assert
            capturedBodies.Should().HaveCount(1);
            capturedUris.Should().HaveCount(1);

            capturedUris[0].Should().Be(new Uri("http://127.0.0.1:8765"));

            capturedBodies[0].Should().NotBeNull();
            JObject payload = JObject.Parse(capturedBodies[0]!);
            payload["action"]!.Value<string>().Should().Be("updateNoteFields");
            payload["version"]!.Value<int>().Should().Be(6);

            JObject noteObject = (JObject)payload["params"]!["note"]!;
            noteObject["id"]!.Value<long>().Should().Be(noteId);

            JObject fields = (JObject)noteObject["fields"]!;
            fields["Front"]!.Value<string>().Should().Be(note.Front);
            fields["Back"]!.Value<string>().Should().Be(note.Back);
            fields["PartOfSpeech"]!.Value<string>().Should().Be(note.PartOfSpeech);
            fields["Forms"]!.Value<string>().Should().Be(note.Forms);
            fields["Example"]!.Value<string>().Should().Be(note.Example);
            fields["Sound"]!.Value<string>().Should().Be($"[sound:{audio.Filename}.mp3]");
        }

        [TestMethod]
        public async Task UpdateNoteWithAnkiConnectAsync_WhenErrorReturned_ThrowsInvalidOperationException()
        {
            // Arrange
            long noteId = 123456;
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"result\":null,\"error\":\"note not found\"}")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.UpdateNoteWithAnkiConnectAsync(noteId, note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to update note in Anki: note not found");
        }

        [TestMethod]
        public async Task UpdateNoteWithAnkiConnectAsync_WhenHttpFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            long noteId = 123456;
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
            var act = async () => await sut.UpdateNoteWithAnkiConnectAsync(noteId, note, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to update note in Anki: HTTP 500 (Internal Server Error)");
        }

        [TestMethod]
        public async Task UpdateNoteWithAnkiConnectAsync_WhenEmptyResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            long noteId = 123456;
            var note = _fixture.Create<AnkiNote>() with { DeckName = "deck", ModelName = "model" };

            HttpClient httpClient = CreateHttpClient((_, _) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

            _fixture.Inject(httpClient);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            var act = async () => await sut.UpdateNoteWithAnkiConnectAsync(noteId, note, CancellationToken.None);

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

        #region Tests for BuildFields

        [TestMethod]
        public void BuildFields_WhenAllFieldsProvided_ReturnsCorrectDictionary()
        {
            // Arrange
            var audio = new AnkiMedia(Url: "http://example.com/sound.mp3", Filename: "test_sound");
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "noun",
                Forms: "form1, form2",
                Example: "example text",
                Audio: new[] { audio });

            // Act
            var result = AnkiConnectService.BuildFields(note);

            // Assert
            result.Should().HaveCount(6);
            result["Front"].Should().Be("Front text");
            result["Back"].Should().Be("Back text");
            result["PartOfSpeech"].Should().Be("noun");
            result["Forms"].Should().Be("form1, form2");
            result["Example"].Should().Be("example text");
            result["Sound"].Should().Be("[sound:test_sound.mp3]");
        }

        [TestMethod]
        public void BuildFields_WhenOptionalFieldsAreNull_ReturnsEmptyStrings()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: null,
                Forms: null,
                Example: null,
                Audio: null);

            // Act
            var result = AnkiConnectService.BuildFields(note);

            // Assert
            result.Should().HaveCount(6);
            result["Front"].Should().Be("Front text");
            result["Back"].Should().Be("Back text");
            result["PartOfSpeech"].Should().BeEmpty();
            result["Forms"].Should().BeEmpty();
            result["Example"].Should().BeEmpty();
            result["Sound"].Should().BeEmpty();
        }

        [TestMethod]
        public void BuildFields_WhenAudioListIsEmpty_ReturnsSoundAsEmptyString()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                PartOfSpeech: "verb",
                Forms: "forms",
                Example: "example",
                Audio: Enumerable.Empty<AnkiMedia>());

            // Act
            var result = AnkiConnectService.BuildFields(note);

            // Assert
            result["Sound"].Should().BeEmpty();
        }

        [TestMethod]
        public void BuildFields_WhenMultipleAudioFiles_UsesFirstAudioFile()
        {
            // Arrange
            var audio1 = new AnkiMedia(Url: "http://example.com/sound1.mp3", Filename: "first_sound");
            var audio2 = new AnkiMedia(Url: "http://example.com/sound2.mp3", Filename: "second_sound");
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Audio: new[] { audio1, audio2 });

            // Act
            var result = AnkiConnectService.BuildFields(note);

            // Assert
            result["Sound"].Should().Be("[sound:first_sound.mp3]");
        }

        [TestMethod]
        public void BuildFields_WhenFrontAndBackContainSpecialCharacters_PreservesContent()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Hej! <b>Hvordan</b> går det?",
                Back: "Hello! <i>How</i> are you? &amp; fine",
                PartOfSpeech: "phrase",
                Forms: null,
                Example: "Example with \"quotes\" and 'apostrophes'");

            // Act
            var result = AnkiConnectService.BuildFields(note);

            // Assert
            result["Front"].Should().Be("Hej! <b>Hvordan</b> går det?");
            result["Back"].Should().Be("Hello! <i>How</i> are you? &amp; fine");
            result["Example"].Should().Be("Example with \"quotes\" and 'apostrophes'");
        }

        #endregion

        #region Tests for SaveMediaFilesAsync

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenNoteHasAudioFiles_SavesSoundFiles()
        {
            // Arrange
            var audio1 = new AnkiMedia(Url: "http://example.com/sound1.mp3", Filename: "sound1");
            var audio2 = new AnkiMedia(Url: "http://example.com/sound2.mp3", Filename: "sound2");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Audio: new[] { audio1, audio2 });

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(s => s.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(audio1.Url, audio1.Filename, It.IsAny<CancellationToken>()),
                Times.Once);
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(audio2.Url, audio2.Filename, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenNoteHasPictureFiles_SavesImageFiles()
        {
            // Arrange
            var picture1 = new AnkiMedia(Url: "http://example.com/image1.jpg", Filename: "image1");
            var picture2 = new AnkiMedia(Url: "http://example.com/image2.jpg", Filename: "image2");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Picture: new[] { picture1, picture2 });

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(s => s.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveImageFileServiceMock.Verify(
                s => s.SaveImagesAsync(
                    It.Is<IEnumerable<ImageFile>>(files =>
                        files.Count() == 2 &&
                        files.Any(f => f.FileName == picture1.Url && f.ImageUrl == picture1.Filename) &&
                        files.Any(f => f.FileName == picture2.Url && f.ImageUrl == picture2.Filename)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenAudioIsNull_DoesNotCallSaveSoundFileService()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Audio: null);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenAudioIsEmpty_DoesNotCallSaveSoundFileService()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Audio: Enumerable.Empty<AnkiMedia>());

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenPictureIsNull_DoesNotCallSaveImageFileService()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Picture: null);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveImageFileServiceMock.Verify(
                s => s.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenPictureIsEmpty_CallsSaveImageFileServiceWithEmptyCollection()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Picture: Enumerable.Empty<AnkiMedia>());

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(s => s.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveImageFileServiceMock.Verify(
                s => s.SaveImagesAsync(
                    It.Is<IEnumerable<ImageFile>>(files => !files.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenAudioUrlIsEmpty_SkipsAudioFile()
        {
            // Arrange
            var audioWithUrl = new AnkiMedia(Url: "http://example.com/sound.mp3", Filename: "sound");
            var audioWithoutUrl = new AnkiMedia(Url: string.Empty, Filename: "empty_sound");
            var audioWithNullUrl = new AnkiMedia(Url: null!, Filename: "null_sound");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Audio: new[] { audioWithUrl, audioWithoutUrl, audioWithNullUrl });

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(s => s.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(audioWithUrl.Url, audioWithUrl.Filename, It.IsAny<CancellationToken>()),
                Times.Once);
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenPictureUrlIsEmpty_SkipsPictureFile()
        {
            // Arrange
            var pictureWithUrl = new AnkiMedia(Url: "http://example.com/image.jpg", Filename: "image");
            var pictureWithoutUrl = new AnkiMedia(Url: string.Empty, Filename: "empty_image");
            var pictureWithNullUrl = new AnkiMedia(Url: null!, Filename: "null_image");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Picture: new[] { pictureWithUrl, pictureWithoutUrl, pictureWithNullUrl });

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(s => s.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveImageFileServiceMock.Verify(
                s => s.SaveImagesAsync(
                    It.Is<IEnumerable<ImageFile>>(files =>
                        files.Count() == 1 &&
                        files.Single().FileName == pictureWithUrl.Url &&
                        files.Single().ImageUrl == pictureWithUrl.Filename),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveMediaFilesAsync_WhenNoteHasBothAudioAndPicture_SavesBothTypes()
        {
            // Arrange
            var audio = new AnkiMedia(Url: "http://example.com/sound.mp3", Filename: "sound");
            var picture = new AnkiMedia(Url: "http://example.com/image.jpg", Filename: "image");

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text",
                Audio: new[] { audio },
                Picture: new[] { picture });

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(s => s.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(s => s.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiConnectService>();

            // Act
            await sut.SaveMediaFilesAsync(note, CancellationToken.None);

            // Assert
            saveSoundFileServiceMock.Verify(
                s => s.SaveSoundFileToAnkiFolderAsync(audio.Url, audio.Filename, It.IsAny<CancellationToken>()),
                Times.Once);

            saveImageFileServiceMock.Verify(
                s => s.SaveImagesAsync(
                    It.Is<IEnumerable<ImageFile>>(files =>
                        files.Count() == 1 &&
                        files.Single().FileName == picture.Url &&
                        files.Single().ImageUrl == picture.Filename),
                    It.IsAny<CancellationToken>()),
                Times.Once);
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
