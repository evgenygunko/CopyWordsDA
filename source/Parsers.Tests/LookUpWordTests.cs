using System.Text;
using Autofac.Extras.Moq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        // todo: enable or delete after translations is implemented
        /*[DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task LookUpWordAsync_ReturnsVariantUrls(int variationsCount)
        {
            const string wordToLookup = "any word";

            List<VariationUrl> variationUrls = new List<VariationUrl>();

            for (int i = 0; i < variationsCount; i++)
            {
                variationUrls.Add(new VariationUrl(Word: wordToLookup + i, URL: "http://utl_to_lookup" + i));
            }

            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IDDOPageParser>().Setup(x => x.ParseWord()).Returns(wordToLookup);
                mock.Mock<IDDOPageParser>().Setup(x => x.ParseVariationUrls()).Returns(variationUrls);

                mock.Mock<IFileDownloader>().Setup(x => x.DownloadPageAsync(It.IsAny<string>(), It.IsAny<Encoding>())).ReturnsAsync("Some page here");

                var sut = mock.Create<LookUpWord>();

                WordModel? wordModel = await sut.LookUpWordAsync(wordToLookup);

                wordModel!.VariationUrls.Should().HaveCount(variationsCount);
            }
        }*/

        #endregion
    }
}
