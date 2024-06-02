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

        [TestMethod]
        public void CheckThatWordIsValid_ReturnsFalse_ForUrl()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("http://ordnet.dk/ddo/ordbog");

                isValid.Should().BeFalse("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ReturnsFalse_ForQuote()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("ordbo'g");

                isValid.Should().BeFalse("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ReturnsTrue_ForWord()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("refusionsopgørelse");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        [TestMethod]
        public void CheckThatWordIsValid_ReturnsTrue_ForTwoWord()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var sut = mock.Create<LookUpWord>();

                (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("rindende vand");

                isValid.Should().BeTrue("error: " + errorMessage);
            }
        }

        [DataTestMethod]
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
        }
    }
}
