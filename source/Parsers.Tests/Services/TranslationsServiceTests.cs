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

            int definitionIndex = 1;
            foreach (Definition definition in wordModel.Definitions)
            {
                // Headword is taken from the first meaning
                Meaning firstMeaning = definition.Contexts.First().Meanings.First();

                Models.Translations.Input.Definition outputDefinition = result.Definitions.First(x => x.id == definitionIndex);
                outputDefinition.Headword.Text.Should().Be(definition.Headword.Original);
                outputDefinition.Headword.Meaning.Should().Be(firstMeaning.Original);
                outputDefinition.Headword.PartOfSpeech.Should().Be(definition.PartOfSpeech);
                outputDefinition.Headword.Examples.Should().HaveCount(firstMeaning.Examples.Count());

                outputDefinition.Meanings.Should().HaveCount(definition.Contexts.Sum(c => c.Meanings.Count()));
                outputDefinition.Meanings.Select(m => m.Text).Should().Contain(firstMeaning.Original);
                outputDefinition.Meanings.First(m => m.Text == firstMeaning.Original).Examples.Should().HaveCount(firstMeaning.Examples.Count());
                outputDefinition.Meanings.First().id.Should().Be(1);
                outputDefinition.Meanings.Last().id.Should().Be(outputDefinition.Meanings.Count());

                definitionIndex++;
            }
        }

        [TestMethod]
        public void CreateTranslationInputFromWordModel_ForAfeitar_AddsTwoDefinitionsToTranslationInput()
        {
            SourceLanguage sourceLanguage = SourceLanguage.Spanish;
            WordModel wordModel = CreateWordModelForAefitar();

            var sut = _fixture.Create<TranslationsService>();
            Models.Translations.Input.TranslationInput result = sut.CreateTranslationInputFromWordModel(sourceLanguage, wordModel);

            result.Should().NotBeNull();
            result.Version.Should().Be("2");
            result.SourceLanguage.Should().Be(sourceLanguage.ToString());
            result.DestinationLanguages.Should().HaveCount(2);

            result.Definitions.Should().HaveCount(2);

            Models.Translations.Input.Definition inputDefinition;
            Models.Translations.Input.Meaning inputMeaning;

            /***********************************************************************/
            // Afeitar
            /***********************************************************************/
            inputDefinition = result.Definitions.First();
            inputDefinition.id.Should().Be(1);
            inputDefinition.Headword.Text.Should().Be("afeitar");
            inputDefinition.Headword.Meaning.Should().Be("to shave");
            inputDefinition.Headword.PartOfSpeech.Should().Be("transitive verb");
            inputDefinition.Headword.Examples.Should().HaveCount(1);

            inputDefinition.Meanings.Should().HaveCount(1);
            inputMeaning = inputDefinition.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("to shave");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("Para el verano, papá decidió afeitar al perro.");

            /***********************************************************************/
            // Afeitarse
            /***********************************************************************/
            inputDefinition = result.Definitions.Last();
            inputDefinition.id.Should().Be(2);
            inputDefinition.Headword.Text.Should().Be("afeitarse");
            inputDefinition.Headword.Meaning.Should().Be("to shave");
            inputDefinition.Headword.PartOfSpeech.Should().Be("reflexive verb");
            inputDefinition.Headword.Examples.Should().HaveCount(1);

            inputDefinition.Meanings.Should().HaveCount(1);
            inputMeaning = inputDefinition.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("to shave");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("¿Con qué frecuencia te afeitas la barba?");
        }

        #endregion

        #region Tests for CreateWordModelFromTranslationOutput

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepWord()
        {
            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.Word.Should().Be(originalWordModel.Word);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepSoundUrl()
        {
            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.SoundUrl.Should().Be(originalWordModel.SoundUrl);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepSoundFileName()
        {
            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.SoundFileName.Should().Be(originalWordModel.SoundFileName);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_KeepVariations()
        {
            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            result.Variations.Should().BeEquivalentTo(originalWordModel.Variations);
        }

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_Should_SetTranslations()
        {
            // Arrange
            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            // Assert
            result.Definitions.Should().HaveCount(1);

            Definition definition = result.Definitions.First();
            Definition originalDefinition = originalWordModel.Definitions.First();

            definition.PartOfSpeech.Should().Be(originalDefinition.PartOfSpeech);
            definition.Endings.Should().Be(originalDefinition.Endings);

            // Check headword
            definition.Headword.Original.Should().Be(originalDefinition.Headword.Original);
            definition.Headword.Russian.Should().NotBeEmpty(); // <-- should be translated
            definition.Headword.English.Should().NotBeEmpty(); // <-- should be translated

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

                    meaning.Translation.Should().NotBeEmpty(); // <-- should be translated
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

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_WhenTranslateMeaningsIsFalse_DoesNotSetTranslations()
        {
            // Arrange
            const bool translateHeadword = true;
            const bool translateMeanings = false;

            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

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

            var originalWordModel = new WordModel(
                Word: _fixture.Create<string>(),
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: [
                    new Definition(
                        Headword: new Headword(Original: _fixture.Create<string>(), English: null, Russian: null),
                        PartOfSpeech: _fixture.Create<string>(),
                        Endings: _fixture.Create<string>(),
                        Contexts: [new Context(ContextEN: _fixture.Create<string>(), Position: _fixture.Create<string>(), Meanings: [_fixture.Create<Meaning>()])]
                    ),
                ],
                Variations: _fixture.CreateMany<Variant>().ToArray()
            );

            var translationOutput = CreateOutputWithOneDefinition();

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

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_ForAfeitar_ReturnsTwoDefinitions()
        {
            // Arrange
            WordModel originalWordModel = CreateWordModelForAefitar();

            var translationOutput = new Models.Translations.Output.TranslationOutput(
                [
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 1,
                        Headword: [
                            new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [ "брить" ]),
                            new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [ "to shave" ]),
                        ],
                        Meanings: [
                            new Models.Translations.Output.Meaning(
                                id: 1,
                                MeaningTranslations: [
                                    new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "брить"),
                                    new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "to shave (to remove hair)"),
                                ]
                            ),
                        ]
                    ),
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 2,
                        Headword: [
                            new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [ "брить себя" ]),
                            new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [ "to shave oneself", "to shave" ]),
                        ],
                        Meanings: [
                            new Models.Translations.Output.Meaning(
                                id: 1,
                                MeaningTranslations: [
                                    new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "бриться (бриться самому)"),
                                    new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "to shave (to shave oneself)"),
                                ]
                            ),
                        ]
                    )
                ]
            );

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            // Assert
            result.Definitions.Should().HaveCount(2);

            Definition definition;

            /***********************************************************************/
            // Afeitar
            /***********************************************************************/
            definition = result.Definitions.First();
            definition.PartOfSpeech.Should().Be("transitive verb");
            definition.Endings.Should().BeEmpty();

            // Check headword
            definition.Headword.Original.Should().Be("afeitar");
            definition.Headword.Russian.Should().Be("брить");
            definition.Headword.English.Should().Be("to shave");

            // Check contexts
            definition.Contexts.Should().HaveCount(1);
            Context context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to remove hair)");
            context.Position.Should().Be("1");

            // Check meanings
            context.Meanings.Should().HaveCount(1);
            Meaning meaning = context.Meanings.First();
            meaning.Original.Should().Be("to shave");
            meaning.Translation.Should().Be("брить");
            meaning.AlphabeticalPosition.Should().Be("a.");
            meaning.Tag.Should().BeNull();
            meaning.ImageUrl.Should().BeNull();
            meaning.Examples.Should().HaveCount(1);
            meaning.Examples.First().Original.Should().Be("Para el verano, papá decidió afeitar al perro.");
            meaning.Examples.First().Translation.Should().BeNull();

            /***********************************************************************/
            // Afeitarse
            /***********************************************************************/
            definition = result.Definitions.Last();
            definition.PartOfSpeech.Should().Be("reflexive verb");
            definition.Endings.Should().BeEmpty();

            // Check headword
            definition.Headword.Original.Should().Be("afeitarse");
            definition.Headword.Russian.Should().Be("брить себя");
            definition.Headword.English.Should().Be("to shave oneself, to shave");

            // Check contexts
            definition.Contexts.Should().HaveCount(1);
            context = definition.Contexts.First();
            context.ContextEN.Should().Be("(to shave oneself)");
            context.Position.Should().Be("1");

            // Check meanings
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("to shave");
            meaning.Translation.Should().Be("бриться (бриться самому)");
            meaning.AlphabeticalPosition.Should().Be("a.");
            meaning.Tag.Should().BeNull();
            meaning.ImageUrl.Should().BeNull();
            meaning.Examples.Should().HaveCount(1);
            meaning.Examples.First().Original.Should().Be("¿Con qué frecuencia te afeitas la barba?");
            meaning.Examples.First().Translation.Should().BeNull();
        }

        #endregion

        #region Private Methods

        private WordModel CreateWordModelForAefitar()
        {
            WordModel wordModel = new WordModel(
                Word: "afeitar",
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: new[]
                {
                    new Definition(
                        Headword: new Headword(Original: "afeitar", English: null, Russian: null),
                        PartOfSpeech: "transitive verb",
                        Endings: "",
                        Contexts: new[]
                        {
                            new Context(
                                ContextEN: "(to remove hair)",
                                Position: "1",
                                Meanings: new[]
                                {
                                    new Meaning(
                                        Original: "to shave",
                                        Translation: null,
                                        AlphabeticalPosition: "a.",
                                        Tag: null,
                                        ImageUrl: null,
                                        Examples: [ new Example(Original: "Para el verano, papá decidió afeitar al perro.", Translation: null) ]
                                    )
                                })
                        }),
                    new Definition(
                        Headword: new Headword(Original: "afeitarse", English: null, Russian: null),
                        PartOfSpeech: "reflexive verb",
                        Endings: "",
                        Contexts: new[]
                        {
                            new Context(
                                ContextEN: "(to shave oneself)",
                                Position: "1",
                                Meanings: new[]
                                {
                                    new Meaning(
                                        Original: "to shave",
                                        Translation: null,
                                        AlphabeticalPosition: "a.",
                                        Tag: null,
                                        ImageUrl: null,
                                        Examples: [ new Example(Original: "¿Con qué frecuencia te afeitas la barba?", Translation: null) ]
                                    )
                                })
                        })
                },
                Variations: Enumerable.Empty<Variant>());

            return wordModel;
        }

        private Models.Translations.Output.TranslationOutput CreateOutputWithOneDefinition()
        {
            var translationOutput = new Models.Translations.Output.TranslationOutput(
                Definitions: [
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 1,
                        Headword: [
                            new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [_fixture.Create<string>()]),
                            new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [_fixture.Create<string>()])
                        ],
                        Meanings: [
                            new Models.Translations.Output.Meaning(
                                id: 0,
                                MeaningTranslations: [
                                    new Models.Translations.Output.MeaningTranslation(Language: "en", Text: _fixture.Create<string>()),
                                    new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: _fixture.Create<string>())
                                ]
                            )
                        ]
                    )
                ]
            );

            return translationOutput;
        }

        #endregion
    }
}
