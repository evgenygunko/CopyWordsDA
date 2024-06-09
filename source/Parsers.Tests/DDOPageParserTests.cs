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
        public void ParseDefinitions_ForKiggePage_Returns5Definitions()
        {
            string content = GetSimpleHTMLPage("KiggePage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(5);

            Definition definition1 = definitions.First();
            definition1.Meaning.Should().Be("rette blikket i en bestemt retning");
            definition1.Examples.Should().HaveCount(3);
            definition1.Examples.First().Should().Be("Børnene kiggede spørgende på hinanden.");
            definition1.Examples.Skip(1).First().Should().Be("kig lige en gang!");
            definition1.Examples.Skip(2).First().Should().Be("Han kiggede sig rundt, som om han ledte efter noget.");

            Definition definition2 = definitions.Skip(1).First();
            definition2.Meaning.Should().Be("undersøge nærmere; sætte sig ind i");
            definition2.Examples.Should().HaveCount(1);
            definition2.Examples.First().Should().Be("hun har kigget på de psykiske eftervirkninger hos voldtagne piger og kvinder.");

            Definition definition3 = definitions.Skip(2).First();
            definition3.Meaning.Should().Be("prøve at finde");
            definition3.Examples.Should().HaveCount(1);
            definition3.Examples.First().Should().Be("Vi kigger efter en bil i det prislag, og Carinaen opfylder de fleste af de krav, vi stiller.");

            Definition definition4 = definitions.Skip(3).First();
            definition4.Meaning.Should().Be("skrive af efter nogen; kopiere noget");
            definition4.Examples.Should().HaveCount(1);
            definition4.Examples.First().Should().Be("Berg er ikke altid lige smart, bl.a. ikke når hun afleverer blækregning for sent OG vedlægger den opgave, hun har kigget efter.");

            Definition definition5 = definitions.Skip(4).First();
            definition5.Meaning.Should().Be("se på; betragte");
            definition5.Examples.Should().HaveCount(1);
            definition5.Examples.First().Should().Be("Har du lyst til at gå ud og kigge stjerner, Oskar? Det er sådan et smukt vejr.");
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

        [TestMethod]
        public void ParseDefinitions_ForHajPage_Returns3Definitions()
        {
            string content = GetSimpleHTMLPage("HajPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(3);

            Definition definition1 = definitions.First();
            definition1.Meaning.Should().Be("stor, langstrakt bruskfisk med tværstillet mund på undersiden af hovedet, med 5-7 gællespalter uden gællelåg og med kraftig, ru hud");
            definition1.Tag.Should().BeNull();
            definition1.Examples.Should().HaveCount(1);
            definition1.Examples.First().Should().Be("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham.");

            Definition definition2 = definitions.Skip(1).First();
            definition2.Meaning.Should().Be("grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning");
            definition2.Tag.Should().Be("slang");
            definition2.Examples.Should().HaveCount(0);

            Definition definition3 = definitions.Skip(2).First();
            definition3.Meaning.Should().Be("person der er særlig dygtig til et spil, håndværk el.lign.");
            definition3.Tag.Should().Be("slang");
            definition3.Examples.Should().HaveCount(1);
            definition3.Examples.First().Should().Be("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb.");
        }

        [TestMethod]
        public void ParseDefinitions_ForDannebrogPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("DannebrogPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            Definition definition1 = definitions.First();

            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Should().Be("For mig er dannebrog noget samlende på tværs af køn, alder, etnicitet, værdier og politisk ståsted.");
            definition1.Examples.Skip(1).First().Should().Be("der var Dannebrog på bordet og balloner og gaver fra gæsterne. Om aftenen var vi alle trætte, men glade for en god fødselsdag.");
        }

        [TestMethod]
        public void ParseDefinitions_ForStiktossetPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("StiktossetPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            Definition definition1 = definitions.First();

            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Should().Be("Det her er så sindssygt, at jeg kan blive stiktosset over det.");
            definition1.Examples.Skip(1).First().Should().Be("Du ved godt, at jeg bliver stiktosset, når du hopper i sofaen.");
        }

        [TestMethod]
        public void ParseDefinitions_ForUnderholdningPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("UnderholdningPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            Definition definition1 = definitions.First();
        }

        [TestMethod]
        public void ParseDefinitions_ForGrillspydPage_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("GrillspydPage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Definition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            Definition definition1 = definitions.First();

            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Should().Be("Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver.");
            definition1.Examples.Skip(1).First().Should().Be("Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater.");
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
