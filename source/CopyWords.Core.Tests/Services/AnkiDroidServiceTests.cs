using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class AnkiDroidServiceTests
    {
        private Fixture _fixture = default!;

        [TestInitialize]
        public void SetUp()
        {
            _fixture = FixtureFactory.CreateFixture();
        }

        #region Tests for IsAvailable

        [TestMethod]
        public void IsAvailable_WhenApiReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.IsAvailable()).Returns(true);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            bool result = sut.IsAvailable();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsAvailable_WhenApiReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.IsAvailable()).Returns(false);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            bool result = sut.IsAvailable();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Tests for HasPermission

        [TestMethod]
        public void HasPermission_WhenApiReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.HasPermission()).Returns(true);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            bool result = sut.HasPermission();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void HasPermission_WhenApiReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.HasPermission()).Returns(false);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            bool result = sut.HasPermission();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Tests for RequestPermissionAsync

        [TestMethod]
        public async Task RequestPermissionAsync_CallsApiRequestPermission()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.RequestPermissionAsync()).Returns(Task.CompletedTask);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            await sut.RequestPermissionAsync();

            // Assert
            ankiContentApiMock.Verify(x => x.RequestPermissionAsync(), Times.Once);
        }

        #endregion

        #region Tests for GetDeckNames

        [TestMethod]
        public void GetDeckNames_WhenApiReturnsDeckList_ReturnsDeckNames()
        {
            // Arrange
            var deckList = new Dictionary<long, string>
            {
                { 1L, "Default" },
                { 2L, "Languages" },
                { 3L, "Science" }
            };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<string> result = sut.GetDeckNames();

            // Assert
            result.Should().BeEquivalentTo(["Default", "Languages", "Science"]);
        }

        [TestMethod]
        public void GetDeckNames_WhenApiReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns((IDictionary<long, string>?)null);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<string> result = sut.GetDeckNames();

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetDeckNames_WhenApiReturnsEmptyDictionary_ReturnsEmptyList()
        {
            // Arrange
            var deckList = new Dictionary<long, string>();

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<string> result = sut.GetDeckNames();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Tests for GetModelNames

        [TestMethod]
        public void GetModelNames_WhenApiReturnsModelList_ReturnsModelNames()
        {
            // Arrange
            var modelList = new Dictionary<long, string>
            {
                { 1L, "Basic" },
                { 2L, "Basic (and reversed card)" },
                { 3L, "Cloze" }
            };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<string> result = sut.GetModelNames();

            // Assert
            result.Should().BeEquivalentTo(["Basic", "Basic (and reversed card)", "Cloze"]);
        }

        [TestMethod]
        public void GetModelNames_WhenApiReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns((IDictionary<long, string>?)null);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<string> result = sut.GetModelNames();

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetModelNames_WhenApiReturnsEmptyDictionary_ReturnsEmptyList()
        {
            // Arrange
            var modelList = new Dictionary<long, string>();

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<string> result = sut.GetModelNames();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Tests for AddNoteAsync

        [TestMethod]
        public async Task AddNoteAsync_WhenAllDataValid_ReturnsNoteId()
        {
            // Arrange
            const long expectedDeckId = 1L;
            const long expectedModelId = 100L;
            const long expectedNoteId = 12345L;

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { expectedDeckId, "Default" } };
            var modelList = new Dictionary<long, string> { { expectedModelId, "Basic" } };
            string[] fieldNames = ["Front", "Back"];

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);
            ankiContentApiMock.Setup(x => x.GetFieldList(expectedModelId)).Returns(fieldNames);
            ankiContentApiMock.Setup(x => x.FindDuplicateNotes(expectedModelId, "Front text")).Returns([]);
            ankiContentApiMock
                .Setup(x => x.AddNote(expectedModelId, expectedDeckId, It.IsAny<string[]>(), null))
                .Returns(expectedNoteId);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            long result = await sut.AddNoteAsync(note);

            // Assert
            result.Should().Be(expectedNoteId);
            ankiContentApiMock.Verify(
                x => x.AddNote(expectedModelId, expectedDeckId, It.Is<string[]>(f => f[0] == "Front text" && f[1] == "Back text"), null),
                Times.Once);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenDeckNotFound_ThrowsAnkiDroidDeckNotFoundException()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "NonExistentDeck",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { 1L, "Default" } };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<AnkiDroidDeckNotFoundException>()
                .WithMessage("Deck 'NonExistentDeck' not found in AnkiDroid.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenDeckListIsNull_ThrowsAnkiDroidDeckNotFoundException()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns((IDictionary<long, string>?)null);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<AnkiDroidDeckNotFoundException>()
                .WithMessage("Deck 'Default' not found in AnkiDroid.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenModelNotFound_ThrowsAnkiDroidModelNotFoundException()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "NonExistentModel",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { 1L, "Default" } };
            var modelList = new Dictionary<long, string> { { 100L, "Basic" } };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<AnkiDroidModelNotFoundException>()
                .WithMessage("Model 'NonExistentModel' not found in AnkiDroid.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenModelListIsNull_ThrowsAnkiDroidModelNotFoundException()
        {
            // Arrange
            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { 1L, "Default" } };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns((IDictionary<long, string>?)null);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<AnkiDroidModelNotFoundException>()
                .WithMessage("Model 'Basic' not found in AnkiDroid.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenFieldsNotFound_ThrowsAnkiDroidFieldsNotFoundException()
        {
            // Arrange
            const long modelId = 100L;

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { 1L, "Default" } };
            var modelList = new Dictionary<long, string> { { modelId, "Basic" } };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);
            ankiContentApiMock.Setup(x => x.GetFieldList(modelId)).Returns((string[]?)null);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<AnkiDroidFieldsNotFoundException>()
                .WithMessage("No fields found for model 'Basic'.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenFieldsArrayIsEmpty_ThrowsAnkiDroidFieldsNotFoundException()
        {
            // Arrange
            const long modelId = 100L;

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { 1L, "Default" } };
            var modelList = new Dictionary<long, string> { { modelId, "Basic" } };

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);
            ankiContentApiMock.Setup(x => x.GetFieldList(modelId)).Returns([]);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<AnkiDroidFieldsNotFoundException>()
                .WithMessage("No fields found for model 'Basic'.");
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenDuplicateFoundAndUserDeclines_DoesNotCallAddNote()
        {
            // Arrange
            const long expectedDeckId = 1L;
            const long expectedModelId = 100L;

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { expectedDeckId, "Default" } };
            var modelList = new Dictionary<long, string> { { expectedModelId, "Basic" } };
            string[] fieldNames = ["Front", "Back"];

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);
            ankiContentApiMock.Setup(x => x.GetFieldList(expectedModelId)).Returns(fieldNames);
            ankiContentApiMock.Setup(x => x.FindDuplicateNotes(expectedModelId, "Front text")).Returns([1L]);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(false);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            long result = await sut.AddNoteAsync(note);

            // Assert
            result.Should().Be(0);
            ankiContentApiMock.Verify(
                x => x.AddNote(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string[]>(), It.IsAny<string[]?>()),
                Times.Never);
            dialogServiceMock.Verify(
                x => x.DisplayAlertAsync(
                    "Note already exists",
                    "Note 'Front text' already exists in the deck 'Default'. Do you want to add a duplicate note?",
                    "Yes",
                    "No"),
                Times.Once);
        }

        [TestMethod]
        public async Task AddNoteAsync_WhenDuplicateFoundAndUserAgrees_CallsAddNoteAndReturnsNoteId()
        {
            // Arrange
            const long expectedDeckId = 1L;
            const long expectedModelId = 100L;
            const long expectedNoteId = 12345L;

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "Back text");

            var deckList = new Dictionary<long, string> { { expectedDeckId, "Default" } };
            var modelList = new Dictionary<long, string> { { expectedModelId, "Basic" } };
            string[] fieldNames = ["Front", "Back"];

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);
            ankiContentApiMock.Setup(x => x.GetFieldList(expectedModelId)).Returns(fieldNames);
            ankiContentApiMock.Setup(x => x.FindDuplicateNotes(expectedModelId, "Front text")).Returns([1L]);
            ankiContentApiMock
                .Setup(x => x.AddNote(expectedModelId, expectedDeckId, It.IsAny<string[]>(), null))
                .Returns(expectedNoteId);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            long result = await sut.AddNoteAsync(note);

            // Assert
            result.Should().Be(expectedNoteId);
            ankiContentApiMock.Verify(
                x => x.AddNote(expectedModelId, expectedDeckId, It.Is<string[]>(f => f[0] == "Front text" && f[1] == "Back text"), null),
                Times.Once);
            dialogServiceMock.Verify(
                x => x.DisplayAlertAsync(
                    "Note already exists",
                    "Note 'Front text' already exists in the deck 'Default'. Do you want to add a duplicate note?",
                    "Yes",
                    "No"),
                Times.Once);
        }

        #endregion
    }
}
