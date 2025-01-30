// Ignore Spelling: Dict Api Afeitar App

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

            _ = sut.Invoking(y => y.LookUpWordAsync("Hello", new Options(SourceLanguage.Danish, TranslatorApiURL: null, TranslateMeanings: false)))
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
            WordModel? result = await sut.LookUpWordAsync(wordToLookup, new Options(sourceLanguage, TranslatorApiURL: null, TranslateMeanings: false));

            result.Should().NotBeNull();
            ddoPageParserMock.Verify(x => x.ParseHeadword());
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenSourceLanguageIsSpanish_CallsSpanishDictPageParser()
        {
            string wordToLookup = "ser";
            SourceLanguage sourceLanguage = SourceLanguage.Spanish;

            Mock<ISpanishDictPageParser> spanishDictPageParserMock = _fixture.Freeze<Mock<ISpanishDictPageParser>>();

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("ser.html");

            var sut = _fixture.Create<LookUpWord>();
            _ = sut.Invoking(y => y.LookUpWordAsync(wordToLookup, new Options(sourceLanguage, TranslatorApiURL: null, TranslateMeanings: false)))
                .Should().ThrowAsync<ArgumentException>();

            WordModel? result = await sut.LookUpWordAsync(wordToLookup, new Options(sourceLanguage, TranslatorApiURL: null, TranslateMeanings: false));

            result.Should().NotBeNull();
            spanishDictPageParserMock.Verify(x => x.ParseHeadword(It.IsAny<Models.SpanishDict.WordJsonModel>()));
        }

        [TestMethod]
        public async Task LookUpWordAsync_Should_DownloadPageAndCallParser()
        {
            const string headWord = "haj";
            const string partOfSpeech = "substantiv, fælleskøn";
            const string endings = "-en, -er, -erne";
            const string soundUrl = "https://static.ordnet.dk/mp3/11019/11019539_1.mp3";

            var definition1 = new Models.DDO.DDODefinition("stor, langstrakt bruskfisk", Tag: null, Enumerable.Empty<Example>());
            var definition2 = new Models.DDO.DDODefinition("grisk, skrupelløs person", Tag: "slang", Enumerable.Empty<Example>());
            var definition3 = new Models.DDO.DDODefinition("person der er særlig dygtig til et spil", Tag: "slang", Enumerable.Empty<Example>());
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

            WordModel? result = await sut.LookUpWordAsync("haj", new Options(SourceLanguage.Danish, TranslatorApiURL: null, TranslateMeanings: false));

            result.Should().NotBeNull();
            result!.Word.Should().Be(headWord);
            result!.SoundUrl.Should().Be(soundUrl);
            result!.SoundFileName.Should().Be("haj.mp3");

            // For DDO, we create one Definition with one Context and several Meanings.
            result!.Definitions.Should().HaveCount(1);
            var definition = result.Definitions.First();
            definition.Contexts.Should().HaveCount(1);
            var context = definition.Contexts.First();
            context.Meanings.Should().HaveCount(3);

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

            _ = sut.Invoking(x => x.GetWordByUrlAsync(url, new Options(SourceLanguage.Spanish, TranslatorApiURL: null, TranslateMeanings: false)))
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

            WordModel? result = await sut.GetWordByUrlAsync(url, new Options(SourceLanguage.Danish, TranslatorApiURL: null, TranslateMeanings: false));

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
            WordModel? result = await sut.GetWordByUrlAsync(url, new Options(SourceLanguage.Danish, TranslatorApiURL: null, TranslateMeanings: false));

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

            var translationOutput = new TranslationOutput(
                [
                    new TranslationItem("ru", [ "акула" ])
                ]);

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseHeadword()).Returns(headWord);
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(definitions);

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translationOutput);

            Options options = new Options(SourceLanguage.Danish, translatorApiUrl, TranslateMeanings: false);

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.GetWordByUrlAsync(ddoUrl, options);

            result.Should().NotBeNull();
            result!.Definitions.First().Headword.Russian.Should().Be("акула");

            fileDownloaderMock.Verify(x => x.DownloadPageAsync(ddoUrl, Encoding.UTF8));

            ddoPageParserMock.Verify(x => x.LoadHtml(It.IsAny<string>()));

            translatorAPIClientMock.Verify(x => x.TranslateAsync(translatorApiUrl, It.Is<TranslationInput>(i => i.Word == headWord)));
        }

        #endregion

        #region Tests for TranslateMeaningAsync

        [TestMethod]
        public async Task TranslateMeaningAsync_Should_ReturnRussianTranslation()
        {
            string translatorApiUrl = "http://localhost:7014/api/Translate";
            const string sourceLanguage = "da";
            const string meaning = "stor, langstrakt bruskfisk";

            var translationOutput = new TranslationOutput(
                [
                    new TranslationItem("ru", [ "крупная, удлиненная хрящевая рыба" ]),
                    new TranslationItem("en", [ "large, elongated cartilaginous fish" ])
                ]);

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translationOutput);

            var sut = _fixture.Create<LookUpWord>();
            string? result = await sut.TranslateMeaningAsync(translatorApiUrl, sourceLanguage, meaning, examples: Enumerable.Empty<string>());

            result.Should().Be("крупная, удлиненная хрящевая рыба");

            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()));
        }

        #endregion

        #region Tests for GetTranslationAsync

        [TestMethod]
        public async Task GetTranslationAsync_WhenTranslatorAPIUrlIsPassed_CallsTranslatorApi()
        {
            string translatorApiUrl = "http://localhost:7014/api/Translate";
            string headWord = _fixture.Create<string>();
            string meaning = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();

            var translationOutput = new TranslationOutput(
                [
                    new TranslationItem("ru", [ "акула" ])
                ]);

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translationOutput);

            var sut = _fixture.Create<LookUpWord>();
            TranslationOutput? result = await sut.GetTranslationAsync(translatorApiUrl, sourceLanguage: "da", headWord, meaning, partOfSpeech, examples: Enumerable.Empty<string>());

            result.Translations.Should().HaveCount(1);

            TranslationItem translationRU = result.Translations.Single();
            translationRU.Language.Should().Be("ru");
            translationRU.TranslationVariants.Should().HaveCount(1);
            translationRU.TranslationVariants.First().Should().Be("акула");
        }

        [TestMethod]
        public async Task GetTranslationAsync_WhenTranslatorAPIUrlIsNull_ReturnsEmpty()
        {
            const string? translatorApiUrl = null;
            string headWord = _fixture.Create<string>();
            string meaning = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();

            var sut = _fixture.Create<LookUpWord>();
            TranslationOutput? result = await sut.GetTranslationAsync(translatorApiUrl, sourceLanguage: "da", headWord, meaning, partOfSpeech, examples: Enumerable.Empty<string>());

            result.Translations.Should().HaveCount(0);

            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()), Times.Never);
        }

        #endregion

        #region Tests for ParseDanishWordAsync

        [TestMethod]
        public async Task ParseDanishWordAsync_Should_ReturnOneDefinitionWithOneContextAndSeveralMeanings()
        {
            string html = _fixture.Create<string>();
            var options = new Options(SourceLanguage.Danish, TranslatorApiURL: null, TranslateMeanings: false);

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(CreateDefinitionsForHaj());

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.ParseDanishWordAsync(html, options);

            result.Should().NotBeNull();

            IEnumerable<Definition> definitions = result!.Definitions;
            definitions.Should().HaveCount(1);

            // For DDO, we create one Definition with one Context and several Meanings.
            Definition definition1 = definitions.First();
            definition1.Contexts.Should().HaveCount(1);
            Context context1 = definition1.Contexts.First();
            context1.Meanings.Should().HaveCount(3);

            Meaning meaning1 = context1.Meanings.First();
            meaning1.Original.Should().Be("stor, langstrakt bruskfisk");
            meaning1.AlphabeticalPosition.Should().Be("1");
            meaning1.Tag.Should().BeNull();
            meaning1.ImageUrl.Should().BeNull();
            meaning1.Examples.Should().HaveCount(1);
            Example example1 = meaning1!.Examples.First();
            example1.Original.Should().Be("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham");

            Meaning meaning2 = context1.Meanings.Skip(1).First();
            meaning2.Original.Should().Be("grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning");
            meaning2.AlphabeticalPosition.Should().Be("2");
            meaning2.Tag.Should().Be("SLANG");
            meaning2.ImageUrl.Should().BeNull();
            meaning2.Examples.Should().HaveCount(1);
            Example example2 = meaning2!.Examples.First();
            example2.Original.Should().Be("-");

            Meaning meaning3 = context1.Meanings.Skip(2).First();
            meaning3.Original.Should().Be("person der er særlig dygtig til et spil, håndværk el.lign.");
            meaning3.AlphabeticalPosition.Should().Be("3");
            meaning3.Tag.Should().Be("SLANG");
            meaning3.ImageUrl.Should().BeNull();
            meaning3.Examples.Should().HaveCount(1);
            Example example3 = meaning3!.Examples.First();
            example3.Original.Should().Be("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb");

            ddoPageParserMock.Verify(x => x.LoadHtml(It.IsAny<string>()));
            ddoPageParserMock.Verify(x => x.ParseHeadword());
            ddoPageParserMock.Verify(x => x.ParseSound());
            ddoPageParserMock.Verify(x => x.ParseDefinitions());
            ddoPageParserMock.Verify(x => x.ParseVariants());
        }

        [TestMethod]
        public async Task ParseDanishWordAsync_WhenTranslateMeaningsIsTrue_CallsTranslationAppForHeadwordAndMeanings()
        {
            const bool translateMeanings = true;

            string html = _fixture.Create<string>();
            var options = new Options(SourceLanguage.Danish, TranslatorApiURL: _fixture.Create<Uri>().ToString(), translateMeanings);

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(CreateDefinitionsForHaj());

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock
                .Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()))
                .ReturnsAsync(_fixture.Create<TranslationOutput>());

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.ParseDanishWordAsync(html, options);

            result.Should().NotBeNull();

            IEnumerable<Definition> definitions = result!.Definitions;
            definitions.Should().HaveCount(1);

            // For DDO, we create one Definition with one Context and several Meanings.
            Definition definition1 = definitions.First();
            definition1.Contexts.Should().HaveCount(1);
            Context context1 = definition1.Contexts.First();
            context1.Meanings.Should().HaveCount(3);

            // We should call Translator app once for the headword and three times for the meanings.
            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()), Times.Exactly(4));
        }

        [TestMethod]
        public async Task ParseDanishWordAsync_WhenTranslateMeaningsIsFalse_CallsTranslationAppOnlyForHeadword()
        {
            const bool translateMeanings = false;

            string html = _fixture.Create<string>();
            var options = new Options(SourceLanguage.Danish, TranslatorApiURL: _fixture.Create<Uri>().ToString(), translateMeanings);

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(CreateDefinitionsForHaj());

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock
                .Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()))
                .ReturnsAsync(_fixture.Create<TranslationOutput>());

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.ParseDanishWordAsync(html, options);

            result.Should().NotBeNull();

            IEnumerable<Definition> definitions = result!.Definitions;
            definitions.Should().HaveCount(1);

            // For DDO, we create one Definition with one Context and several Meanings.
            Definition definition1 = definitions.First();
            definition1.Contexts.Should().HaveCount(1);
            Context context1 = definition1.Contexts.First();
            context1.Meanings.Should().HaveCount(3);

            // We should call Translator app once for the headword and three times for the meanings.
            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task ParseDanishWordAsync_Should_CallTranslationAppMax6Times()
        {
            // some words have a lot of meanings, we will only translate first 5 meanings + the headword.
            var ddoDefinitions = new List<Models.DDO.DDODefinition>()
            {
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>(),
                _fixture.Create<Models.DDO.DDODefinition>()
            };
            ddoDefinitions.Should().HaveCount(10);

            string html = _fixture.Create<string>();
            var options = new Options(SourceLanguage.Danish, TranslatorApiURL: _fixture.Create<Uri>().ToString(), TranslateMeanings: true);

            Mock<IFileDownloader> fileDownloaderMock = _fixture.Freeze<Mock<IFileDownloader>>();
            fileDownloaderMock.Setup(x => x.DownloadPageAsync(It.IsAny<string>(), Encoding.UTF8)).ReturnsAsync("haj.html");

            Mock<IDDOPageParser> ddoPageParserMock = _fixture.Freeze<Mock<IDDOPageParser>>();
            ddoPageParserMock.Setup(x => x.ParseDefinitions()).Returns(ddoDefinitions);

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock
                .Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()))
                .ReturnsAsync(_fixture.Create<TranslationOutput>());

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.ParseDanishWordAsync(html, options);

            result.Should().NotBeNull();

            IEnumerable<Definition> definitions = result!.Definitions;
            definitions.Should().HaveCount(1);

            // For DDO, we create one Definition with one Context and several Meanings.
            Definition definition1 = definitions.First();
            definition1.Contexts.Should().HaveCount(1);
            Context context1 = definition1.Contexts.First();
            context1.Meanings.Should().HaveCount(10);

            // We should call Translator app once for the headword and three times for the meanings.
            translatorAPIClientMock.Verify(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>()), Times.Exactly(6));
        }

        #endregion

        #region Tests for ParseSpanishWordAsync

        [TestMethod]
        public async Task ParseSpanishWordAsync_Should_Return2MeaningsForAfeitar()
        {
            string headwordES = "afeitar";
            string html = _fixture.Create<string>();
            var options = new Options(SourceLanguage.Spanish, TranslatorApiURL: null, TranslateMeanings: false);

            Mock<ISpanishDictPageParser> spanishDictPageParserMock = _fixture.Freeze<Mock<ISpanishDictPageParser>>();
            spanishDictPageParserMock.Setup(x => x.ParseHeadword(It.IsAny<Models.SpanishDict.WordJsonModel>())).Returns(headwordES);
            spanishDictPageParserMock.Setup(x => x.ParseDefinitions(It.IsAny<Models.SpanishDict.WordJsonModel>())).Returns(CreateDefinitionsForAfeitar());

            var sut = _fixture.Create<LookUpWord>();

            WordModel? result = await sut.ParseSpanishWordAsync(html, options);

            result.Should().NotBeNull();

            result!.Word.Should().Be(headwordES);

            IEnumerable<Definition> definitions = result!.Definitions;
            definitions.Should().HaveCount(2);

            // 1. afeitar
            Definition definition1 = definitions.First();
            definition1.Headword.Original.Should().Be("afeitar");
            definition1.Contexts.Should().HaveCount(1);
            definition1.PartOfSpeech.Should().Be("TRANSITIVE VERB");
            Context context1 = definition1.Contexts.First();
            context1.ContextEN.Should().Be("(to remove hair)");
            context1.Position.Should().Be("1");
            context1.Meanings.Should().HaveCount(1);

            Meaning meaning1 = context1.Meanings.First();
            meaning1.Original.Should().Be("to shave");
            meaning1.AlphabeticalPosition.Should().Be("a");
            meaning1.Tag.Should().BeNull();
            meaning1.ImageUrl.Should().BeNull();
            meaning1.Examples.Should().HaveCount(1);
            Example example1 = meaning1!.Examples.First();
            example1.Original.Should().Be("Para el verano, papá decidió afeitar al perro.");
            example1.Translation.Should().Be("For the summer, dad decided to shave the dog.");

            // 2. afeitarse
            Definition definition2 = definitions.Skip(1).First();
            definition2.Headword.Original.Should().Be("afeitarse");
            definition2.PartOfSpeech.Should().Be("REFLEXIVE VERB");
            definition2.Contexts.Should().HaveCount(1);
            context1 = definition2.Contexts.First();
            context1.Position.Should().Be("1");
            context1.Meanings.Should().HaveCount(1);

            meaning1 = context1.Meanings.First();
            meaning1.Original.Should().Be("to shave");
            meaning1.AlphabeticalPosition.Should().Be("a");
            meaning1.Tag.Should().BeNull();
            meaning1.ImageUrl.Should().BeNull();
            meaning1.Examples.Should().HaveCount(1);
            example1 = meaning1!.Examples.First();
            example1.Original.Should().Be("¿Con qué frecuencia te afeitas la barba?");
            example1.Translation.Should().Be("How often do you shave your beard?");

            spanishDictPageParserMock.Verify(x => x.ParseWordJson(html));
            spanishDictPageParserMock.Verify(x => x.ParseHeadword(It.IsAny<Models.SpanishDict.WordJsonModel>()));
            spanishDictPageParserMock.Verify(x => x.ParseSoundURL(It.IsAny<Models.SpanishDict.WordJsonModel>()));
            spanishDictPageParserMock.Verify(x => x.ParseDefinitions(It.IsAny<Models.SpanishDict.WordJsonModel>()));
        }

        [TestMethod]
        public async Task ParseSpanishWordAsync_Should_CallTranslationService()
        {
            string headwordES = "casa";
            string html = _fixture.Create<string>();

            string translationApiURL = _fixture.Create<Uri>().ToString();
            var options = new Options(SourceLanguage.Spanish, TranslatorApiURL: translationApiURL, TranslateMeanings: false);
            var translationOutput = new TranslationOutput(
                [
                    new TranslationItem("ru", [ "дом" ]),
                    new TranslationItem("en", [ "house", "household" ])
                ]);

            Mock<ISpanishDictPageParser> spanishDictPageParserMock = _fixture.Freeze<Mock<ISpanishDictPageParser>>();
            spanishDictPageParserMock.Setup(x => x.ParseHeadword(It.IsAny<Models.SpanishDict.WordJsonModel>())).Returns(headwordES);
            spanishDictPageParserMock.Setup(x => x.ParseDefinitions(It.IsAny<Models.SpanishDict.WordJsonModel>())).Returns(CreateDefinitionsForCasa());

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();
            translatorAPIClientMock.Setup(x => x.TranslateAsync(It.IsAny<string>(), It.IsAny<TranslationInput>())).ReturnsAsync(translationOutput);

            var sut = _fixture.Create<LookUpWord>();
            WordModel? result = await sut.ParseSpanishWordAsync(html, options);

            result.Should().NotBeNull();

            result!.Word.Should().Be(headwordES);

            IEnumerable<Definition> definitions = result!.Definitions;
            definitions.Should().HaveCount(1);

            // 1. house (dwelling)
            Definition definition1 = definitions.First();
            definition1.Headword.Original.Should().Be("casa");
            definition1.Headword.Russian.Should().Be("дом");
            definition1.Headword.English.Should().Be("house, household");

            // For Spanish words, we only translate headword but don't translate meanings.
            translatorAPIClientMock.Verify(x => x.TranslateAsync(translationApiURL, It.Is<TranslationInput>(input =>
                input.SourceLanguage == "es"
                && input.DestinationLanguages.Count() == 2
                && input.Word == headwordES
                && input.Meaning == "house (dwelling)"
                && input.PartOfSpeech == "feminine noun"
                && input.Examples.Count() == 1 && input.Examples.First() == "Vivimos en una casa con un gran jardín.")),
                Times.Once());
        }

        #endregion

        #region Private Methods

        private static List<Models.DDO.DDODefinition> CreateDefinitionsForHaj()
        {
            var definition1 = new Models.DDO.DDODefinition(Meaning: "stor, langstrakt bruskfisk", Tag: null, new List<Example>()
                {
                    new Example(Original: "Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham", Translation: null)
                });

            var definition2 = new Models.DDO.DDODefinition(Meaning: "grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning", Tag: "SLANG", Examples: new List<Example>()
                {
                    new Example(Original: "-", Translation: null)
                });

            var definition3 = new Models.DDO.DDODefinition(Meaning: "person der er særlig dygtig til et spil, håndværk el.lign.", Tag: "SLANG", Examples: new List<Example>()
                {
                    new Example(Original : "Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb", Translation: null)
                });

            return new List<Models.DDO.DDODefinition>() { definition1, definition2, definition3 };
        }

        private static List<Models.SpanishDict.SpanishDictDefinition> CreateDefinitionsForAfeitar()
        {
            var definition1 = new Models.SpanishDict.SpanishDictDefinition(WordES: "afeitar", PartOfSpeech: "TRANSITIVE VERB",
                new List<Models.SpanishDict.SpanishDictContext>
                {
                    new Models.SpanishDict.SpanishDictContext(ContextEN: "(to remove hair)", Position: 1,
                        new List<Models.SpanishDict.Meaning>
                        {
                            new Models.SpanishDict.Meaning(Original: "to shave", AlphabeticalPosition: "a", ImageUrl: null,
                                new List<Example>() { new Example(Original: "Para el verano, papá decidió afeitar al perro.", Translation: "For the summer, dad decided to shave the dog.") }),
                        }),
                });

            var definition2 = new Models.SpanishDict.SpanishDictDefinition(WordES: "afeitarse", PartOfSpeech: "REFLEXIVE VERB",
                new List<Models.SpanishDict.SpanishDictContext>
                {
                    new Models.SpanishDict.SpanishDictContext(ContextEN: "(to shave oneself)", Position: 1,
                        new List<Models.SpanishDict.Meaning>
                        {
                            new Models.SpanishDict.Meaning(Original: "to shave", AlphabeticalPosition: "a", ImageUrl: null,
                                new List<Example>() { new Example(Original: "¿Con qué frecuencia te afeitas la barba?", Translation: "How often do you shave your beard?") }),
                        }),
                });

            return new List<Models.SpanishDict.SpanishDictDefinition>() { definition1, definition2 };
        }

        private static List<Models.SpanishDict.SpanishDictDefinition> CreateDefinitionsForCasa()
        {
            var definition1 = new Models.SpanishDict.SpanishDictDefinition(WordES: "casa", PartOfSpeech: "feminine noun",
                new List<Models.SpanishDict.SpanishDictContext>
                {
                    new Models.SpanishDict.SpanishDictContext(ContextEN: "(dwelling)", Position: 1,
                        new List<Models.SpanishDict.Meaning>
                        {
                            new Models.SpanishDict.Meaning(Original: "house", AlphabeticalPosition: "a", ImageUrl: null,
                                new List<Example>() { new Example(Original: "Vivimos en una casa con un gran jardín.", Translation: "We live in a house with a big garden.") }),
                        }),
                    new Models.SpanishDict.SpanishDictContext(ContextEN: "(household)", Position: 1,
                        new List<Models.SpanishDict.Meaning>
                        {
                            new Models.SpanishDict.Meaning(Original: "home", AlphabeticalPosition: "a", ImageUrl: null,
                                new List<Example>() { new Example(Original: "Mi casa es donde mi familia y mis amigos están.", Translation: "My home is where my family and friends are.") }),
                        }),
                });

            return new List<Models.SpanishDict.SpanishDictDefinition>() { definition1 };
        }
        #endregion
    }
}
