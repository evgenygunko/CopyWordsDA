// Ignore Spelling: Coche Veneno Vaso Trotar Saltamontes Preocupes Nocturno Mitologo Iglesia Guay Indígena Hipócrita Dict Afeitar

using System.Reflection;
using CopyWords.Parsers.Models.SpanishDict;
using FluentAssertions;
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

        [TestMethod]
        [DataRow("Afeitar")]
        [DataRow("Águila")]
        [DataRow("Aprovechar")]
        [DataRow("costar un ojo de la cara")]
        [DataRow("mitologo")]
        [DataRow("saltamontes")]
        [DataRow("wasapear")]
        public void ParseHeadword_Should_ReturnWordFromModel(string word)
        {
            var parser = new SpanishDictPageParser();

            string? result = parser.ParseHeadword(LoadTestObject(word));

            result.Should().Be(word.ToLower());
        }

        #endregion

        #region ParseSoundURL tests

        [TestMethod]
        [DataRow("Afeitar", "https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_4189_speaker_7_syllable_all_version_50.mp4")]
        [DataRow("Águila", "https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_13701_speaker_7_syllable_all_version_50.mp4")]
        [DataRow("Aprovechar", "https://d10gt6izjc94x0.cloudfront.net/desktop/lang_es_pron_13603_speaker_7_syllable_all_version_50.mp4")]
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

        #region ParseDefinition tests

        [TestMethod]
        public void ParseDefinition_ForAfeitar_ReturnsDefinitionForTransitiveVerb()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Afeitar"));

            definition.Should().NotBeNull();

            // We return only first variant - "afeitar". For getting "afeitarse", a user will need to click on the variant link.
            definition.WordES.Should().Be("afeitar");
            definition.PartOfSpeech.Should().Be("transitive verb");

            definition.Contexts.Should().HaveCount(1);
            SpanishDictContext context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to remove hair)");
            context.Meanings.Should().HaveCount(1);

            Meaning meaning = context.Meanings.First();
            meaning.Original.Should().Be("to shave");
            meaning.Examples.Should().HaveCount(1);
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/to+shave?langFrom=en");

            Models.Example example = meaning.Examples.First();
            example.Original.Should().Be("Para el verano, papá decidió afeitar al perro.");
            example.Translation.Should().Be("For the summer, dad decided to shave the dog.");
        }

        [TestMethod]
        public void ParseDefinition_ForAfeitar_ReturnsDefinitionForReflexiveVerb()
        {
            var parser = new SpanishDictPageParser();

            string n = "1";
            string p = "0";
            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Afeitar"), n, p);

            definition.Should().NotBeNull();
            definition.WordES.Should().Be("afeitarse");
            definition.PartOfSpeech.Should().Be("reflexive verb");

            definition.Contexts.Should().HaveCount(1);
            SpanishDictContext context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to shave oneself)");

            context.Meanings.Should().HaveCount(1);
            Meaning meaning = context.Meanings.First();
            meaning.Original.Should().Be("to shave");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/to+shave?langFrom=en");

            Models.Example example = meaning.Examples.First();
            example.Original.Should().Be("¿Con qué frecuencia te afeitas la barba?");
            example.Translation.Should().Be("How often do you shave your beard?");
        }

        [TestMethod]
        public void ParseDefinition_ForTrotar_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Trotar"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("trotar");
            definition.PartOfSpeech.Should().Be("intransitive verb");

            definition.Contexts.Should().HaveCount(3);

            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to run) (Mexico)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("to jog");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/to+jog?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Salgo a trotar todas las mañanas.");
            example.Translation.Should().Be("I go jogging every morning.");

            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(horseback riding)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("to trot");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/to+trot?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Carolina se levanta temprano cada día a sacar el caballo a trotar.");
            example.Translation.Should().Be("Carolina gets up early every day to take the horse out to trot.");

            context = definition.Contexts.Skip(2).First();
            context.ContextEN.Should().Be("(colloquial) (to bustle about)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("to rush around");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/to+rush+around?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Ya me cansé de estar trotando todo el día.");
            example.Translation.Should().Be("I'm tired of rushing around all day.");
        }

        [TestMethod]
        public void ParseDefinition_ForCoche_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Coche"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("coche");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(4);

            // 1. (vehicle)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(vehicle)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("car");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/car?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Mi coche no prende porque tiene una falla en el motor.");
            example.Translation.Should().Be("My car won't start because of a problem with the engine.");

            meaning = context.Meanings.Skip(1).First();
            meaning.Original.Should().Be("automobile");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/automobile?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Todos estos coches tienen bolsas de aire.");
            example.Translation.Should().Be("All these automobiles have airbags.");

            // 2. (vehicle led by horses)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(vehicle led by horses)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("carriage");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/carriage?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Los monarcas llegaron en un coche elegante.");
            example.Translation.Should().Be("The monarchs arrived in an elegant carriage.");

            meaning = context.Meanings.Skip(1).First();
            meaning.Original.Should().Be("coach");
            meaning.LookupUrl.Should().Be("https://www.spanishdict.com/translate/coach?langFrom=en");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.");
            example.Translation.Should().Be("Horse-drawn coaches were used much more before the invention of the automobile.");

            // ...
        }

        [TestMethod]
        public void ParseDefinition_ForHipócrita_ReturnsDefinitionForNoun()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("hipócrita"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // el hipócrita, la hipócrita
            definition.WordES.Should().Be("el hipócrita, la hipócrita");
            definition.PartOfSpeech.Should().Be("masculine or feminine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (false person)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(false person)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("hypocrite");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Es una hipócrita. Pues y no va por ahí criticándome a mis espaldas.");
            example.Translation.Should().Be("She's a hypocrite. It turns out she goes around criticizing me behind my back.");
        }

        [TestMethod]
        public void ParseDefinition_ForHipócrita_ReturnsDefinitionForAdjective()
        {
            var parser = new SpanishDictPageParser();

            string n = "0";
            string p = "1";
            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("hipócrita"), n, p);

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // el hipócrita, la hipócrita
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(false)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("hypocritical");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("No soporto esa sonrisa hipócrita que tiene.");
            example.Translation.Should().Be("I cannot stand that hypocritical smile of his.");
        }

        [TestMethod]
        public void ParseDefinition_ForGuay_ReturnsDefinitionForInterjection()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Guay"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // guay
            context = definition.Contexts.First();
            definition.WordES.Should().Be("guay");
            definition.PartOfSpeech.Should().Be("interjection");

            definition.Contexts.Should().HaveCount(1);

            // 1. (colloquial) (used to express approval) (Spain)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(colloquial) (used to express approval) (Spain)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("cool (colloquial)");
            meaning.Examples.Should().HaveCount(2);
            example = meaning.Examples.First();
            example.Original.Should().Be("¿Quieres que veamos la peli en mi ordenador? - ¡Guay, tío!");
            example.Translation.Should().Be("Do you want to watch the movie on my computer? - Cool, man!");
            example = meaning.Examples.Skip(1).First();
            example.Original.Should().Be("¡Gané un viaje a Francia! - ¡Guay!");
            example.Translation.Should().Be("I won a trip to France! - Cool!");
        }

        [TestMethod]
        public void ParseDefinition_ForGuay_ReturnsDefinitionForAdjective()
        {
            var parser = new SpanishDictPageParser();

            string n = "0";
            string p = "1";
            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Guay"), n, p);

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // guay
            context = definition.Contexts.First();
            definition.WordES.Should().Be("guay");
            definition.PartOfSpeech.Should().Be("adjective");

            definition.Contexts.Should().HaveCount(1);

            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(colloquial) (extremely good) (Spain)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("cool (colloquial)");
            meaning.Examples.Should().HaveCount(2);
            example = meaning.Examples.First();
            example.Original.Should().Be("La fiesta de anoche estuvo muy guay.");
            example.Translation.Should().Be("Last night's party was really cool.");
            example = meaning.Examples.Skip(1).First();
            example.Original.Should().Be("Tus amigos son guays, Roberto. ¿Dónde los conociste?");
            example.Translation.Should().Be("Your friends are cool, Roberto. Where did you meet them?");

            meaning = context.Meanings.Skip(1).First();
            meaning.Original.Should().Be("super (colloquial)");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("¡Que monopatín tan guay!");
            example.Translation.Should().Be("That's a super skateboard!");
        }

        [TestMethod]
        public void ParseDefinition_ForGuay_ReturnsDefinitionForAdverb()
        {
            var parser = new SpanishDictPageParser();

            string n = "0";
            string p = "2";
            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("Guay"), n, p);

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            // guay
            context = definition.Contexts.First();
            definition.WordES.Should().Be("guay");
            definition.PartOfSpeech.Should().Be("adverb");

            definition.Contexts.Should().HaveCount(1);

            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(colloquial) (extremely well) (Spain)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("awesome (colloquial) (adjective)");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Nos lo pasamos guay en la fiesta de Reme.");
            example.Translation.Should().Be("We had an awesome time at Reme's party.");

            meaning = context.Meanings.Skip(1).First();
            meaning.Original.Should().Be("great (colloquial) (adjective)");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Tu coche nos vendría guay para la excursión.");
            example.Translation.Should().Be("It would be great if we could use your car for the trip.");
        }

        [TestMethod]
        public void ParseDefinition_ForClubNocturno_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("club nocturno"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(general)");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (general)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(general)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("nightclub");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Este bar va a cerrar pronto, pero hay un club nocturno cerca de aquí que abre hasta las 3 am.");
            example.Translation.Should().Be("This bar is going to close soon, but there's a nightclub nearby that's open until 3 am.");
        }

        [TestMethod]
        public void ParseDefinition_ForVeneno_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("veneno"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Models.Example example;

            definition.WordES.Should().Be("veneno");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(2);

            // 1. (toxic substance)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(toxic substance)");
            context.Meanings.Should().HaveCount(2);

            Meaning meaning1 = context.Meanings.First();
            meaning1.Original.Should().Be("venom (of an animal)");
            meaning1.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/d533b470-18a4-4cae-ad08-3ee8858ae02c.jpg");
            meaning1.Examples.Should().HaveCount(1);
            example = meaning1.Examples.First();
            example.Original.Should().Be("La herida aún tiene el veneno dentro.");
            example.Translation.Should().Be("The wound still has venom in it.");

            Meaning meaning2 = context.Meanings.Skip(1).First();
            meaning2.Original.Should().Be("poison");
            meaning2.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/d07aa7fd-a3fd-4d06-9751-656180d8b1ee.jpg");
            meaning2.Examples.Should().HaveCount(1);
            example = meaning2.Examples.First();
            example.Original.Should().Be("Estos hongos contienen un veneno mortal.");
            example.Translation.Should().Be("These mushrooms contain a deadly poison.");

            // 2. (ill intent)
            context = definition.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(ill intent)");
            context.Meanings.Should().HaveCount(1);

            meaning1 = context.Meanings.First();
            meaning1.Original.Should().Be("venom");
            meaning1.ImageUrl.Should().BeNull();
            meaning1.Examples.Should().HaveCount(1);
            example = meaning1.Examples.First();
            example.Original.Should().Be("Le espetó con tal veneno que ni se atrevió a responderle.");
            example.Translation.Should().Be("She spat at him with such venom that he didn't even dare respond.");
        }

        [TestMethod]
        public void ParseDefinition_ForMitologo_ThrowsSpanishDictPageParserException()
        {
            var parser = new SpanishDictPageParser();

            Action act = () => parser.ParseDefinition(LoadTestObject("mitologo"));

            act.Should().Throw<Exceptions.SpanishDictPageParserException>();
        }

        [TestMethod]
        public void ParseDefinition_For123_ThrowsSpanishDictPageParserException()
        {
            var parser = new SpanishDictPageParser();

            Action act = () => parser.ParseDefinition(LoadTestObject("123"));

            act.Should().Throw<Exceptions.SpanishDictPageParserException>();
        }

        [TestMethod]
        public void ParseDefinition_ForSaltamontes_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("saltamontes"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("saltamontes");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (animal)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(animal)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("grasshopper");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Los saltamontes pueden saltar muy alto.");
            example.Translation.Should().Be("Grasshoppers can jump really high.");

            meaning.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/5bf100e5-da54-4be6-a55c-281edcd08b10.jpg");
        }

        [TestMethod]
        public void ParseDefinition_ForIndígena_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("indígena"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("indígena");
            definition.PartOfSpeech.Should().Be("adjective");

            definition.Contexts.Should().HaveCount(1);

            // 1. (of native origins)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(of native origins)");
            context.Meanings.Should().HaveCount(2);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("indigenous");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("El gobierno quiere preservar el folclor y las tradiciones indígenas.");
            example.Translation.Should().Be("The government wants to preserve the indigenous folklore and traditions.");

            meaning.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/native%252C%2520indigenous.jpg");

            meaning = context.Meanings.Skip(1).First();
            meaning.Original.Should().Be("native");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("La comunidad indígena no está de acuerdo con la tala del bosque.");
            example.Translation.Should().Be("The native community is against the clearing of the forest.");
        }

        [TestMethod]
        public void ParseDefinition_ForIglesia_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("iglesia"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("iglesia");
            definition.PartOfSpeech.Should().Be("feminine noun");

            definition.Contexts.Should().HaveCount(1);

            // 1. (religious)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(religious)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("church");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Vámonos a la iglesia que la misa comienza pronto.");
            example.Translation.Should().Be("Let's go to the church; mass starts soon.");

            meaning.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/temple%253B%2520church.jpg");
        }

        [TestMethod]
        public void ParseDefinition_ForVaso_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("vaso"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("vaso");
            definition.PartOfSpeech.Should().Be("masculine noun");

            definition.Contexts.Should().HaveCountGreaterThan(1);

            // 1. (tableware)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(tableware)");
            context.Meanings.Should().HaveCountGreaterThan(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("glass");
            meaning.Examples.Should().HaveCount(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("Toma un vaso del estante si tienes sed.");
            example.Translation.Should().Be("Get a glass from the shelf if you're thirsty.");

            meaning.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/Glass%2520%2528empty%2529.jpg");
        }

        [TestMethod]
        public void ParseDefinition_ForNoTePreocupes_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            SpanishDictDefinition? definition = parser.ParseDefinition(LoadTestObject("NoTePreocupes"));

            definition.Should().NotBeNull();

            SpanishDictContext context;
            Meaning meaning;
            Models.Example example;

            definition.WordES.Should().Be("no te preocupes");
            definition.PartOfSpeech.Should().Be("phrase");

            definition.Contexts.Should().HaveCount(1);

            // 1. (general)
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(general)");
            context.Meanings.Should().HaveCount(1);

            meaning = context.Meanings.First();
            meaning.Original.Should().Be("don't worry");
            meaning.Examples.Should().HaveCountGreaterThan(1);
            example = meaning.Examples.First();
            example.Original.Should().Be("No te preocupes por los resultados del examen.");
            example.Translation.Should().Be("Don't worry about the results of the test.");

            meaning.ImageUrl.Should().Be("https://sd-hegemone-production.sdcdns.com/dictionary-images/300/don%2527t%2520worry.jpg");
        }

        #endregion

        #region Tests for ParseVariants

        [TestMethod]
        public void ParseVariants_ForAconsejar_Returns2Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("aconsejar"));

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("aconsejar (transitive verb)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/aconsejar?n=0&p=0");

            variants[1].Word.Should().Be("aconsejarse (pronominal verb)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/aconsejar?n=1&p=0");
        }

        [TestMethod]
        public void ParseVariants_ForAfeitar_Returns2Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("Afeitar"));

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("afeitar (transitive verb)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/afeitar?n=0&p=0");

            variants[1].Word.Should().Be("afeitarse (reflexive verb)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/afeitar?n=1&p=0");
        }

        [TestMethod]
        public void ParseVariants_ForÁguila_Returns2Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("Águila"));

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("águila (feminine noun)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/%c3%a1guila?n=0&p=0");

            variants[1].Word.Should().Be("águila (interjection)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/%c3%a1guila?n=0&p=1");
        }

        [TestMethod]
        public void ParseVariants_ForAprovechar_Returns3Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("Aprovechar"));

            variants.Should().HaveCount(3);

            variants[0].Word.Should().Be("aprovechar (transitive verb)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/aprovechar?n=0&p=0");

            variants[1].Word.Should().Be("aprovechar (intransitive verb)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/aprovechar?n=0&p=1");

            variants[2].Word.Should().Be("aprovecharse (pronominal verb)");
            variants[2].Url.Should().Be("https://www.spanishdict.com/translate/aprovechar?n=1&p=0");
        }

        [TestMethod]
        public void ParseVariants_ForAcensor_Returns1Variant()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("ascensor"));

            variants.Should().HaveCount(1);

            variants[0].Word.Should().Be("ascensor (masculine noun)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/ascensor?n=0&p=0");
        }

        [TestMethod]
        public void ParseVariants_ForCoche_Returns1Variant()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("Coche"));

            variants.Should().HaveCount(1);

            variants[0].Word.Should().Be("coche (masculine noun)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/coche?n=0&p=0");
        }

        [TestMethod]
        public void ParseVariants_ForGuay_Returns3Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("Guay"));

            variants.Should().HaveCount(3);

            variants[0].Word.Should().Be("guay (interjection)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/guay?n=0&p=0");

            variants[1].Word.Should().Be("guay (adjective)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/guay?n=0&p=1");

            variants[2].Word.Should().Be("guay (adverb)");
            variants[2].Url.Should().Be("https://www.spanishdict.com/translate/guay?n=0&p=2");
        }

        [TestMethod]
        public void ParseVariants_ForHipócrita_Returns2Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("hipócrita"));

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("hipócrita (masculine or feminine noun)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/hip%c3%b3crita?n=0&p=0");

            variants[1].Word.Should().Be("hipócrita (adjective)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/hip%c3%b3crita?n=0&p=1");
        }

        [TestMethod]
        public void ParseVariants_ForIndígena_Returns2Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("indígena"));

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("indígena (adjective)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/ind%c3%adgena?n=0&p=0");

            variants[1].Word.Should().Be("indígena (masculine or feminine noun)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/ind%c3%adgena?n=0&p=1");
        }

        [TestMethod]
        public void ParseVariants_ForIndigente_Returns2Variants()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("indigente"));

            variants.Should().HaveCount(2);

            variants[0].Word.Should().Be("indigente (adjective)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/indigente?n=0&p=0");

            variants[1].Word.Should().Be("indigente (masculine or feminine noun)");
            variants[1].Url.Should().Be("https://www.spanishdict.com/translate/indigente?n=0&p=1");
        }

        [TestMethod]
        public void ParseVariants_ForVeneno_Returns1Variant()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("veneno"));

            variants.Should().HaveCount(1);

            variants[0].Word.Should().Be("veneno (masculine noun)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/veneno?n=0&p=0");
        }

        [TestMethod]
        public void ParseVariants_ForWasapear_Returns1Variant()
        {
            var parser = new SpanishDictPageParser();

            List<Models.Variant> variants = parser.ParseVariants(LoadTestObject("wasapear"));

            variants.Should().HaveCount(1);

            variants[0].Word.Should().Be("wasapear (transitive verb)");
            variants[0].Url.Should().Be("https://www.spanishdict.com/translate/wasapear?n=0&p=0");
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
