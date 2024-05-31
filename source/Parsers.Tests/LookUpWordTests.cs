using System.Reflection;
using System.Text;
using Autofac.Extras.Moq;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Models.SpanishDict;
using CopyWords.Parsers.Services;
using FluentAssertions;
using Moq;

namespace CopyWords.Parsers.Tests
{
    [TestClass]
    public class LookUpWordTests
    {
        private static string s_path = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _ = context;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#pragma warning disable CS8601 // Possible null reference assignment.
            s_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        #region Tests for CheckThatWordIsValid

        [TestMethod]
        public void CheckThatWordIsValid_WhenUrl_ReturnsFalse()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("http://ordnet.dk/ddo/ordbog");

                isValid.Should().BeFalse("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_WhenHasQuote_ReturnsFalse()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("ordbo'g");

                isValid.Should().BeFalse("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_WhenValidWord_ReturnsTrue()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("aprovechar");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_WhenHasAccentedSymbols_ReturnsTrue()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("águila");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_WhenTwoWords_ReturnsTrue()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("rindende vand");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        #endregion

        #region Tests for ParseWordJson

        [DataTestMethod]
        [DataRow("afeitar")]
        [DataRow("águila")]
        [DataRow("aprovechar")]
        [DataRow("costar un ojo de la cara")]
        public void ParseWordJson_WhenCanParseHtml_ReturnsModel(string word)
        {
            string htmlPage = LoadHtml(word);

            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                WordJsonModel? result = sut.ParseWordJson(htmlPage);

                result.Should().NotBeNull();
                result!
                    .resultCardHeaderProps
                    .headwordAndQuickdefsProps
                    .headword
                    .displayText.Should().Be(word);
            }
        }

        [TestMethod]
        public void ParseWordJson_WhenCannotParseModel_ReturnsNull()
        {
            const string word = "Udfordring";
            string htmlPage = LoadHtml(word);

            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                WordJsonModel? result = sut.ParseWordJson(htmlPage);

                result.Should().BeNull();
            }
        }

        #endregion

        #region Tests for LookUpWordAsync

        [TestMethod]
        public async Task LookUpWordAsync_WhenWordIsNull_ThrowsException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                Func<Task> act = sut.Awaiting(x => x.LookUpWordAsync(null!));
                await act.Should().ThrowAsync<ArgumentException>().WithMessage("LookUp text cannot be null or empty.");
            }
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenWordIsNotValid_ThrowsException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                Func<Task> act = sut.Awaiting(x => x.LookUpWordAsync("ordbo'g"));
                await act.Should().ThrowAsync<ArgumentException>().WithMessage("Search can only contain alphanumeric characters and spaces.");
            }
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenCannotDownloadHtmlPage_ReturnsNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                WordModel? wordModel = await sut.LookUpWordAsync("test");

                wordModel.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenCanNotParseHtmlPage_ReturnsNull()
        {
            const string htmlPageContent = "<html><script>window.SD_COMPONENT_DATA={}</script>some page</html>";

            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IFileDownloader>().Setup(x => x.DownloadPageAsync(It.IsAny<string>(), It.IsAny<Encoding>())).ReturnsAsync(htmlPageContent);

                var sut = mock.Create<LookUpWord>();

                WordModel? wordModel = await sut.LookUpWordAsync("test");

                wordModel.Should().BeNull();

                mock.Mock<IFileDownloader>().Verify(x => x.DownloadPageAsync(It.IsAny<string>(), It.IsAny<Encoding>()));
            }
        }

        #endregion

        #region Private methods

        private static string LoadHtml(string word)
        {
            string htmlPagePath = Path.Combine(s_path, "TestPages", $"{word}.html");
            if (!File.Exists(htmlPagePath))
            {
                throw new Exception($"Cannot find test file '{htmlPagePath}'");
            }

            string json = File.ReadAllText(htmlPagePath);
            return json;
        }

        #endregion
    }
}
