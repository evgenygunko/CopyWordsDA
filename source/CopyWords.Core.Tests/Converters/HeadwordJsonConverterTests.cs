using System.Text.Json;
using CopyWords.Core.Converters;
using CopyWords.Core.Models;
using FluentAssertions;

namespace CopyWords.Core.Tests.Converters
{
    [TestClass]
    public class HeadwordJsonConverterTests
    {
        #region Deserialization Tests

        [TestMethod]
        public void Read_WhenJsonHasTranslationProperty_DeserializesCorrectly()
        {
            string json = """
                {
                    "Original": "hund",
                    "English": "dog",
                    "Translation": "собака"
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Original.Should().Be("hund");
            result.English.Should().Be("dog");
            result.Translation.Should().Be("собака");
        }

        [TestMethod]
        public void Read_WhenJsonHasRussianProperty_DeserializesAsTranslation()
        {
            string json = """
                {
                    "Original": "hund",
                    "English": "dog",
                    "Russian": "собака"
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Original.Should().Be("hund");
            result.English.Should().Be("dog");
            result.Translation.Should().Be("собака");
        }

        [TestMethod]
        public void Read_WhenJsonHasBothTranslationAndRussian_TranslationTakesPrecedence()
        {
            string json = """
                {
                    "Original": "hund",
                    "English": "dog",
                    "Translation": "Hund",
                    "Russian": "собака"
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Translation.Should().Be("Hund");
        }

        [TestMethod]
        public void Read_WhenJsonHasNullTranslation_DeserializesCorrectly()
        {
            string json = """
                {
                    "Original": "hund",
                    "English": "dog",
                    "Translation": null
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Original.Should().Be("hund");
            result.English.Should().Be("dog");
            result.Translation.Should().BeNull();
        }

        [TestMethod]
        public void Read_WhenJsonHasNullEnglishAndTranslation_DeserializesCorrectly()
        {
            string json = """
                {
                    "Original": "hund",
                    "English": null,
                    "Translation": null
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Original.Should().Be("hund");
            result.English.Should().BeNull();
            result.Translation.Should().BeNull();
        }

        [TestMethod]
        public void Read_WhenPropertyNamesAreLowerCase_DeserializesCorrectly()
        {
            string json = """
                {
                    "original": "hund",
                    "english": "dog",
                    "translation": "собака"
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Original.Should().Be("hund");
            result.English.Should().Be("dog");
            result.Translation.Should().Be("собака");
        }

        [TestMethod]
        public void Read_WhenJsonHasUnknownProperties_IgnoresThem()
        {
            string json = """
                {
                    "Original": "hund",
                    "English": "dog",
                    "Translation": "собака",
                    "UnknownProperty": "some value"
                }
                """;

            var result = JsonSerializer.Deserialize<Headword>(json);

            result.Should().NotBeNull();
            result!.Original.Should().Be("hund");
            result.English.Should().Be("dog");
            result.Translation.Should().Be("собака");
        }

        #endregion

        #region Serialization Tests

        [TestMethod]
        public void Write_WhenHeadwordHasAllProperties_SerializesCorrectly()
        {
            var headword = new Headword("hund", "dog", "собака");

            string json = JsonSerializer.Serialize(headword);

            // Deserialize back to verify round-trip works
            var deserialized = JsonSerializer.Deserialize<Headword>(json);

            deserialized.Should().NotBeNull();
            deserialized!.Original.Should().Be("hund");
            deserialized.English.Should().Be("dog");
            deserialized.Translation.Should().Be("собака");

            // Verify it uses Translation property name (not Russian)
            json.Should().NotContain("Russian");
        }

        [TestMethod]
        public void Write_WhenTranslationIsNull_SerializesAsNull()
        {
            var headword = new Headword("hund", "dog", null);

            string json = JsonSerializer.Serialize(headword);

            json.Should().Contain("\"Translation\":null");
        }

        #endregion
    }
}
