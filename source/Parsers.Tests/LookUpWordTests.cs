using System.Text;
using AutoFixture;
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
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

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
            var sut = _fixture.Create<LookUpWord>();

            (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid(lookUp);

            isValid.Should().BeFalse("error: " + errorMessage);
            errorMessage.Should().Be("LookUp text cannot be null or empty.");
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForUrl_ReturnsFalse()
        {
            var sut = _fixture.Create<LookUpWord>();

            (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("http://ordnet.dk/ddo/ordbog");

            isValid.Should().BeFalse("error: " + errorMessage);
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForQuote_ReturnsFalse()
        {
            var sut = _fixture.Create<LookUpWord>();

            (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("ordbo'g");

            isValid.Should().BeFalse("error: " + errorMessage);
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForWord_ReturnsTrue()
        {
            var sut = _fixture.Create<LookUpWord>();

            (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("refusionsopgørelse");

            isValid.Should().BeTrue("error: " + errorMessage);
        }

        [TestMethod]
        public void CheckThatWordIsValid_ForTwoWords_ReturnsTrue()
        {
            var sut = _fixture.Create<LookUpWord>();

            (bool isValid, string? errorMessage) = sut.CheckThatWordIsValid("rindende vand");

            isValid.Should().BeTrue("error: " + errorMessage);
        }

        #endregion

        #region Tests for LookUpWordAsync

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("æø")]
        public void LookUpWordAsync_WhenWordIsNotValid_ThrowsException(string wordToLookup)
        {
            var sut = _fixture.Create<LookUpWord>();

            _ = sut.Invoking(y => y.LookUpWordAsync("Hello", new Options(SourceLanguage.Danish, TranslatorApiURL: null)))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenSourceLanguageIsDanish_CallsDDOPageParser()
        {
            string wordToLookup = "haj";
            SourceLanguage sourceLanguage = SourceLanguage.Danish;

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            var sut = _fixture.Create<LookUpWord>();
            WordModel? result = await sut.LookUpWordAsync(wordToLookup, new Options(sourceLanguage, TranslatorApiURL: null));

            result.Should().NotBeNull();
            ddoPageParserMock.Verify(x => x.ParseHeadword());
        }

        [TestMethod]
        public void LookUpWordAsync_WhenSourceLanguageIsSpanish_CallsSpanishDictPageParser()
        {
            string wordToLookup = "ser";
            SourceLanguage sourceLanguage = SourceLanguage.Spanish;

            Mock<ISpanishDictPageParser> spanishDictPageParserMock = _fixture.Freeze<Mock<ISpanishDictPageParser>>();

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("ser.html");

            var sut = _fixture.Create<LookUpWord>();
            _ = sut.Invoking(y => y.LookUpWordAsync(wordToLookup, new Options(sourceLanguage, TranslatorApiURL: null)))
                .Should().ThrowAsync<ArgumentException>();

            /*WordModel? result = await sut.LookUpWordAsync(wordToLookup, new Options(sourceLanguage, TranslatorApiURL: null));

            result.Should().NotBeNull();
            spanishDictPageParserMock.Verify(x => x.ParseHeadword(It.IsAny<Models.SpanishDict.WordJsonModel>()));*/
        }

        [TestMethod]
        public async Task LookUpWordAsync_Should_DownloadPageAndCallParser()
        {
            const string headWord = "haj";
            const string partOfSpeech = "substantiv, fælleskøn";
            const string endings = "-en, -er, -erne";
            const string soundUrl = "https://static.ordnet.dk/mp3/11019/11019539_1.mp3";

            var definition1 = new Models.DDO.DDODefinition(Position: 1, Enumerable.Empty<Meaning>());
            var definition2 = new Models.DDO.DDODefinition(Position: 2, Enumerable.Empty<Meaning>());
            var definition3 = new Models.DDO.DDODefinition(Position: 3, Enumerable.Empty<Meaning>());
            var definitions = new List<Models.DDO.DDODefinition>() { definition1, definition2, definition3 };

            var variants = new List<Variant>() { new Variant("haj", "https://ordnet.dk/ddo/ordbog?select=haj&query=haj") };

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseHeadword()).Returns(headWord);
            ddoPageParserMock.Setup(x => x.ParsePartOfSpeech()).Returns(partOfSpeech);
            ddoPageParserMock.Setup(x => x.ParseEndings()).Returns(endings);
            ddoPageParserMock.Setup(x => x.ParseSound()).Returns(soundUrl);
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(definitions);
            ddoPageParserMock.Setup(x => x.ParseVariants()).Returns(variants);

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.LookUpWordAsync("haj", new Options(SourceLanguage.Danish, TranslatorApiURL: null));

            result.Should().NotBeNull();
            result!.Word.Should().Be(headWord);
            result!.SoundUrl.Should().Be(soundUrl);
            result!.SoundFileName.Should().Be("haj.mp3");
            result!.Definitions.Should().HaveCount(3);
            result!.Variations.Should().HaveCount(1);

            fileDownloaderMock.Verify(x => x.DownloadPageAsync(It.Is<string>(str => str.EndsWith($"?query={headWord}&search=S%C3%B8g")), Encoding.UTF8));
        }

        #endregion

        #region Tests for GetWordByUrlAsync

        [TestMethod]
        public void GetWordByUrlAsync_WhenSourceLanguageIsNotDanish_ThrowsException()
        {
            string url = _fixture.Create<string>();

            var sut = _fixture.Create<LookUpWord>();

            _ = sut.Invoking(x => x.GetWordByUrlAsync(url, new Options(SourceLanguage.Spanish, TranslatorApiURL: null)))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task GetWordByUrlAsync_Should_DownloadPageAndCallParser()
        {
            string url = _fixture.Create<string>();

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.GetWordByUrlAsync(url, new Options(SourceLanguage.Danish, TranslatorApiURL: null));

            result.Should().NotBeNull();

            fileDownloaderMock.Verify(x => x.DownloadPageAsync(url, Encoding.UTF8));

            ddoPageParserMock.Verify(x => x.LoadHtml(It.IsAny<string>()));
            ddoPageParserMock.Verify(x => x.ParseHeadword());
            ddoPageParserMock.Verify(x => x.ParseSound());
            ddoPageParserMock.Verify(x => x.ParseDefinitions());
            ddoPageParserMock.Verify(x => x.ParseVariants());
        }

        [TestMethod]
        public async Task GetWordByUrlAsync_WhenSoundUrlIsEmpty_SetsSoundFileNameToEmptyString()
        {
            string url = _fixture.Create<string>();
            string headWord = _fixture.Create<string>();

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("i forb. med.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseHeadword()).Returns(headWord);
            ddoPageParserMock.Setup(x => x.ParseSound()).Returns(string.Empty);

            var sut = _fixture.Create<LookUpWord>();
            WordModel? result = await sut.GetWordByUrlAsync(url, new Options(SourceLanguage.Danish, TranslatorApiURL: null));

            result.Should().NotBeNull();
            result!.SoundFileName.Should().BeEmpty();
            result!.SoundUrl.Should().BeEmpty();

            fileDownloaderMock.Verify(x => x.DownloadPageAsync(url, Encoding.UTF8));

            ddoPageParserMock.Verify(x => x.ParseSound());
        }

        [TestMethod]
        public async Task GetWordByUrlAsync_WhenTranslatorAPIUrlIsPassed_CallsTranslatorApi()
        {
            string ddoUrl = _fixture.Create<string>();
            string translatorApiUrl = "http://localhost:7014/api/Translate";
            string headWord = _fixture.Create<string>();
            var definitions = _fixture.Create<List<Models.DDO.DDODefinition>>();

            var translations = new List<TranslationOutput>()
            {
                new TranslationOutput("ru", "акула", new List<string>() { "крупная вытянутая хрящевая рыба с поперечным ртом на нижней стороне головы" })
            };

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseHeadword()).Returns(headWord);
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(definitions);

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translations);

            Options options = new Options(SourceLanguage.Danish, translatorApiUrl);

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.GetWordByUrlAsync(ddoUrl, options);

            result.Should().NotBeNull();
            result!.Definitions.First().Headword.Russian.Should().Be("акула");

            fileDownloaderMock.Verify(x => x.DownloadPageAsync(ddoUrl, Encoding.UTF8));

            ddoPageParserMock.Verify(x => x.LoadHtml(It.IsAny<string>()));

            translatorAPIClientMock.Verify(x => x.TranslateAsync(translatorApiUrl, It.Is<TranslationInput>(i => i.HeadWord == headWord)));
        }

        #endregion

        #region Tests for GetTranslationAsync

        [TestMethod]
        public async Task GetTranslationAsync_WhenTranslatorAPIUrlIsPassed_CallsTranslatorApi()
        {
            string translatorApiUrl = "http://localhost:7014/api/Translate";
            string headWord = _fixture.Create<string>();
            var meanings = _fixture.Create<List<string>>();

            var translations = new List<TranslationOutput>()
            {
                new TranslationOutput("ru", "акула", new List<string>() { "крупная вытянутая хрящевая рыба с поперечным ртом на нижней стороне головы" })
            };

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translations);

            var sut = _fixture.Create<LookUpWord>();
            IEnumerable<TranslationOutput>? result = await sut.GetTranslationAsync(translatorApiUrl, headWord, meanings);

            result.Should().HaveCount(1);

            TranslationOutput translationRU = result.Single();
            translationRU.Language.Should().Be("ru");
            translationRU.HeadWord.Should().Be("акула");
        }

        [TestMethod]
        public async Task GetTranslationAsync_WhenTranslatorAPIUrlIsPassedAndMeaningsIsEmpty_CallsTranslatorApi()
        {
            string translatorApiUrl = "http://localhost:7014/api/Translate";
            string headWord = _fixture.Create<string>();
            var meanings = Enumerable.Empty<string>();

            var translations = new List<TranslationOutput>()
            {
                new TranslationOutput("ru", "акула", new List<string>() { "крупная вытянутая хрящевая рыба с поперечным ртом на нижней стороне головы" })
            };

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translations);

            var sut = _fixture.Create<LookUpWord>();
            IEnumerable<TranslationOutput>? result = await sut.GetTranslationAsync(translatorApiUrl, headWord, meanings);

            result.Should().HaveCount(1);

            TranslationOutput translationRU = result.Single();
            translationRU.Language.Should().Be("ru");
            translationRU.HeadWord.Should().Be("акула");

            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()));
        }

        [TestMethod]
        public async Task GetTranslationAsync_WhenTranslatorAPIUrlIsNull_ReturnsEmpty()
        {
            const string? translatorApiUrl = null;
            string headWord = _fixture.Create<string>();
            List<string> meanings = _fixture.Create<List<string>>();

            var translations = new List<TranslationOutput>()
            {
                new TranslationOutput("ru", "акула", new List<string>() { "крупная вытянутая хрящевая рыба с поперечным ртом на нижней стороне головы" })
            };

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();

            var sut = _fixture.Create<LookUpWord>();
            IEnumerable<TranslationOutput>? result = await sut.GetTranslationAsync(translatorApiUrl, headWord, meanings);

            result.Should().HaveCount(0);

            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()), Times.Never);
        }

        #endregion
    }
}
