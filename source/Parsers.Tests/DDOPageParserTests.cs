// Ignore Spelling: Underholdning Tiltale Højtryk Substantiv På Grillspyd Høj Kigge Stødtand Såsom Stiktosset Påtage Konjunktion Haj Frabede Fladtang Dannebrog

using System.Reflection;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Models.DDO;
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

        #region Tests for LoadHtml

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

        #region Tests for ParseHeadword

        [TestMethod]
        public void ParseHeadword_ForUnderholdning_ReturnsUnderholdning()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            word.Should().Be("underholdning");
        }

        [TestMethod]
        public void ParseHeadword_ForUGrillspyd_ReturnsGrillspydPage()
        {
            string content = GetSimpleHTMLPage("Grillspyd.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            word.Should().Be("grillspyd");
        }

        [TestMethod]
        public void ParseHeadword_ForStødtand_ReturnsStødtand()
        {
            string content = GetSimpleHTMLPage("Stødtand.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            word.Should().Be("stødtand");
        }

        [TestMethod]
        public void ParseHeadword_ForTiltale_ReturnsTiltale()
        {
            string content = GetSimpleHTMLPage("Tiltale.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            word.Should().Be("tiltale");
        }

        [TestMethod]
        public void ParseHeadword_ForPåHøjtryk_ReturnsPåUnderHøjtryk()
        {
            string content = GetSimpleHTMLPage("PåHøjtryk.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            word.Should().Be("på/under højtryk");
        }

        [TestMethod]
        public void ParseHeadword_Should_ParsePåtage()
        {
            string content = GetSimpleHTMLPage("Påtage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParseHeadword();
            word.Should().Be("påtage");
        }

        #endregion

        #region Tests for ParsePartOfSpeech

        [TestMethod]
        public void ParsePartOfSpeech_ForHaj_ReturnsSubstantiv()
        {
            string content = GetSimpleHTMLPage("Haj.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParsePartOfSpeech();
            word.Should().Be("substantiv, fælleskøn");
        }

        [TestMethod]
        public void ParsePartOfSpeech_ForGrillspyd_ReturnsSubstantiv()
        {
            string content = GetSimpleHTMLPage("Grillspyd.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParsePartOfSpeech();
            word.Should().Be("substantiv, intetkøn");
        }

        [TestMethod]
        public void ParsePartOfSpeech_ForHøj_ReturnsSubstantiv()
        {
            string content = GetSimpleHTMLPage("Høj.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParsePartOfSpeech();
            word.Should().Be("adjektiv");
        }

        [TestMethod]
        public void ParsePartOfSpeech_ForKigge_ReturnsSubstantiv()
        {
            string content = GetSimpleHTMLPage("Kigge.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParsePartOfSpeech();
            word.Should().Be("verbum");
        }

        [TestMethod]
        public void ParsePartOfSpeech_ForSåsom_ReturnsKonjunktion()
        {
            string content = GetSimpleHTMLPage("Såsom.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParsePartOfSpeech();
            word.Should().Be("konjunktion");
        }

        [TestMethod]
        public void ParsePartOfSpeech_ForPåHøjtryk_ReturnsEmptyString()
        {
            string content = GetSimpleHTMLPage("PåHøjtryk.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string word = parser.ParsePartOfSpeech();
            word.Should().BeEmpty();
        }

        #endregion

        #region Tests for ParseEndings

        [TestMethod]
        public void ParseEndings_ForUnderholdning_ReturnsEndings()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string endings = parser.ParseEndings();
            endings.Should().Be("-en");
        }

        [TestMethod]
        public void ParseEndings_ForStødtand_ReturnsEndings()
        {
            string content = GetSimpleHTMLPage("Stødtand.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string endings = parser.ParseEndings();
            endings.Should().Be("-en, ..tænder, ..tænderne");
        }

        [TestMethod]
        public void ParseEndings_ForHøj_ReturnsEndings()
        {
            string content = GetSimpleHTMLPage("Høj.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string endings = parser.ParseEndings();
            endings.Should().Be("-t, -e || -ere, -est");
        }

        [TestMethod]
        public void ParseEndings_ForSkat_ReturnsEndings()
        {
            string content = GetSimpleHTMLPage("Skat.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string endings = parser.ParseEndings();
            endings.Should().Be("betydning 1: -ten, -ter, -terne || betydning 2: -ten, -te, -tene");
        }

        #endregion

        #region Tests for ParsePronunciation

        [TestMethod]
        public void ParsePronunciation_ReturnsUnderholdning_ForUnderholdningPage()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParsePronunciation();
            Assert.AreEqual("[ˈɔnʌˌhʌlˀneŋ]", pronunciation);
        }

        [TestMethod]
        public void ParsePronunciation_ReturnsKigge_ForKiggePage()
        {
            string content = GetSimpleHTMLPage("Kigge.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParsePronunciation();
            Assert.AreEqual("[ˈkigə]", pronunciation);
        }

        [TestMethod]
        public void ParsePronunciation_ReturnsEmptyString_ForGrillspydPage()
        {
            string content = GetSimpleHTMLPage("Grillspyd.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParsePronunciation();
            Assert.AreEqual(string.Empty, pronunciation);
        }

        #endregion

        #region Tests for ParseSound

        [TestMethod]
        public void ParseSound_ReturnsSoundFile_ForUnderholdningPage()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string pronunciation = parser.ParseSound();
            pronunciation.Should().Be("https://static.ordnet.dk/mp3/12004/12004770_1.mp3");
        }

        [TestMethod]
        public void ParseSound_ReturnsEmptyString_ForKiggePage()
        {
            string content = GetSimpleHTMLPage("Kigge.html");

            // Kigge page doesn't have a sound file
            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string sound = parser.ParseSound();
            sound.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseSound_ForDannebrog_ReturnsSoundFile()
        {
            string content = GetSimpleHTMLPage("Dannebrog.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string sound = parser.ParseSound();
            sound.Should().Be("https://static.ordnet.dk/mp3/11008/11008357_1.mp3");
        }

        [TestMethod]
        public void ParseSound_ForHaj_ReturnsSoundFile()
        {
            string content = GetSimpleHTMLPage("Haj.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            string sound = parser.ParseSound();
            sound.Should().Be("https://static.ordnet.dk/mp3/11019/11019539_1.mp3");
        }

        #endregion

        #region Tests for ParseDefinitions

        [TestMethod]
        public void ParseDefinitions_ForUnderholdning_ReturnsOneDefinition()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);
            definitions.First().Meaning.Should().Be("noget der morer, glæder eller adspreder nogen, fx optræden, et lettere og ikke særlig krævende åndsprodukt eller en fornøjelig beskæftigelse");
        }

        [TestMethod]
        public void ParseDefinitions_ForKigge_Returns5Definitions()
        {
            string content = GetSimpleHTMLPage("Kigge.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(5);

            DDODefinition definition1 = definitions.First();
            definition1.Meaning.Should().Be("rette blikket i en bestemt retning");
            definition1.Examples.Should().HaveCount(3);
            definition1.Examples.First().Original.Should().Be("Børnene kiggede spørgende på hinanden.");
            definition1.Examples.Skip(1).First().Original.Should().Be("kig lige en gang!");
            definition1.Examples.Skip(2).First().Original.Should().Be("Han kiggede sig rundt, som om han ledte efter noget.");

            DDODefinition definition2 = definitions.Skip(1).First();
            definition2.Meaning.Should().Be("undersøge nærmere; sætte sig ind i");
            definition2.Examples.Should().HaveCount(1);
            definition2.Examples.First().Original.Should().Be("hun har kigget på de psykiske eftervirkninger hos voldtagne piger og kvinder.");

            DDODefinition definition3 = definitions.Skip(2).First();
            definition3.Meaning.Should().Be("prøve at finde");
            definition3.Examples.Should().HaveCount(1);
            definition3.Examples.First().Original.Should().Be("Vi kigger efter en bil i det prislag, og Carinaen opfylder de fleste af de krav, vi stiller.");

            DDODefinition definition4 = definitions.Skip(3).First();
            definition4.Meaning.Should().Be("skrive af efter nogen; kopiere noget");
            definition4.Examples.Should().HaveCount(1);
            definition4.Examples.First().Original.Should().Be("Berg er ikke altid lige smart, bl.a. ikke når hun afleverer blækregning for sent OG vedlægger den opgave, hun har kigget efter.");

            DDODefinition definition5 = definitions.Skip(4).First();
            definition5.Meaning.Should().Be("se på; betragte");
            definition5.Examples.Should().HaveCount(1);
            definition5.Examples.First().Original.Should().Be("Har du lyst til at gå ud og kigge stjerner, Oskar? Det er sådan et smukt vejr.");
        }

        [TestMethod]
        public void ParseDefinitions_ForGrillspyd_ReturnsOneDefinition()
        {
            string content = GetSimpleHTMLPage("Grillspyd.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);
            definitions.First().Meaning.Should().Be("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public void ParseDefinitions_ForHaj_Returns3Definitions()
        {
            string content = GetSimpleHTMLPage("Haj.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(3);

            DDODefinition definition1 = definitions.First();
            definition1.Meaning.Should().Be("stor, langstrakt bruskfisk med tværstillet mund på undersiden af hovedet, med 5-7 gællespalter uden gællelåg og med kraftig, ru hud");
            definition1.Tag.Should().BeNull();
            definition1.Examples.Should().HaveCount(1);
            definition1.Examples.First().Original.Should().Be("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham.");

            DDODefinition definition2 = definitions.Skip(1).First();
            definition2.Meaning.Should().Be("grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning");
            definition2.Tag.Should().Be("slang");
            definition2.Examples.Should().HaveCount(1);
            definition2.Examples.First().Original.Should().Be("-");

            DDODefinition definition3 = definitions.Skip(2).First();
            definition3.Meaning.Should().Be("person der er særlig dygtig til et spil, håndværk el.lign.");
            definition3.Tag.Should().Be("slang");
            definition3.Examples.Should().HaveCount(1);
            definition3.Examples.First().Original.Should().Be("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb.");
        }

        [TestMethod]
        public void ParseDefinitions_ForFladtang_ReturnsOneEmptyExample()
        {
            // When there are no examples, we still want to add an empty example to allow copying back the meaning.
            // (Copying requires selecting one or more examples.)
            string content = GetSimpleHTMLPage("fladtang.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            ;
            definition1.Meaning.Should().Be("lille tang med aflange, flade og riflede kæber som bruges til at holde fast på små genstande fx i forbindelse med elektriker- eller hobbyarbejde");
            definition1.Tag.Should().BeNull();
            definition1.Examples.Should().HaveCount(1);
            definition1.Examples.First().Original.Should().Be("-");
        }

        [TestMethod]
        public void ParseDefinitions_ForDannebrog_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("Dannebrog.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Original.Should().Be("For mig er dannebrog noget samlende på tværs af køn, alder, etnicitet, værdier og politisk ståsted.");
            definition1.Examples.Skip(1).First().Original.Should().Be("der var Dannebrog på bordet og balloner og gaver fra gæsterne. Om aftenen var vi alle trætte, men glade for en god fødselsdag.");
        }

        [TestMethod]
        public void ParseDefinitions_ForStiktosset_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("Stiktosset.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Original.Should().Be("Det her er så sindssygt, at jeg kan blive stiktosset over det.");
            definition1.Examples.Skip(1).First().Original.Should().Be("Du ved godt, at jeg bliver stiktosset, når du hopper i sofaen.");
        }

        [TestMethod]
        public void ParseDefinitions_ForUnderholdning_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            definition1.Examples.Should().HaveCount(2);
        }

        [TestMethod]
        public void ParseDefinitions_ForGrillspyd_Returns2Examples()
        {
            string content = GetSimpleHTMLPage("Grillspyd.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Original.Should().Be("Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver.");
            definition1.Examples.Skip(1).First().Original.Should().Be("Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater.");
        }

        [TestMethod]
        public void ParseDefinitions_ForPåHøjtryk_ReturnsOneDefinition()
        {
            string content = GetSimpleHTMLPage("PåHøjtryk.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            definition1.Meaning.Should().Be("intensivt og som regel under tidspres");
            definition1.Tag.Should().Be("overført");
            definition1.Examples.Should().HaveCount(1);
            definition1.Examples.First().Original.Should().Be("håndværkerne [arbejdede] på højtryk for at gøre teatersalen klar.");
        }

        [TestMethod]
        public void ParseDefinitions_ForFrabede_ReturnsOneDefinition()
        {
            string content = GetSimpleHTMLPage("frabede.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<DDODefinition> definitions = parser.ParseDefinitions();

            definitions.Should().HaveCount(1);

            DDODefinition definition1 = definitions.First();
            definition1.Meaning.Should().Be("bede sig fritaget for; afslå; sige nej tak til");
            definition1.Tag.Should().BeNull();
            definition1.Examples.Should().HaveCount(2);
            definition1.Examples.First().Original.Should().Be("ud af 2,4 millioner husstande har kun 36.000 frabedt sig reklametryksager.");
            definition1.Examples.Last().Original.Should().Be("Al opmærksomhed på min fødselsdag den 21. sept. frabedes.");
        }

        #endregion

        #region Tests for ParseVariants

        [TestMethod]
        public void ParseVariants_ForUnderholdning_Returns1Variation()
        {
            string content = GetSimpleHTMLPage("Underholdning.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Variant> variants = parser.ParseVariants();

            variants.Should().HaveCount(1);

            variants[0].Word.Should().Be("underholdning sb.");
            variants[0].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=underholdning&query=underholdning");
        }

        [TestMethod]
        public void ParseVariants_ForHøj_Returns2Variants()
        {
            string content = GetSimpleHTMLPage("Høj.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Variant> variants = parser.ParseVariants();

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("høj(1) sb.");
            variants[0].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=høj,1&query=høj");

            variants[1].Word.Should().Be("høj(2) adj.");
            variants[1].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=høj,2&query=høj");
        }

        [TestMethod]
        public void ParseVariants_ForSkatPage_Returns2Variants()
        {
            string content = GetSimpleHTMLPage("Skat.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Variant> variants = parser.ParseVariants();

            variants.Should().HaveCount(3);

            variants[0].Word.Should().Be("skat sb.");
            variants[0].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=skat&query=skat");

            variants[1].Word.Should().Be("skat -> skatte vb.");
            variants[1].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=skatte&query=skat");

            variants[2].Word.Should().Be("skat -> skate vb.");
            variants[2].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=skate&query=skat");
        }

        [TestMethod]
        public void ParseVariants_ForPåHøjtryk_Returns0Variants()
        {
            string content = GetSimpleHTMLPage("PåHøjtryk.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Variant> variants = parser.ParseVariants();

            variants.Should().HaveCount(0);
        }

        [TestMethod]
        public void ParseVariants_ForPåtage_Returns1Variant()
        {
            string content = GetSimpleHTMLPage("Påtage.html");

            DDOPageParser parser = new DDOPageParser();
            parser.LoadHtml(content);

            List<Variant> variants = parser.ParseVariants();

            variants.Should().HaveCount(1);

            variants[0].Word.Should().Be("påtage vb.");
            variants[0].Url.Should().Be("https://ordnet.dk/ddo/ordbog?select=påtage&query=påtage");
        }

        #endregion

        #region Private Methods

        private static string GetSimpleHTMLPage(string fileName)
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

        #endregion
    }
}
