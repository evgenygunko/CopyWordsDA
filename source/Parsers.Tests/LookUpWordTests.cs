using System.Text;
using Autofac.Extras.Moq;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CopyWords.Parsers.Tests
{
    [TestClass]
    public class LookUpWordTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _ = context;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region Tests for CheckThatWordIsValid

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void CheckThatWordIsValid_WhenLookUpIsNullOrEmpty_ReturnsFalse(string lookUp)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid(lookUp);

                isValid.Should().BeFalse("error: " + errorMessage);
                errorMessage.Should().Be("LookUp text cannot be null or empty.");
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForUrl_ReturnsFalse()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("http://ordnet.dk/ddo/ordbog");

                isValid.Should().BeFalse("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForQuote_ReturnsFalse()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("ordbo'g");

                isValid.Should().BeFalse("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForWord_ReturnsTrue()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("refusionsopgørelse");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForTwoWords_ReturnsTrue()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("rindende vand");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        #endregion

        #region Tests for LookUpWordAsync

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("æø")]
        public void LookUpWordAsync_WhenWordIsNotValid_ThrowsException(string wordToLookup)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                _ = sut.Invoking(y => y.LookUpWordAsync("Hello"))
                    .Should().ThrowAsync<ArgumentException>();
            }
        }

        [TestMethod]
        public async Task LookUpWordAsync_Should_DownloadPageAndCallParser()
        {
            const string headWord = "haj";
            const string soundUrl = "http://test.com/haj.mp3";

            var definition1 = new Definition("stor, langstrakt bruskfisk", Enumerable.Empty<string>());
            var definition2 = new Definition("grisk, skrupelløs person", Enumerable.Empty<string>());
            var definition3 = new Definition("person der er særlig dygtig til et spil", Enumerable.Empty<string>());
            var definitions = new List<Definition>() { definition1, definition2, definition3 };

            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IFileDownloader>().Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

                mock.Mock<IDDOPageParser>().Setup(x => x.ParseHeadword()).Returns(headWord);
                mock.Mock<IDDOPageParser>().Setup(x => x.ParseSound()).Returns(soundUrl);
                mock.Mock<IDDOPageParser>().Setup(x => x.ParseDefinitions()).Returns(definitions);

                var sut = mock.Create<LookUpWord>();

                WordModel? result = await sut.LookUpWordAsync("haj");

                result.Should().NotBeNull();
                result!.Headword.Should().Be(headWord);
                result!.SoundUrl.Should().Be(soundUrl);
                result!.SoundFileName.Should().BeNull();
                result!.Definitions.Should().HaveCount(3);

                mock.Mock<IFileDownloader>().Verify(x => x.DownloadPageAsync(It.Is<string>(str => str.EndsWith($"?query={headWord}&search=S%C3%B8g")), Encoding.UTF8));
            }
        }

        #endregion
    }
}
