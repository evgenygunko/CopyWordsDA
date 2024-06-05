using System.Reflection;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CopyWords.Parsers.Tests
{
    [TestClass]
    public class DDOPageParserTests
    {
        private static string? _path;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _ = context;
            _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        }

        #region LoadHtml tests

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void LoadHtml_WhenStringIsNullOrEmpty_ThrowsException(string content)
        {
            DDOPageParser parser = new DDOPageParser();

            _ = parser.Invoking(x => x.LoadHtml(content))
                    .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void LoadHtml_ForValidString_DoesNotThrowException()
        {
            string content = GetSimpleHTMLPage("SimplePage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);
        }

        #endregion

        #region ParseVariationUrls tests

        [TestMethod]
        public void ParseVariationUrls_ReturnsVariationUrls_ForUnderholdningPage()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<VariationUrl> variationUrls = parser.ParseVariationUrls();
            Assert.AreEqual(1, variationUrls.Count);
            Assert.AreEqual("underholdning sb.", variationUrls[0].Word.ToLower());
            Assert.AreEqual("https://ordnet.dk/ddo/ordbog?select=underholdning&query=underholdning", variationUrls[0].URL.ToLower());
        }

        [TestMethod]
        public void ParseVariationUrls_ReturnsVariationUrls_ForHojPage()
        {
            string content = GetSimpleHTMLPage("HøjPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<VariationUrl> variationUrls = parser.ParseVariationUrls();
            Assert.AreEqual(2, variationUrls.Count);

            Assert.AreEqual("høj(1) sb.", variationUrls[0].Word);
            Assert.AreEqual("https://ordnet.dk/ddo/ordbog?select=høj,1&query=høj", variationUrls[0].URL);

            Assert.AreEqual("høj(2) adj.", variationUrls[1].Word);
            Assert.AreEqual("https://ordnet.dk/ddo/ordbog?select=høj,2&query=høj", variationUrls[1].URL);
        }

        [TestMethod]
        public void ParseVariationUrls_ReturnsVariationUrls_ForSkatPage()
        {
            string content = GetSimpleHTMLPage("SkatPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<VariationUrl> variationUrls = parser.ParseVariationUrls();
            Assert.AreEqual(3, variationUrls.Count);

            Assert.AreEqual("skat sb.", variationUrls[0].Word);
            Assert.AreEqual("https://ordnet.dk/ddo/ordbog?select=skat&query=skat", variationUrls[0].URL);

            Assert.AreEqual("skat -> skatte vb.", variationUrls[1].Word);
            Assert.AreEqual("https://ordnet.dk/ddo/ordbog?select=skatte&query=skat", variationUrls[1].URL);

            Assert.AreEqual("skat -> skate vb.", variationUrls[2].Word);
            Assert.AreEqual("https://ordnet.dk/ddo/ordbog?select=skate&query=skat", variationUrls[2].URL);
        }

        #endregion

        #region ParseHeadword tests

        [TestMethod]
        public void ParseHeadword_ForUnderholdningPage_ReturnsUnderholdning()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            Assert.AreEqual("underholdning", word);
        }

        [TestMethod]
        public void ParseHeadword_ForUGrillspydPage_ReturnsGrillspydPage()
        {
            string content = GetSimpleHTMLPage("GrillspydPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            Assert.AreEqual("grillspyd", word);
        }

        [TestMethod]
        public void ParseHeadword_ForStødtandPage_ReturnsStødtand()
        {
            string content = GetSimpleHTMLPage("StødtandPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            Assert.AreEqual("stødtand", word);
        }

        #endregion

        #region ParseEndings tests

        [TestMethod]
        public void ParseEndings_ReturnsEn_ForUnderholdningPage()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string endings = parser.ParseEndings();
            Assert.AreEqual("-en", endings);
        }

        [TestMethod]
        public void ParseEndings_ReturnsEndings_ForStødtandPage()
        {
            string content = GetSimpleHTMLPage("StødtandPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string endings = parser.ParseEndings();
            Assert.AreEqual("-en, ..tænder, ..tænderne", endings);
        }

        #endregion

        #region ParsePronunciation tests

        [TestMethod]
        public void ParsePronunciation_ReturnsUnderholdning_ForUnderholdningPage()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParsePronunciation();
            Assert.AreEqual("[ˈɔnʌˌhʌlˀneŋ]", pronunciation);
        }

        [TestMethod]
        public void ParsePronunciation_ReturnsKigge_ForKiggePage()
        {
            string content = GetSimpleHTMLPage("KiggePage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParsePronunciation();
            Assert.AreEqual("[ˈkigə]", pronunciation);
        }

        [TestMethod]
        public void ParsePronunciation_ReturnsEmptyString_ForGrillspydPage()
        {
            string content = GetSimpleHTMLPage("GrillspydPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParsePronunciation();
            Assert.AreEqual(string.Empty, pronunciation);
        }

        #endregion

        #region ParseSound tests

        [TestMethod]
        public void ParseSound_ReturnsSoundFile_ForUnderholdningPage()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParseSound();
            Assert.AreEqual("https://static.ordnet.dk/mp3/12004/12004770_1.mp3", pronunciation);
        }

        [TestMethod]
        public void ParseSound_ReturnsEmptyString_ForKiggePage()
        {
            string content = GetSimpleHTMLPage("KiggePage.html");

            // Kigge page doesn't have a sound file
            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string sound = parser.ParseSound();
            Assert.AreEqual(string.Empty, sound);
        }

        [TestMethod]
        public void ParseSound_ForDannebrogPage_ReturnsSoundFile()
        {
            string content = GetSimpleHTMLPage("DannebrogPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string sound = parser.ParseSound();
            sound.Should().Be("https://static.ordnet.dk/mp3/11008/11008357_1.mp3");
        }

        #endregion

        #region ParseDefinitions tests

        [TestMethod]
        public void ParseDefinitions_ForUnderholdningPage_ReturnsOneDefinition()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);
            definitions.First().Meaning.Should().Be("noget der morer, glæder eller adspreder nogen, fx optræden, et lettere og ikke særlig krævende åndsprodukt eller en fornøjelig beskæftigelse");
        }

        [TestMethod]
        public void ParseDefinitions_ForKiggePage_ReturnsSeveralDefinitions()
        {
            string content = GetSimpleHTMLPage("KiggePage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(5);

            definitions.First().Meaning.Should().Be("rette blikket i en bestemt retning");
            definitions.Skip(1).First().Meaning.Should().Be("undersøge nærmere; sætte sig ind i");
            definitions.Skip(2).First().Meaning.Should().Be("prøve at finde");
            definitions.Skip(3).First().Meaning.Should().Be("skrive af efter nogen; kopiere noget");
            definitions.Skip(4).First().Meaning.Should().Be("se på; betragte");
        }

        [TestMethod]
        public void ParseDefinitions_ForGrillspydPage_ReturnsOneDefinition()
        {
            string content = GetSimpleHTMLPage("GrillspydPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            definitions.First().Meaning.Should().Be("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        #endregion

        #region ParseExamples tests

        [TestMethod]
        public void ParseExamples_ForDannebrogPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("DannebrogPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<string> examples = parser.ParseExamples();

            examples.Should().HaveCount(2);
            examples[0].Should().Be("1. For mig er dannebrog noget samlende på tværs af køn, alder, etnicitet, værdier og politisk ståsted.");
            examples[1].Should().Be("2. der var Dannebrog på bordet og balloner og gaver fra gæsterne. Om aftenen var vi alle trætte, men glade for en god fødselsdag.");
        }

        [TestMethod]
        public void ParseExamples_ForStiktossetPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("StiktossetPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<string> examples = parser.ParseExamples();

            examples.Should().HaveCount(2);
            examples[0].Should().Be("1. Det her er så sindssygt, at jeg kan blive stiktosset over det.");
            examples[1].Should().Be("2. Du ved godt, at jeg bliver stiktosset, når du hopper i sofaen.");
        }

        [TestMethod]
        public void ParseExamples_ForUnderholdningPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<string> examples = parser.ParseExamples();

            examples.Should().HaveCount(2);
            examples[0].Should().Be("1. 8000 medarbejdere skal til fest med god mad og underholdning af bl.a. Hans Otto Bisgaard.");
            examples[1].Should().Be("2. vi havde jo ikke radio, TV eller video, så underholdningen bestod mest af kortspil i familien.");
        }

        [TestMethod]
        public void ParseExamples_ForKiggePage_ReturnsMultipleExamples()
        {
            string content = GetSimpleHTMLPage("KiggePage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<string> examples = parser.ParseExamples();

            examples.Should().HaveCount(7);
            examples[0].Should().Be("1. Børnene kiggede spørgende på hinanden.");
            examples[1].Should().Be("2. kig lige en gang!");
            examples[2].Should().Be("3. Han kiggede sig rundt, som om han ledte efter noget.");
            examples[3].Should().Be("4. hun har kigget på de psykiske eftervirkninger hos voldtagne piger og kvinder.");
            examples[4].Should().Be("5. Vi kigger efter en bil i det prislag, og Carinaen opfylder de fleste af de krav, vi stiller.");
            examples[5].Should().Be("6. Berg er ikke altid lige smart, bl.a. ikke når hun afleverer blækregning for sent OG vedlægger den opgave, hun har kigget efter.");
            examples[6].Should().Be("7. Har du lyst til at gå ud og kigge stjerner, Oskar? Det er sådan et smukt vejr.");
        }

        [TestMethod]
        public void ParseExamples_ForGrillspydPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("GrillspydPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<string> examples = parser.ParseExamples();

            examples.Should().HaveCount(2);
            examples[0].Should().Be("1. Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver.");
            examples[1].Should().Be("2. Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater.");
        }

        #endregion

        public static string GetSimpleHTMLPage(string fileName)
        {
            string filePath = Path.Combine(_path!, "TestPages", "ddo", fileName);
            Assert.IsTrue(File.Exists(filePath));

            string fileContent;
            using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd();
            }

            fileContent.Should().NotBeNullOrEmpty($"cannot read content of file '{filePath}'.");

            return fileContent;
        }
    }
}
