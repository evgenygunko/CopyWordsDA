using System.Reflection;
using CopyWords.Parsers.Models;
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

        #region ParseWord tests

        [DataTestMethod]
        [DataRow("afeitar")]
        [DataRow("águila")]
        [DataRow("aprovechar")]
        [DataRow("costar un ojo de la cara")]
        [DataRow("mitologo")]
        [DataRow("saltamontes")]
        public void ParseWord_Should_ReturnWordFromModel(string word)
        {
            var parser = new SpanishDictPageParser();

            string? result = parser.ParseWord(LoadTestObject(word));

            result.Should().Be(word);
        }

        #endregion

        #region ParseSound tests

        [DataTestMethod]
        [DataRow("afeitar", "4189")]
        [DataRow("águila", "13701")]
        [DataRow("aprovechar", "13603")]
        [DataRow("costar un ojo de la cara", null)]
        public void ParseSound_Should_ReturnSoundFileIdFromModel(string word, string expectedSoundId)
        {
            var parser = new SpanishDictPageParser();

            string? result = parser.ParseSound(LoadTestObject(word));

            result.Should().Be(expectedSoundId);
        }

        #endregion

        #region ParseTranslations tests

        [TestMethod]
        public void ParseTranslations_ForAfeitar_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("afeitar")).ToList();

            result.Should().HaveCount(2);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            // afeitar
            variant = result[0];
            variant.WordES.Should().Be("afeitar");
            variant.Type.Should().Be("transitive verb");

            variant.Contexts.Should().HaveCount(1);
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(to remove hair)");
            context.Translations.Should().HaveCount(1);
            translation = context.Translations.First();
            translation.English.Should().Be("to shave");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Para el verano, papá decidió afeitar al perro.");
            example.ExampleEN.Should().Be("For the summer, dad decided to shave the dog.");

            // afeitarse
            variant = result[1];
            variant.WordES.Should().Be("afeitarse");
            variant.Type.Should().Be("reflexive verb");

            variant.Contexts.Should().HaveCount(1);
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(to shave oneself)");
            context.Translations.Should().HaveCount(1);
            translation = context.Translations.First();
            translation.English.Should().Be("to shave");
            example = translation.Examples.First();
            example.ExampleES.Should().Be("¿Con qué frecuencia te afeitas la barba?");
            example.ExampleEN.Should().Be("How often do you shave your beard?");
        }

        [TestMethod]
        public void ParseTranslations_ForTrotar_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("trotar")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            variant = result[0];
            variant.WordES.Should().Be("trotar");
            variant.Type.Should().Be("intransitive verb");

            variant.Contexts.Should().HaveCount(3);

            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(to run) (Mexico)");
            context.Translations.Should().HaveCount(1);
            translation = context.Translations.First();
            translation.English.Should().Be("to jog");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Salgo a trotar todas las mañanas.");
            example.ExampleEN.Should().Be("I go jogging every morning.");

            context = variant.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(horseback riding)");
            context.Translations.Should().HaveCount(1);
            translation = context.Translations.First();
            translation.English.Should().Be("to trot");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Carolina se levanta temprano cada día a sacar el caballo a trotar.");
            example.ExampleEN.Should().Be("Carolina gets up early every day to take the horse out to trot.");

            context = variant.Contexts.Skip(2).First();
            context.ContextEN.Should().Be("(colloquial) (to bustle about)");
            context.Translations.Should().HaveCount(1);
            translation = context.Translations.First();
            translation.English.Should().Be("to rush around");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Ya me cansé de estar trotando todo el día.");
            example.ExampleEN.Should().Be("I'm tired of rushing around all day.");
        }

        [TestMethod]
        public void ParseTranslations_ForCoche_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("coche")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            variant = result[0];
            variant.WordES.Should().Be("coche");
            variant.Type.Should().Be("masculine noun");

            variant.Contexts.Should().HaveCount(4);

            // 1. (vehicle)
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(vehicle)");
            context.Translations.Should().HaveCount(2);

            translation = context.Translations.First();
            translation.English.Should().Be("car");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Mi coche no prende porque tiene una falla en el motor.");
            example.ExampleEN.Should().Be("My car won't start because of a problem with the engine.");

            translation = context.Translations.Skip(1).First();
            translation.English.Should().Be("automobile");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Todos estos coches tienen bolsas de aire.");
            example.ExampleEN.Should().Be("All these automobiles have airbags.");

            // 2. (vehicle led by horses)
            context = variant.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(vehicle led by horses)");
            context.Translations.Should().HaveCount(2);

            translation = context.Translations.First();
            translation.English.Should().Be("carriage");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Los monarcas llegaron en un coche elegante.");
            example.ExampleEN.Should().Be("The monarchs arrived in an elegant carriage.");

            translation = context.Translations.Skip(1).First();
            translation.English.Should().Be("coach");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.");
            example.ExampleEN.Should().Be("Horse-drawn coaches were used much more before the invention of the automobile.");

            // ...
        }

        [TestMethod]
        public void ParseTranslations_ForHipócrita_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("hipócrita")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            // el hipócrita, la hipócrita
            variant = result[0];
            variant.WordES.Should().Be("el hipócrita, la hipócrita");
            variant.Type.Should().Be("masculine or feminine noun");

            variant.Contexts.Should().HaveCount(2);

            // 1. (false person)
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(false person)");
            context.Translations.Should().HaveCount(1);

            translation = context.Translations.First();
            translation.English.Should().Be("hypocrite");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Es una hipócrita. Pues y no va por ahí criticándome a mis espaldas.");
            example.ExampleEN.Should().Be("She's a hypocrite. It turns out she goes around criticizing me behind my back.");

            // (false)
            context = variant.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(false)");
            context.Translations.Should().HaveCount(1);

            translation = context.Translations.First();
            translation.English.Should().Be("hypocritical");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("No soporto esa sonrisa hipócrita que tiene.");
            example.ExampleEN.Should().Be("I cannot stand that hypocritical smile of his.");
        }

        [TestMethod]
        public void ParseTranslations_ForGuay_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("guay")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            // guay
            variant = result[0];
            variant.WordES.Should().Be("guay");
            variant.Type.Should().Be("interjection");

            variant.Contexts.Should().HaveCount(3);

            // 1. (colloquial) (used to express approval) (Spain)
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(colloquial) (used to express approval) (Spain)");
            context.Translations.Should().HaveCount(2);

            translation = context.Translations.First();
            translation.English.Should().Be("cool (colloquial)");
            translation.Examples.Should().HaveCount(2);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("¿Quieres que veamos la peli en mi ordenador? - ¡Guay, tío!");
            example.ExampleEN.Should().Be("Do you want to watch the movie on my computer? - Cool, man!");
            example = translation.Examples.Skip(1).First();
            example.ExampleES.Should().Be("¡Gané un viaje a Francia! - ¡Guay!");
            example.ExampleEN.Should().Be("I won a trip to France! - Cool!");

            // 2. (colloquial) (extremely good) (Spain)
            context = variant.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(colloquial) (extremely good) (Spain)");
            context.Translations.Should().HaveCount(2);

            translation = context.Translations.First();
            translation.English.Should().Be("cool (colloquial)");
            translation.Examples.Should().HaveCount(2);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("La fiesta de anoche estuvo muy guay.");
            example.ExampleEN.Should().Be("Last night's party was really cool.");
            example = translation.Examples.Skip(1).First();
            example.ExampleES.Should().Be("Tus amigos son guays, Roberto. ¿Dónde los conociste?");
            example.ExampleEN.Should().Be("Your friends are cool, Roberto. Where did you meet them?");

            translation = context.Translations.Skip(1).First();
            translation.English.Should().Be("super (colloquial)");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("¡Que monopatín tan guay!");
            example.ExampleEN.Should().Be("That's a super skateboard!");

            // 3. (colloquial) (extremely well) (Spain)
            context = variant.Contexts.Skip(2).First();
            context.ContextEN.Should().Be("(colloquial) (extremely well) (Spain)");
            context.Translations.Should().HaveCount(2);

            translation = context.Translations.First();
            translation.English.Should().Be("awesome (colloquial) (adjective)");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Nos lo pasamos guay en la fiesta de Reme.");
            example.ExampleEN.Should().Be("We had an awesome time at Reme's party.");

            translation = context.Translations.Skip(1).First();
            translation.English.Should().Be("great (colloquial) (adjective)");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Tu coche nos vendría guay para la excursión.");
            example.ExampleEN.Should().Be("It would be great if we could use your car for the trip.");
        }

        [TestMethod]
        public void ParseTranslations_ForClubNocturno_ReturnsTranslationsFromModel()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("club nocturno")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            variant = result[0];
            variant.WordES.Should().Be("club nocturno");
            variant.Type.Should().Be("masculine noun");

            variant.Contexts.Should().HaveCount(1);

            // 1. (general)
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(general)");
            context.Translations.Should().HaveCount(1);

            translation = context.Translations.First();
            translation.English.Should().Be("nightclub");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Este bar va a cerrar pronto, pero hay un club nocturno cerca de aquí que abre hasta las 3 am.");
            example.ExampleEN.Should().Be("This bar is going to close soon, but there's a nightclub nearby that's open until 3 am.");
        }

        [TestMethod]
        public void ParseTranslations_ForMitologo_ReturnsEmptyTranslationsList()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("mitologo")).ToList();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseTranslations_For123_ReturnsEmptyTranslationsList()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("123")).ToList();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseTranslations_ForSaltamontes_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("saltamontes")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            variant = result[0];
            variant.WordES.Should().Be("saltamontes");
            variant.Type.Should().Be("masculine noun");

            variant.Contexts.Should().HaveCount(1);

            // 1. (animal)
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(animal)");
            context.Translations.Should().HaveCount(1);

            translation = context.Translations.First();
            translation.English.Should().Be("grasshopper");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Los saltamontes pueden saltar muy alto.");
            example.ExampleEN.Should().Be("Grasshoppers can jump really high.");

            translation.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/5bf100e5-da54-4be6-a55c-281edcd08b10.jpg");
        }

        [TestMethod]
        public void ParseTranslations_ForIndígena_SetsImageUrl()
        {
            var parser = new SpanishDictPageParser();

            List<WordVariant> result = parser.ParseTranslations(LoadTestObject("indígena")).ToList();

            result.Should().HaveCount(1);

            WordVariant variant;
            Context context;
            Translation translation;
            Example example;

            variant = result[0];
            variant.WordES.Should().Be("indígena");
            variant.Type.Should().Be("adjective");

            variant.Contexts.Should().HaveCount(2);

            // 1. (of native origins)
            context = variant.Contexts.First();
            context.ContextEN.Should().Be("(of native origins)");
            context.Translations.Should().HaveCount(2);

            translation = context.Translations.First();
            translation.English.Should().Be("indigenous");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("El gobierno quiere preservar el folclor y las tradiciones indígenas.");
            example.ExampleEN.Should().Be("The government wants to preserve the indigenous folklore and traditions.");

            translation.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/native%252C%2520indigenous.jpg");

            translation = context.Translations.Skip(1).First();
            translation.English.Should().Be("native");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("La comunidad indígena no está de acuerdo con la tala del bosque.");
            example.ExampleEN.Should().Be("The native community is against the clearing of the forest.");

            // 2. (indigenous person)
            context = variant.Contexts.Skip(1).First();
            context.ContextEN.Should().Be("(indigenous person)");
            context.Translations.Should().HaveCount(1);

            translation = context.Translations.First();
            translation.English.Should().Be("native");
            translation.Examples.Should().HaveCount(1);
            example = translation.Examples.First();
            example.ExampleES.Should().Be("Este parque natural está protegido por los indígenas que habitan la zona.");
            example.ExampleEN.Should().Be("This natural park is protected by the natives that inhabit the area.");

            translation.ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/0ca649f9-134a-4210-ae48-2a8bbadb32cc.jpg");
        }

        #endregion

        #region Private methods

        private static Models.SpanishDict.WordJsonModel LoadTestObject(string word)
        {
            string htmlPagePath = Path.Combine(s_path, "TestPages", $"{word}.json");
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
