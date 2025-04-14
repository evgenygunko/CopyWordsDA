using AutoFixture;
using CopyWords.Parsers.Models;
using CopyWords.Parsers.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CopyWords.Parsers.Tests.Services
{
    [TestClass]
    public class TranslationsServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for TranslateAsync

        [TestMethod]
        public void TranslateAsync_Should_CallTranslatorAPIClient()
        {
            Options options = new Options(SourceLanguage.Danish, _fixture.Create<Uri>().ToString(), TranslateHeadword: true, TranslateMeanings: false);

            WordModel wordModel = _fixture.Create<WordModel>();

            var translatorAPIClientMock = _fixture.Freeze<Mock<ITranslatorAPIClient>>();

            var sut = _fixture.Create<TranslationsService>();
            _ = sut.TranslateAsync(options, wordModel);

            translatorAPIClientMock.Verify(x => x.TranslateAsync(options.TranslatorApiURL!, It.IsAny<Models.Translations.Input.TranslationInput>()));
        }

        #endregion

        #region Tests for CreateTranslationInputFromWordModel

        [TestMethod]
        public void CreateTranslationInputFromWordModel_Should_ReturnTranslationInput()
        {
            SourceLanguage sourceLanguage = SourceLanguage.Danish;
            WordModel wordModel = _fixture.Create<WordModel>();

            var sut = _fixture.Create<TranslationsService>();
            Models.Translations.Input.TranslationInput result = sut.CreateTranslationInputFromWordModel(sourceLanguage, wordModel);

            result.Should().NotBeNull();
            result.Version.Should().Be("2");
            result.SourceLanguage.Should().Be(sourceLanguage.ToString());
            result.DestinationLanguages.Should().HaveCount(2);

            Definition firstDefinition = wordModel.Definitions.First();
            Meaning firstMeaning = firstDefinition.Contexts.First().Meanings.First();

            Models.Translations.Input.Definition outputDefinition = result.Definitions.First();
            outputDefinition.Headword.Text.Should().Be(wordModel.Word);
            outputDefinition.Headword.Meaning.Should().Be(firstMeaning.Original);
            outputDefinition.Headword.PartOfSpeech.Should().Be(firstDefinition.PartOfSpeech);
            outputDefinition.Headword.Examples.Should().HaveCount(firstMeaning.Examples.Count());

            outputDefinition.Meanings.Should().HaveCount(wordModel.Definitions.Sum(d => d.Contexts.Sum(c => c.Meanings.Count())));
            outputDefinition.Meanings.Select(m => m.Text).Should().Contain(firstMeaning.Original);
            outputDefinition.Meanings.First(m => m.Text == firstMeaning.Original).Examples.Should().HaveCount(firstMeaning.Examples.Count());
            outputDefinition.Meanings.First().id.Should().Be(0);
            outputDefinition.Meanings.Last().id.Should().Be(outputDefinition.Meanings.Count() - 1);
        }

        #endregion

        #region Tests for CreateWordModelFromTranslationOutput

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepWord()
        {
            WordModel originalWordModel = _fixture.Create<WordModel>();
            Models.Translations.Output.TranslationOutput translationOutput = _fixture.Create<Models.Translations.Output.TranslationOutput>();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.Word.Should().Be(originalWordModel.Word);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepSoundUrl()
        {
            WordModel originalWordModel = _fixture.Create<WordModel>();
            Models.Translations.Output.TranslationOutput translationOutput = _fixture.Create<Models.Translations.Output.TranslationOutput>();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.SoundUrl.Should().Be(originalWordModel.SoundUrl);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepSoundFileName()
        {
            WordModel originalWordModel = _fixture.Create<WordModel>();
            Models.Translations.Output.TranslationOutput translationOutput = _fixture.Create<Models.Translations.Output.TranslationOutput>();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.SoundFileName.Should().Be(originalWordModel.SoundFileName);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepVariations()
        {
            WordModel originalWordModel = _fixture.Create<WordModel>();
            Models.Translations.Output.TranslationOutput translationOutput = _fixture.Create<Models.Translations.Output.TranslationOutput>();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.Variations.Should().BeEquivalentTo(originalWordModel.Variations);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_SetDefinitions()
        {
            // Arrange
            WordModel originalWordModel = _fixture.Create<WordModel>();

            // We need to generate a list of translated meanings where at least one translation is Russian, so we can't generate TranslationOutput with AutoFixture.
            int meaningsCount = originalWordModel.Definitions.SelectMany(d => d.Contexts).SelectMany(c => c.Meanings).Count();

            var outputMeanings = new List<Models.Translations.Output.Meaning>();
            for (int i = 0; i < meaningsCount; i++)
            {
                var translatedMeanings = new List<Models.Translations.Output.MeaningTranslation>
                {
                    new Models.Translations.Output.MeaningTranslation(Language: "en", Text: _fixture.Create<string>()),
                    new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: _fixture.Create<string>())
                };

                outputMeanings.Add(new Models.Translations.Output.Meaning(
                    id: i,
                    MeaningTranslations: translatedMeanings.ToArray())
                );
            }

            int headwordsCount = originalWordModel.Definitions.Select(d => d.Headword).Count();
            var outputHeadwords = new List<Models.Translations.Output.Headword>();

            for (int i = 0; i < headwordsCount; i++)
            {
                outputHeadwords.Add(new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [_fixture.Create<string>()]));
                outputHeadwords.Add(new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [_fixture.Create<string>()]));
            }

            var translationOutput = new Models.Translations.Output.TranslationOutput(
                [
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 0,
                        Headword: outputHeadwords.ToArray(),
                        Meanings: outputMeanings.ToArray())
                ]
            );

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            // Assert
            result.Definitions.Should().HaveCount(originalWordModel.Definitions.Count());

            // Check definitions
            IEnumerator<Definition> originalDefinitionsEnumerator = originalWordModel.Definitions.GetEnumerator();
            foreach (Definition definition in result.Definitions)
            {
                originalDefinitionsEnumerator.MoveNext();
                Definition originalDefinition = originalDefinitionsEnumerator.Current;

                definition.PartOfSpeech.Should().Be(originalDefinition.PartOfSpeech);
                definition.Endings.Should().Be(originalDefinition.Endings);

                // Check headword
                definition.Headword.Original.Should().Be(originalDefinition.Headword.Original);

                var outputDefinition = translationOutput.Definitions.First();
                outputDefinition.Headword
                    .SelectMany(headword => headword.HeadwordTranslations)
                    .Should().Contain(definition.Headword.English);
                outputDefinition.Headword
                    .SelectMany(headword => headword.HeadwordTranslations)
                    .Should().Contain(definition.Headword.Russian);

                // Check contexts
                definition.Contexts.Should().HaveCount(originalDefinition.Contexts.Count());
                IEnumerator<Context> originalContextsEnumerator = originalDefinition.Contexts.GetEnumerator();
                foreach (Context context in definition.Contexts)
                {
                    originalContextsEnumerator.MoveNext();
                    Context originalContext = originalContextsEnumerator.Current;

                    context.ContextEN.Should().Be(originalContext.ContextEN);
                    context.Position.Should().Be(originalContext.Position);

                    // Check meanings
                    context.Meanings.Should().HaveCount(originalContext.Meanings.Count());
                    IEnumerator<Meaning> originalMeaningsEnumerator = originalContext.Meanings.GetEnumerator();
                    foreach (Meaning meaning in context.Meanings)
                    {
                        originalMeaningsEnumerator.MoveNext();
                        Meaning originalMeaning = originalMeaningsEnumerator.Current;

                        meaning.Original.Should().Be(originalMeaning.Original);

                        // the most interesting part
                        outputDefinition.Meanings
                            .SelectMany(meaning => meaning.MeaningTranslations)
                            .Where(mt => mt.Language == "ru")
                            .Select(mt => mt.Text)
                            .Should().Contain(meaning.Translation);

                        meaning.AlphabeticalPosition.Should().Be(originalMeaning.AlphabeticalPosition);
                        meaning.Tag.Should().Be(originalMeaning.Tag);
                        meaning.ImageUrl.Should().Be(originalMeaning.ImageUrl);

                        // Check examples
                        meaning.Examples.Should().BeEquivalentTo(originalMeaning.Examples);
                        IEnumerator<Example> originalExamplesEnumerator = originalMeaning.Examples.GetEnumerator();
                        foreach (Example example in meaning.Examples)
                        {
                            originalExamplesEnumerator.MoveNext();
                            Example originalExample = originalExamplesEnumerator.Current;

                            example.Original.Should().Be(originalExample.Original);
                            example.Translation.Should().Be(originalExample.Translation);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_MatchMeanings()
        {
            // Arrange
            WordModel originalWordModel = _fixture.Create<WordModel>();

            // We need to generate a list of translated meanings where at least one translation is Russian, so we can't generate TranslationOutput with AutoFixture.
            int meaningsCount = originalWordModel.Definitions.SelectMany(d => d.Contexts).SelectMany(c => c.Meanings).Count();

            var outputMeanings = new List<Models.Translations.Output.Meaning>();
            for (int i = 0; i < meaningsCount; i++)
            {
                var translatedMeanings = new List<Models.Translations.Output.MeaningTranslation>
                {
                    new Models.Translations.Output.MeaningTranslation(Language: "en", Text: _fixture.Create<string>()),
                    new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: _fixture.Create<string>())
                };

                outputMeanings.Add(new Models.Translations.Output.Meaning(
                    id: i,
                    MeaningTranslations: translatedMeanings.ToArray())
                );
            }

            int headwordsCount = originalWordModel.Definitions.Select(d => d.Headword).Count();
            var outputHeadwords = new List<Models.Translations.Output.Headword>();

            for (int i = 0; i < headwordsCount; i++)
            {
                outputHeadwords.Add(new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [_fixture.Create<string>()]));
                outputHeadwords.Add(new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [_fixture.Create<string>()]));
            }

            var translationOutput = new Models.Translations.Output.TranslationOutput(
                [
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 0,
                        Headword: outputHeadwords.ToArray(),
                        Meanings: outputMeanings.ToArray())
                ]
            );

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            // Assert
            var originalMeanings = originalWordModel.Definitions.SelectMany(d => d.Contexts).SelectMany(c => c.Meanings);
            var meanings = result.Definitions.SelectMany(d => d.Contexts).SelectMany(c => c.Meanings);

            var meaningTranslations = meanings.Select(m => m.Translation);
            meaningTranslations.Distinct().Should().HaveCount(originalMeanings.Count());
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_WhenTranslateMeaningsIsFalse_DoesNotSetTranslations()
        {
            // Arrange
            const bool translateHeadword = true;
            const bool translateMeanings = false;

            WordModel originalWordModel = _fixture.Create<WordModel>();
            var translationOutput = _fixture.Create<Models.Translations.Output.TranslationOutput>();

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword, translateMeanings, originalWordModel, translationOutput);

            // Assert
            // Check definitions
            IEnumerator<Definition> originalDefinitionsEnumerator = originalWordModel.Definitions.GetEnumerator();
            foreach (Definition definition in result.Definitions)
            {
                originalDefinitionsEnumerator.MoveNext();
                Definition originalDefinition = originalDefinitionsEnumerator.Current;

                // Check contexts
                IEnumerator<Context> originalContextsEnumerator = originalDefinition.Contexts.GetEnumerator();
                foreach (Context context in definition.Contexts)
                {
                    originalContextsEnumerator.MoveNext();
                    Context originalContext = originalContextsEnumerator.Current;

                    // Check meanings
                    context.Meanings.Should().HaveCount(originalContext.Meanings.Count());
                    IEnumerator<Meaning> originalMeaningsEnumerator = originalContext.Meanings.GetEnumerator();
                    foreach (Meaning meaning in context.Meanings)
                    {
                        originalMeaningsEnumerator.MoveNext();
                        Meaning originalMeaning = originalMeaningsEnumerator.Current;

                        meaning.Original.Should().Be(originalMeaning.Original);

                        // the most interesting part
                        meaning.Translation.Should().Be(originalMeaning.Translation);
                    }
                }
            }
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_WhenTranslateHeadWordIsFalse_DoesNotSetTranslations()
        {
            // Arrange
            const bool translateHeadword = false;
            const bool translateMeanings = true;

            WordModel originalWordModel = _fixture.Create<WordModel>();
            var translationOutput = _fixture.Create<Models.Translations.Output.TranslationOutput>();

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword, translateMeanings, originalWordModel, translationOutput);

            // Assert
            // Check definitions
            IEnumerator<Definition> originalDefinitionsEnumerator = originalWordModel.Definitions.GetEnumerator();
            foreach (Definition definition in result.Definitions)
            {
                originalDefinitionsEnumerator.MoveNext();
                Definition originalDefinition = originalDefinitionsEnumerator.Current;

                // Check headword
                definition.Headword.Original.Should().Be(originalDefinition.Headword.Original);
                definition.Headword.English.Should().Be(originalDefinition.Headword.English);
                definition.Headword.Russian.Should().Be(originalDefinition.Headword.Russian);
            }
        }

        #endregion
    }
}
