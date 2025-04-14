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

                Models.Translations.Input.Definition inputDefinition = result.Definitions.First(x => x.id == definitionIndex);
                inputDefinition.Headword.Text.Should().Be(definition.Headword.Original);
                inputDefinition.Headword.Meaning.Should().Be(firstMeaning.Original);
                inputDefinition.Headword.PartOfSpeech.Should().Be(definition.PartOfSpeech);
                inputDefinition.Headword.Examples.Should().HaveCount(firstMeaning.Examples.Count());

                inputDefinition.Contexts.Should().HaveCount(definition.Contexts.Count());

                int contextIndex = 1;
                foreach (Context context in definition.Contexts)
                {
                    Models.Translations.Input.Context inputContext = inputDefinition.Contexts.First(x => x.id == contextIndex);
                    inputContext.Meanings.Should().HaveCount(context.Meanings.Count());

                    Models.Translations.Input.Meaning firstInputMeaning = inputContext.Meanings.First();
                    Meaning firstMeaningInContext = context.Meanings.First();

                    firstInputMeaning.Text.Should().Be(firstMeaningInContext.Original);
                    firstInputMeaning.Examples.Should().HaveCount(firstMeaningInContext.Examples.Count());
                    firstInputMeaning.id.Should().Be(1);

                    inputContext.Meanings.Last().id.Should().Be(inputContext.Meanings.Count());

                    contextIndex++;
                }

                definitionIndex++;
            }
        }

        [TestMethod]
        public void CreateTranslationInputFromWordModel_ForAfeitar_Adds2DefinitionsToTranslationInput()
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
            Models.Translations.Input.Context inputContext;
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

            inputContext = inputDefinition.Contexts.First();
            inputContext.Meanings.Should().HaveCount(1);
            inputMeaning = inputContext.Meanings.First();
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

            inputContext = inputDefinition.Contexts.First();
            inputContext.Meanings.Should().HaveCount(1);
            inputMeaning = inputContext.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("to shave");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("¿Con qué frecuencia te afeitas la barba?");
        }

        [TestMethod]
        public void CreateTranslationInputFromWordModel_ForCoche_Adds4ContextsToTranslationInput()
        {
            SourceLanguage sourceLanguage = SourceLanguage.Spanish;
            WordModel wordModel = CreateWordModelForCoche();

            var sut = _fixture.Create<TranslationsService>();
            Models.Translations.Input.TranslationInput result = sut.CreateTranslationInputFromWordModel(sourceLanguage, wordModel);

            result.Should().NotBeNull();
            result.Version.Should().Be("2");
            result.SourceLanguage.Should().Be(sourceLanguage.ToString());
            result.DestinationLanguages.Should().HaveCount(2);

            result.Definitions.Should().HaveCount(1);

            Models.Translations.Input.Definition inputDefinition = result.Definitions.First();
            inputDefinition.id.Should().Be(1);
            inputDefinition.Headword.Text.Should().Be("el coche");
            inputDefinition.Headword.Meaning.Should().Be("car");
            inputDefinition.Headword.PartOfSpeech.Should().Be("masculine noun");
            inputDefinition.Headword.Examples.Should().HaveCount(1);

            Models.Translations.Input.Context inputContext;
            Models.Translations.Input.Meaning inputMeaning;

            /***********************************************************************/
            // Context 1
            /***********************************************************************/
            inputContext = inputDefinition.Contexts.First();
            inputContext.ContextEN.Should().Be("(vehicle)");
            inputContext.Meanings.Should().HaveCount(1);
            inputMeaning = inputContext.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("car");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("Mi coche no prende porque tiene una falla en el motor.");

            /***********************************************************************/
            // Context 2
            /***********************************************************************/
            inputContext = inputDefinition.Contexts.Skip(1).First();
            inputContext.ContextEN.Should().Be("(vehicle led by horses)");
            inputContext.Meanings.Should().HaveCount(1);
            inputMeaning = inputContext.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("carriage");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("Los monarcas llegaron en un coche elegante.");

            /***********************************************************************/
            // Context 3
            /***********************************************************************/
            inputContext = inputDefinition.Contexts.Skip(2).First();
            inputContext.ContextEN.Should().Be("(train car)");
            inputContext.Meanings.Should().HaveCount(1);
            inputMeaning = inputContext.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("car");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("Tu mamá y yo vamos a pasar al coche comedor para almorzar.");

            /***********************************************************************/
            // Context 4
            /***********************************************************************/
            inputContext = inputDefinition.Contexts.Skip(3).First();
            inputContext.ContextEN.Should().Be("(for babies)");
            inputContext.Meanings.Should().HaveCount(1);
            inputMeaning = inputContext.Meanings.First();
            inputMeaning.id.Should().Be(1);
            inputMeaning.Text.Should().Be("stroller");
            inputMeaning.Examples.Should().HaveCount(1);
            inputMeaning.Examples.First().Should().Be("La niñita no se quería subir al coche. Quería ir caminando.");
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
        public void CreateWordModelFromTranslationOutput_ForAfeitar_Returns2Definitions()
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
                        Contexts: [
                            new Models.Translations.Output.Context(
                                id: 1,
                                Meanings: [
                                    new Models.Translations.Output.Meaning(
                                        id: 1,
                                        MeaningTranslations: [
                                            new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "брить"),
                                            new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "to shave (to remove hair)"),
                                        ]
                                    )
                                ]
                            )
                        ]
                    ),
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 2,
                        Headword: [
                            new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [ "брить себя" ]),
                            new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [ "to shave oneself", "to shave" ]),
                        ],
                        Contexts: [
                            new Models.Translations.Output.Context(
                                id: 1,
                                Meanings: [
                                    new Models.Translations.Output.Meaning(
                                        id: 1,
                                        MeaningTranslations: [
                                            new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "бриться (бриться самому)"),
                                            new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "to shave (to shave oneself)"),
                                        ]
                                    )
                                ]
                            )
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

        [TestMethod]
        public void CreateWordModelFromTranslationOutput_ForCoche_Returns4Contexts()
        {
            // Arrange
            WordModel originalWordModel = CreateWordModelForCoche();

            var translationOutput = new Models.Translations.Output.TranslationOutput(
                [
                    new Models.Translations.Output.DefinitionTranslations(
                        id: 1,
                        Headword: [
                            new Models.Translations.Output.Headword(Language: "ru", HeadwordTranslations: [ "автомобиль" ]),
                            new Models.Translations.Output.Headword(Language: "en", HeadwordTranslations: [ "car" ]),
                        ],
                        Contexts: [
                            new Models.Translations.Output.Context(
                                id: 1,
                                Meanings: [
                                    new Models.Translations.Output.Meaning(
                                        id: 1,
                                        MeaningTranslations: [
                                            new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "автомобиль"),
                                            new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "car"),
                                        ]
                                    )
                                ]
                            ),
                            new Models.Translations.Output.Context(
                                id: 2,
                                Meanings: [
                                    new Models.Translations.Output.Meaning(
                                        id: 1,
                                        MeaningTranslations: [
                                            new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "повозка"),
                                            new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "carriage"),
                                        ]
                                    )
                                ]
                            ),
                            new Models.Translations.Output.Context(
                                id: 3,
                                Meanings: [
                                    new Models.Translations.Output.Meaning(
                                        id: 1,
                                        MeaningTranslations: [
                                            new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "вагон"),
                                            new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "car"),
                                        ]
                                    )
                                ]
                            ),
                            new Models.Translations.Output.Context(
                                id: 4,
                                Meanings: [
                                    new Models.Translations.Output.Meaning(
                                        id: 1,
                                        MeaningTranslations: [
                                            new Models.Translations.Output.MeaningTranslation(Language: "ru", Text: "коляска"),
                                            new Models.Translations.Output.MeaningTranslation(Language: "en", Text: "stroller"),
                                        ]
                                    )
                                ]
                            )
                        ]
                    )
                ]
            );

            // Act
            var sut = _fixture.Create<TranslationsService>();
            WordModel result = sut.CreateWordModelFromTranslationOutput(translateHeadword: true, translateMeanings: true, originalWordModel, translationOutput);

            // Assert
            result.Definitions.Should().HaveCount(1);

            Definition definition = result.Definitions.First();

            /***********************************************************************/
            // Afeitar
            /***********************************************************************/
            definition.PartOfSpeech.Should().Be("masculine noun");
            definition.Endings.Should().BeEmpty();

            // Check headword
            definition.Headword.Original.Should().Be("el coche");
            definition.Headword.Russian.Should().Be("автомобиль");
            definition.Headword.English.Should().Be("car");

            // Check contexts
            definition.Contexts.Should().HaveCount(4);

            /***********************************************************************/
            // Context 1
            /***********************************************************************/
            Context context = definition.Contexts.First();
            context.Position.Should().Be("1");
            context.ContextEN.Should().Be("(vehicle)");
            context.Meanings.Should().HaveCount(1);
            Meaning meaning = context.Meanings.First();
            meaning.Original.Should().Be("car");
            meaning.Translation.Should().Be("автомобиль");
            meaning.AlphabeticalPosition.Should().Be("a.");
            meaning.Examples.Should().HaveCount(1);
            meaning.Examples.First().Original.Should().Be("Mi coche no prende porque tiene una falla en el motor.");
            meaning.Examples.First().Translation.Should().BeNull();

            /***********************************************************************/
            // Context 2
            /***********************************************************************/
            context = definition.Contexts.Skip(1).First();
            context.Position.Should().Be("2");
            context.ContextEN.Should().Be("(vehicle led by horses)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("carriage");
            meaning.Translation.Should().Be("повозка");
            meaning.AlphabeticalPosition.Should().Be("a.");
            meaning.Examples.Should().HaveCount(1);
            meaning.Examples.First().Original.Should().Be("Los monarcas llegaron en un coche elegante.");
            meaning.Examples.First().Translation.Should().BeNull();

            /***********************************************************************/
            // Context 3
            /***********************************************************************/
            context = definition.Contexts.Skip(2).First();
            context.Position.Should().Be("3");
            context.ContextEN.Should().Be("(train car)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("car");
            meaning.Translation.Should().Be("вагон");
            meaning.AlphabeticalPosition.Should().Be("a.");
            meaning.Examples.Should().HaveCount(1);
            meaning.Examples.First().Original.Should().Be("Tu mamá y yo vamos a pasar al coche comedor para almorzar.");
            meaning.Examples.First().Translation.Should().BeNull();

            /***********************************************************************/
            // Context 4
            /***********************************************************************/
            context = definition.Contexts.Skip(3).First();
            context.Position.Should().Be("4");
            context.ContextEN.Should().Be("(for babies)");
            context.Meanings.Should().HaveCount(1);
            meaning = context.Meanings.First();
            meaning.Original.Should().Be("stroller");
            meaning.Translation.Should().Be("коляска");
            meaning.AlphabeticalPosition.Should().Be("a.");
            meaning.Examples.Should().HaveCount(1);
            meaning.Examples.First().Original.Should().Be("La niñita no se quería subir al coche. Quería ir caminando.");
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

        private WordModel CreateWordModelForCoche()
        {
            WordModel wordModel = new WordModel(
                Word: "el coche",
                SoundUrl: _fixture.Create<Uri>().ToString(),
                SoundFileName: _fixture.Create<string>(),
                Definitions: new[]
                {
                    new Definition(
                        Headword: new Headword(Original: "el coche", English: null, Russian: null),
                        PartOfSpeech: "masculine noun",
                        Endings: "",
                        Contexts: [
                            new Context(
                                ContextEN: "(vehicle)",
                                Position: "1",
                                Meanings: [
                                    new Meaning(
                                        Original: "car",
                                        Translation: null,
                                        AlphabeticalPosition: "a.",
                                        Tag: null,
                                        ImageUrl: null,
                                        Examples: [ new Example(Original: "Mi coche no prende porque tiene una falla en el motor.", Translation: null) ]
                                    )
                                ]),
                            new Context(
                                ContextEN: "(vehicle led by horses)",
                                Position: "2",
                                Meanings: [
                                    new Meaning(
                                        Original: "carriage",
                                        Translation: null,
                                        AlphabeticalPosition: "a.",
                                        Tag: null,
                                        ImageUrl: null,
                                        Examples: [ new Example(Original: "Los monarcas llegaron en un coche elegante.", Translation: null) ]
                                    )
                                ]),
                            new Context(
                                ContextEN: "(train car)",
                                Position: "3",
                                Meanings: [
                                    new Meaning(
                                        Original: "car",
                                        Translation: null,
                                        AlphabeticalPosition: "a.",
                                        Tag: null,
                                        ImageUrl: null,
                                        Examples: [ new Example(Original: "Tu mamá y yo vamos a pasar al coche comedor para almorzar.", Translation: null) ]
                                    )
                                ]),
                            new Context(
                                ContextEN: "(for babies)",
                                Position: "4",
                                Meanings: [
                                    new Meaning(
                                        Original: "stroller",
                                        Translation: null,
                                        AlphabeticalPosition: "a.",
                                        Tag: null,
                                        ImageUrl: null,
                                        Examples: [ new Example(Original: "La niñita no se quería subir al coche. Quería ir caminando.", Translation: null) ]
                                    )
                                ])
                        ]
                    )
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
                        Contexts: [
                            new Models.Translations.Output.Context(
                                id: 1,
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
                    )
                ]
            );

            return translationOutput;
        }

        #endregion
    }
}
