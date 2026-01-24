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
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

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
            Func<Task> act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

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
            Func<Task> act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

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
            Func<Task> act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

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
            Func<Task> act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

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
            Func<Task> act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

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
            Func<Task> act = async () => await sut.AddNoteAsync(note, CancellationToken.None);

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
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

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
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

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

        [TestMethod]
        public async Task AddNoteAsync_Should_UpdateImageTagsInBackField()
        {
            // Arrange
            const long expectedDeckId = 1L;
            const long expectedModelId = 100L;
            const long expectedNoteId = 12345L;

            const string backWithUpdatedImageTags = "back <img src=\"image1_12345.png\" /> <img src=\"image2_67890.png\" />";

            var ankiMediaImages = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: _fixture.Create<Uri>().ToString(), Filename: "image1.png"),
                new AnkiMedia(Url: _fixture.Create<Uri>().ToString(), Filename: "image2.png")
            };

            var note = new AnkiNote(
                DeckName: "Default",
                ModelName: "Basic",
                Front: "Front text",
                Back: "back <img src=\"image1.png\"> <img src=\"image2.png\">",
                Picture: ankiMediaImages);

            var deckList = new Dictionary<long, string> { { expectedDeckId, "Default" } };
            var modelList = new Dictionary<long, string> { { expectedModelId, "Basic" } };
            string[] fieldNames = ["Front", "Back"];

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock.Setup(x => x.GetDeckList()).Returns(deckList);
            ankiContentApiMock.Setup(x => x.GetModelList()).Returns(modelList);
            ankiContentApiMock.Setup(x => x.GetFieldList(expectedModelId)).Returns(fieldNames);
            ankiContentApiMock.Setup(x => x.FindDuplicateNotes(expectedModelId, "Front text")).Returns([]);
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync("image1.png", It.IsAny<Stream>()))
                .ReturnsAsync("<img src=\"image1_12345.png\" />");
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync("image2.png", It.IsAny<Stream>()))
                .ReturnsAsync("<img src=\"image2_67890.png\" />");
            ankiContentApiMock
                .Setup(x => x.AddNote(expectedModelId, expectedDeckId, It.IsAny<string[]>(), null))
                .Returns(expectedNoteId);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            long result = await sut.AddNoteAsync(note, CancellationToken.None);

            // Assert
            result.Should().Be(expectedNoteId);
            ankiContentApiMock.Verify(
                x => x.AddNote(expectedModelId, expectedDeckId, It.Is<string[]>(f => f[0] == "Front text" && f[1] == backWithUpdatedImageTags), null),
                Times.Once);
        }

        #endregion

        #region Tests for SaveImagesAsync

        [TestMethod]
        public async Task SaveImagesAsync_WhenImageFilesIsEmpty_DoesNotCallAnyServices()
        {
            // Arrange
            var imageFiles = Enumerable.Empty<AnkiMedia>();

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            // Assert
            saveImageFileServiceMock.Verify(
                x => x.DownloadAndResizeImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync(It.IsAny<string>(), It.IsAny<Stream>()),
                Times.Never);
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenSingleImageFile_DownloadsAndSavesToAnki()
        {
            // Arrange
            const string fileName = "test_image.jpg";
            const string imageUrl = "https://example.com/image.jpg";
            var imageFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: imageUrl, Filename: fileName)
            };

            const string ankiImageTag = "<img src=\test_image.jpg_6481766173072004017.jpg\" />";

            using var testStream = new MemoryStream([1, 2, 3]);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.DownloadAndResizeImageAsync(imageUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testStream);

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync(fileName, testStream))
                .ReturnsAsync(ankiImageTag);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<ImageTag> result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            ImageTag imageTag1 = result.First();
            imageTag1.FileName.Should().Be(fileName);
            imageTag1.HtmlTag.Should().Be(ankiImageTag);

            saveImageFileServiceMock.Verify(
                x => x.DownloadAndResizeImageAsync(imageUrl, It.IsAny<CancellationToken>()),
                Times.Once);
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync(fileName, testStream),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenMultipleImageFiles_DownloadsAndSavesAllToAnki()
        {
            // Arrange
            var imageFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: "https://example.com/image1.jpg", Filename: "image1.jpg"),
                new AnkiMedia(Url: "https://example.com/image2.jpg", Filename: "image2.jpg"),
                new AnkiMedia(Url: "https://example.com/image3.jpg", Filename: "image3.jpg"),
            };

            using var stream1 = new MemoryStream([1]);
            using var stream2 = new MemoryStream([2]);
            using var stream3 = new MemoryStream([3]);

            const string ankiImageTag1 = "<img src=\"image1.jpg_6481766173072004017.jpg\" />";
            const string ankiImageTag2 = "<img src=\"image2.jpg_6481766173072004017.jpg\" />";
            const string ankiImageTag3 = "<img src=\"image3.jpg_6481766173072004017.jpg\" />";

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.DownloadAndResizeImageAsync("https://example.com/image1.jpg", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stream1);
            saveImageFileServiceMock
                .Setup(x => x.DownloadAndResizeImageAsync("https://example.com/image2.jpg", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stream2);
            saveImageFileServiceMock
                .Setup(x => x.DownloadAndResizeImageAsync("https://example.com/image3.jpg", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stream3);

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock
                .SetupSequence(x => x.AddImageToAnkiMediaAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(ankiImageTag1)
                .ReturnsAsync(ankiImageTag2)
                .ReturnsAsync(ankiImageTag3);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            IEnumerable<ImageTag> result = await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);

            ImageTag imageTag1 = result.First();
            imageTag1.FileName.Should().Be("image1.jpg");
            imageTag1.HtmlTag.Should().Be(ankiImageTag1);

            ImageTag imageTag2 = result.Skip(1).First();
            imageTag2.FileName.Should().Be("image2.jpg");
            imageTag2.HtmlTag.Should().Be(ankiImageTag2);

            ImageTag imageTag3 = result.Skip(2).First();
            imageTag3.FileName.Should().Be("image3.jpg");
            imageTag3.HtmlTag.Should().Be(ankiImageTag3);

            saveImageFileServiceMock.Verify(
                x => x.DownloadAndResizeImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync("image1.jpg", stream1),
                Times.Once);
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync("image2.jpg", stream2),
                Times.Once);
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync("image3.jpg", stream3),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenDownloadThrows_PropagatesException()
        {
            // Arrange
            var imageFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: "https://example.com/image1.jpg", Filename: "image1.jpg")
            };

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.DownloadAndResizeImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Download failed"));

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Download failed");
        }

        [TestMethod]
        public async Task SaveImagesAsync_WhenAddImageToAnkiThrows_PropagatesException()
        {
            // Arrange
            var imageFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: "https://example.com/image1.jpg", Filename: "image1.jpg")
            };

            using var testStream = new MemoryStream([1, 2, 3]);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.DownloadAndResizeImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testStream);

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                .ThrowsAsync(new InvalidOperationException("Failed to save to Anki media"));

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.SaveImagesAsync(imageFiles, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to save to Anki media");
        }

        #endregion

        #region Tests for SaveSoundAsync

        [TestMethod]
        public async Task SaveSoundAsync_WhenSoundUrlAndWordProvided_DownloadsAndSavesToAnki()
        {
            // Arrange
            const string soundUrl = "https://example.com/sound.mp3";
            const string word = "testword";
            const string expectedFileName = "testword.mp3";
            const string ankiSoundTag = "[sound:testword.mp3_6481766173072004017.mp3]";

            using var testStream = new MemoryStream([1, 2, 3]);

            var soundFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: soundUrl, Filename: word)
            };

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(x => x.DownloadSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testStream);

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(ankiSoundTag);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            SoundTag result = await sut.SaveSoundAsync(soundFiles, It.IsAny<CancellationToken>());

            // Assert
            result.FileName.Should().Be(expectedFileName);
            result.AnkiTag.Should().Be(ankiSoundTag);

            saveSoundFileServiceMock.Verify(
                x => x.DownloadSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>()),
                Times.Once);
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync(expectedFileName, testStream),
                Times.Once);
        }

        [TestMethod]
        public async Task SaveSoundAsync_WhenDownloadThrows_PropagatesException()
        {
            // Arrange
            const string soundUrl = "https://example.com/sound.mp3";

            var soundFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: soundUrl, Filename: "testword.mp3")
            };

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(x => x.DownloadSoundFileAsync(soundUrl, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Download failed"));

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.SaveSoundAsync(soundFiles, It.IsAny<CancellationToken>());

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Download failed");
        }

        [TestMethod]
        public async Task SaveSoundAsync_WhenAddToAnkiMediaThrows_PropagatesException()
        {
            // Arrange
            const string soundUrl = "https://example.com/sound.mp3";
            const string word = "testword";

            using var testStream = new MemoryStream([1, 2, 3]);

            var soundFiles = new List<AnkiMedia>()
            {
                new AnkiMedia(Url: soundUrl, Filename: "testword.mp3")
            };

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(x => x.DownloadSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testStream);

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync(It.IsAny<string>(), It.IsAny<Stream>()))
                .ThrowsAsync(new InvalidOperationException("Failed to save to Anki media"));

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            Func<Task> act = async () => await sut.SaveSoundAsync(soundFiles, It.IsAny<CancellationToken>());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to save to Anki media");
        }

        [TestMethod]
        public async Task SaveSoundAsync_GeneratesCorrectFileName()
        {
            // Arrange
            const string soundUrl = "https://example.com/sound.mp3";
            const string word = "danish_word";
            const string expectedFileName = "danish_word.mp3";
            const string ankiSoundTag = "[sound:danish_word.mp3]";

            var soundFiles = new List<AnkiMedia>()
            {
                // We pass Word in the Filename property
                new AnkiMedia(Url: soundUrl, Filename: word)
            };

            using var testStream = new MemoryStream([1, 2, 3]);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock
                .Setup(x => x.DownloadSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testStream);

            var ankiContentApiMock = _fixture.Freeze<Mock<IAnkiContentApi>>();
            ankiContentApiMock
                .Setup(x => x.AddImageToAnkiMediaAsync(expectedFileName, It.IsAny<Stream>()))
                .ReturnsAsync(ankiSoundTag);

            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            await sut.SaveSoundAsync(soundFiles, It.IsAny<CancellationToken>());

            // Assert
            ankiContentApiMock.Verify(
                x => x.AddImageToAnkiMediaAsync(expectedFileName, It.IsAny<Stream>()),
                Times.Once);
        }

        #endregion
    }
}
