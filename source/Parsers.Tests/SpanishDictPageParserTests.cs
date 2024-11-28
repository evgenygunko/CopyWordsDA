using System.Reflection;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Models.SpanishDict;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CopyWords.Parsers.Tests
{
    [TestClass]
    public class SpanishDictPageParserTests
    {
        private static string s_path = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _ = context;
#pragma warning disable CS8601 // Possible null reference assignment.
            s_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        #region ParseHeadword tests

        [DataTestMethod]
        [DataRow("afeitar")]
        [DataRow("águila")]
        [DataRow("aprovechar")]
        [DataRow("costar un ojo de la cara")]
        [DataRow("mitologo")]
        [DataRow("saltamontes")]
        public void ParseHeadword_Should_ReturnWordFromModel(string word)
        {
            var parser = new SpanishDictPageParser();

            string? result = parser.ParseHeadword(LoadTestObject(word));

            result.Should().Be(word);
        }

        #endregion

        #region ParseSoundURL tests

        [DataTestMethod]
        [DataRow("afeitar", "https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_4189_speaker_7_syllable_all_version_50.mp4")]
        [DataRow("águila", "https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_13701_speaker_7_syllable_all_version_50.mp4")]
        [DataRow("aprovechar", "https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_13603_speaker_7_syllable_all_version_50.mp4")]
        public void ParseSoundURL_WhenSpanishPronunciationWithVideoExists_ReturnSoundURL(string word, string expectedSoundURL)
        {
            var parser = new SpanishDictPageParser();
            string? result = parser.ParseSoundURL(LoadTestObject(word));

            result.Should().Be(expectedSoundURL);
        }

        [TestMethod]
        public void ParseSoundURL_WhenNoPronunciationsExist_ReturnNull()
        {
            const string word = "costar un ojo de la cara";

            var parser = new SpanishDictPageParser();
            string? result = parser.ParseSoundURL(LoadTestObject(word));

            result.Should().BeNull();
        }

        [TestMethod]
        public void ParseSoundURL_WhenSpanishPronunciationHasNoVideo_UsesLatinAmericaPronunciation()
        {
            // When there is no video for the sound, it uses a robotic pronunciation. We don't want to use it because the quality is too poor.
            const string word = "aconsejar";
            var parser = new SpanishDictPageParser();

            string? result = parser.ParseSoundURL(LoadTestObject(word));

            result.Should().Be("https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_265_speaker_3_syllable_all_version_51.mp4");
        }

        #endregion

        #region ParseDefinitions tests

        [TestMethod]
        public void ParseDefinitions_ForAfeitar_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("afeitar")).ToList();

            result.Should().HaveCount(2);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // afeitar
            definition = result[0];
            definition.WordES.Should().Be("afeitar");
            definition.PartOfSpeech.Should().Be("transitive verb");

            definition.Contexts.Should().HaveCount(1);
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to remove hair)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Description.Should().Be("to shave");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Para el verano, papá decidió afeitar al perro.");
            example.English.Should().Be("For the summer, dad decided to shave the dog.");

            // afeitarse
            definition = result[1];
            definition.WordES.Should().Be("afeitarse");
            definition.PartOfSpeech.Should().Be("reflexive verb");

            definition.Contexts.Should().HaveCount(1);
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to shave oneself)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Description.Should().Be("to shave");
            example = meaning.Examples.First();
            example.Original.Should().Be("¿Con qué frecuencia te afeitas la barba?");
            example.English.Should().Be("How often do you shave your beard?");
        }

        [TestMethod]
        public void ParseDefinitions_ForTrotar_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("trotar")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("trotar");
            definition.PartOfSpeech.Should().Be("intransitive verb");

            definition.Contexts.Should().HaveCount(3);

            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to run) (Mexico)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Description.Should().Be("to jog");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Salgo a trotar todas las mañanas.");
            example.English.Should().Be("I go jogging every morning.");

            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(horseback riding)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Description.Should().Be("to trot");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Carolina se levanta temprano cada día a sacar el caballo a trotar.");
            example.English.Should().Be("Carolina gets up early every day to take the horse out to trot.");

            context = definition.Contexts.Skip(2).First();
            context.ContextEN.Should().Be("(colloquial) (to bustle about)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Description.Should().Be("to rush around");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Ya me cansé de estar trotando todo el día.");
            example.English.Should().Be("I'm tired of rushing around all day.");
        }

        [TestMethod]
        public void ParseDefinitions_ForCoche_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("coche")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("coche");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(4);

            // 1. (vehicle)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(vehicle)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("car");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Mi coche no prende porque tiene una falla en el motor.");
            example.English.Should().Be("My car won't start because of a problem with the engine.");

            meaning = context.Meanings.Skip(1).First();
            meaning.Description.Should().Be("automobile");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Todos estos coches tienen bolsas de aire.");
            example.English.Should().Be("All these automobiles have airbags.");

            // 2. (vehicle led by horses)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(vehicle led by horses)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("carriage");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Los monarcas llegaron en un coche elegante.");
            example.English.Should().Be("The monarchs arrived in an elegant carriage.");

            meaning = context.Meanings.Skip(1).First();
            meaning.Description.Should().Be("coach");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.");
            example.English.Should().Be("Horse-drawn coaches were used much more before the invention of the automobile.");

            // ...
        }

        [TestMethod]
        public void ParseDefinitions_ForHipócrita_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("hipócrita")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // el hipócrita, la hipócrita
            definition = result[0];
            definition.WordES.Should().Be("el hipócrita, la hipócrita");
            definition.PartOfSpeech.Should().Be("masculine or feminine noun");

            definition.Contexts.Should().HaveCount(2);

            // 1. (false person)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(false person)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("hypocrite");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Es una hipócrita. Pues y no va por ahí criticándome a mis espaldas.");
            example.English.Should().Be("She's a hypocrite. It turns out she goes around criticizing me behind my back.");

            // (false)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(false)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("hypocritical");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("No soporto esa sonrisa hipócrita que tiene.");
            example.English.Should().Be("I cannot stand that hypocritical smile of his.");
        }

        [TestMethod]
        public void ParseDefinitions_ForGuay_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("guay")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // guay
            definition = result[0];
            definition.WordES.Should().Be("guay");
            definition.PartOfSpeech.Should().Be("interjection");

            definition.Contexts.Should().HaveCount(3);

            // 1. (colloquial) (used to express approval) (Spain)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(colloquial) (used to express approval) (Spain)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("cool (colloquial)");
            meaning.Examples.Should().HaveCount(2);
            example = meaning.Examples.First();
            example.Original.Should().Be("¿Quieres que veamos la peli en mi ordenador? - ¡Guay, tío!");
            example.English.Should().Be("Do you want to watch the movie on my computer? - Cool, man!");
            example = meaning.Examples.Skip(1).First();
            example.Original.Should().Be("¡Gané un viaje a Francia! - ¡Guay!");
            example.English.Should().Be("I won a trip to France! - Cool!");

            // 2. (colloquial) (extremely good) (Spain)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(colloquial) (extremely good) (Spain)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("cool (colloquial)");
            meaning.Examples.Should().HaveCount(2);
            example = meaning.Examples.First();
            example.Original.Should().Be("La fiesta de anoche estuvo muy guay.");
            example.English.Should().Be("Last night's party was really cool.");
            example = meaning.Examples.Skip(1).First();
            example.Original.Should().Be("Tus amigos son guays, Roberto. ¿Dónde los conociste?");
            example.English.Should().Be("Your friends are cool, Roberto. Where did you meet them?");

            meaning = context.Meanings.Skip(1).First();
            meaning.Description.Should().Be("super (colloquial)");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("¡Que monopatín tan guay!");
            example.English.Should().Be("That's a super skateboard!");

            // 3. (colloquial) (extremely well) (Spain)
            context = definition.Contexts.Skip(2).First();
            context.ContextEN.Should().Be("(colloquial) (extremely well) (Spain)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("awesome (colloquial) (adjective)");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Nos lo pasamos guay en la fiesta de Reme.");
            example.English.Should().Be("We had an awesome time at Reme's party.");

            meaning = context.Meanings.Skip(1).First();
            meaning.Description.Should().Be("great (colloquial) (adjective)");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Tu coche nos vendría guay para la excursión.");
            example.English.Should().Be("It would be great if we could use your car for the trip.");
        }

        [TestMethod]
        public void ParseDefinitions_ForClubNocturno_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("club nocturno")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("club nocturno");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (general)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(general)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("nightclub");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Este bar va a cerrar pronto, pero hay un club nocturno cerca de aquí que abre hasta las 3 am.");
            example.English.Should().Be("This bar is going to close soon, but there's a nightclub nearby that's open until 3 am.");
        }

        [TestMethod]
        public void ParseDefinitions_ForVeneno_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("veneno")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("veneno");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(2);

            // 1. (toxic substance)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(toxic substance)");
            context.Meanings.Should().HaveCount(2);

            Meaning meaning1 = context.Meanings.First();
            meaning1.Description.Should().Be("venom (of an animal)");
            meaning1.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d533b470-18a4-4cae-ad08-3ee8858ae02c.jpg");
            meaning1.Examples.Should().HaveCount(1);
            example = meaning1.Examples.First();
            example.Original.Should().Be("La herida aún tiene el veneno dentro.");
            example.English.Should().Be("The wound still has venom in it.");

            Meaning meaning2 = context.Meanings.Skip(1).First();
            meaning2.Description.Should().Be("poison");
            meaning2.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d07aa7fd-a3fd-4d06-9751-656180d8b1ee.jpg");
            meaning2.Examples.Should().HaveCount(1);
            example = meaning2.Examples.First();
            example.Original.Should().Be("Estos hongos contienen un veneno mortal.");
            example.English.Should().Be("These mushrooms contain a deadly poison.");

            // 2. (ill intent)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(ill intent)");
            context.Meanings.Should().HaveCount(1);

            meaning1 = context.Meanings.First();
            meaning1.Description.Should().Be("venom");
            meaning1.ImageUrl.Should().BeNull();
            meaning1.Examples.Should().HaveCount(1);
            example = meaning1.Examples.First();
            example.Original.Should().Be("Le espetó con tal veneno que ni se atrevió a responderle.");
            example.English.Should().Be("She spat at him with such venom that he didn't even dare respond.");
        }

        [TestMethod]
        public void ParseDefinitions_ForMitologo_ReturnsEmptyTranslationsList()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("mitologo")).ToList();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseDefinitions_For123_ReturnsEmptyTranslationsList()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("123")).ToList();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseDefinitions_ForSaltamontes_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("saltamontes")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("saltamontes");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (animal)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(animal)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("grasshopper");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Los saltamontes pueden saltar muy alto.");
            example.English.Should().Be("Grasshoppers can jump really high.");

            meaning.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/5bf100e5-da54-4be6-a55c-281edcd08b10.jpg");
        }

        [TestMethod]
        public void ParseDefinitions_ForIndígena_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("indígena")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("indígena");
            definition.PartOfSpeech.Should().Be("adjective");

            definition.Contexts.Should().HaveCount(2);

            // 1. (of native origins)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(of native origins)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("indigenous");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("El gobierno quiere preservar el folclor y las tradiciones indígenas.");
            example.English.Should().Be("The government wants to preserve the indigenous folklore and traditions.");

            meaning.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/native%252C%2520indigenous.jpg");

            meaning = context.Meanings.Skip(1).First();
            meaning.Description.Should().Be("native");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("La comunidad indígena no está de acuerdo con la tala del bosque.");
            example.English.Should().Be("The native community is against the clearing of the forest.");

            // 2. (indigenous person)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(indigenous person)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("native");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Este parque natural está protegido por los indígenas que habitan la zona.");
            example.English.Should().Be("This natural park is protected by the natives that inhabit the area.");

            meaning.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/0ca649f9-134a-4210-ae48-2a8bbadb32cc.jpg");
        }

        [TestMethod]
        public void ParseDefinitions_ForIglesia_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("iglesia")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("iglesia");
            definition.PartOfSpeech.Should().Be("feminine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (religious)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(religious)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("church");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Vámonos a la iglesia que la misa comienza pronto.");
            example.English.Should().Be("Let's go to the church; mass starts soon.");

            meaning.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/temple%253B%2520church.jpg");
        }

        [TestMethod]
        public void ParseDefinitions_ForVaso_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("vaso")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("vaso");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCountGreaterThan(1);

            // 1. (tableware)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(tableware)");
            context.Meanings.Should().HaveCountGreaterThan(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("glass");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Toma un vaso del estante si tienes sed.");
            example.English.Should().Be("Get a glass from the shelf if you're thirsty.");

            meaning.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/Glass%2520%2528empty%2529.jpg");
        }

        [TestMethod]
        public void ParseDefinitions_ForNoTePreocupes_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<SpanishDictDefinition> result = parser.ParseDefinitions(LoadTestObject("NoTePreocupes")).ToList();

            result.Should().HaveCount(1);

            SpanishDictDefinition definition;
            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition = result[0];
            definition.WordES.Should().Be("no te preocupes");
            definition.PartOfSpeech.Should().Be("phrase");

            definition.Contexts.Should().HaveCount(1);

            // 1. (general)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(general)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Description.Should().Be("don't worry");
            meaning.Examples.Should().HaveCountGreaterThan(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("No te preocupes por los resultados del examen.");
            example.English.Should().Be("Don't worry about the results of the test.");

            meaning.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/don%2527t%2520worry.jpg");
        }

        #endregion

        #region Private methods

        private static Models.SpanishDict.WordJsonModel LoadTestObject(string word)
        {
            string htmlPagePath = Path.Combine(s_path, "TestPages", "SpanishDict", $"{word}.json");
            if (!File.Exists(htmlPagePath))
            {
                throw new Exception($"Cannot find test file '{htmlPagePath}'");
            }

            string json = File.ReadAllText(htmlPagePath);

            var wordObj = JsonConvert.DeserializeObject<Models.SpanishDict.WordJsonModel>(json);
            return wordObj!;
        }

        #endregion
    }
}
