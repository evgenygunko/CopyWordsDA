using System.Collections.ObjectModel;
using AutoFixture;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class CopySelectedToClipboardServiceTests
    {
        private Fixture _fixture = default!;

        private Mock<ICopySelectedToClipboardService> _copySelectedToClipboardServiceMock = default!;
        private Mock<IClipboardService> _clipboardServiceMock = default!;
        private Mock<IDialogService> _dialogServiceMock = default!;

        [TestInitialize]
        public void Initialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            _clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            _dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
        }

        #region Tests for CompileFrontAsync

        #region Danish

        /*[TestMethod]
        public async Task CompileFrontAsync_ForSubstantivIntetkøn_AddsArticle()
        {
            const string meaning = "grillspyd";
            const string partOfSpeech = "substantiv, intetkøn";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("et grillspyd");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForuSubstantivFælleskøn_AddsArticle()
        {
            const string meaning = "underholdning";
            const string partOfSpeech = "substantiv, fælleskøn";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("en underholdning");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForVerbum_AddsAt()
        {
            const string meaning = "kigge";
            const string partOfSpeech = "verbum";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("at kigge");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForAdjektiv_AddsLabel()
        {
            const string meaning = "høj";
            const string partOfSpeech = "adjektiv";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("høj <span style=\"color: rgba(0, 0, 0, 0.4)\">ADJEKTIV</span>");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForAdverbium_AddsLabel()
        {
            const string meaning = "ligeud";
            const string partOfSpeech = "adverbium";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("ligeud <span style=\"color: rgba(0, 0, 0, 0.4)\">ADVERBIUM</span>");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForForkortelse_AddsLabel()
        {
            const string meaning = "i forb. med";
            const string partOfSpeech = "forkortelse";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("i forb. med <span style=\"color: rgba(0, 0, 0, 0.4)\">FORKORTELSE</span>");
        } */

        #endregion

        #region Spanish

        [TestMethod]
        public async Task CompileFrontAsync_ForCocheWhenOneExampleSelected_ReturnsOneFrontMeaning()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("un coche");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForCocheWhenTwoExamplesSelected_ReturnsOneFrontMeaning()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[1].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("un coche");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForMasculineNoun_AddsUn()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("un coche");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForFeminineNoun_AddsUna()
        {
            var definitionVMs = CreateVMForCasa();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("una casa");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForBien_AddsAdverbToTheEnd()
        {
            var definitionVMs = CreateVMForBien();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("bien <span style=\"color: rgba(0, 0, 0, 0.4)\">ADVERB</span>");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForGuayWhenAdjectiveSelected_AddsAdjectiveToTheEnd()
        {
            var definitionVMs = CreateVMForGuay();
            definitionVMs[1].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[1].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[1].IsChecked = true;
            definitionVMs[1].ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[1]);

            front.Should().Be("guay <span style=\"color: rgba(0, 0, 0, 0.4)\">ADJECTIVE</span>");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForLuce_AddsPhraseToTheEnd()
        {
            var definitionVMs = CreateVMForLuce();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[2].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("luce <span style=\"color: rgba(0, 0, 0, 0.4)\">PHRASE</span>");
        }

        #endregion

        #endregion

        #region Tests for CompileBackAsync

        #region Danish

        /*[TestMethod]
        public async Task CompileBackAsync_WhenRussianTranslationIsSelected_AddsTranslationToResult()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;

            var headword = new Headword("Grillspyd", "kebabs", "шашлыки");
            var headwordVM = new HeadwordViewModel(headword);
            headwordVM.IsRussianTranslationChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, headwordVM);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">шашлыки</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenEnglishTranslationIsSelected_AddsTranslationToResult()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;

            var headword = new Headword("Grillspyd", "kebabs", "шашлыки");
            var headwordVM = new HeadwordViewModel(headword);
            headwordVM.IsEnglishTranslationChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, headwordVM);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">kebabs</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenBothTranslationsAreSelected_AddsTranslationsToResult()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;

            var headword = new Headword("Grillspyd", "kebabs", "шашлыки");
            var headwordVM = new HeadwordViewModel(headword);
            headwordVM.IsRussianTranslationChecked = true;
            headwordVM.IsEnglishTranslationChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, headwordVM);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">шашлыки</span><br><span style=\"color: rgba(0, 0, 0, 0.4)\">kebabs</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenOneExampleIsSelected_DoesNotAddNumbers()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;

            var headwordVM = _fixture.Create<HeadwordViewModel>();
            headwordVM.IsRussianTranslationChecked = false;
            headwordVM.IsEnglishTranslationChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, headwordVM);

            result.Should().Be("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenSeveralExamplesAreSelected_AddsNumbers()
        {
            var wordVariantVMs = CreateVMForHaj();
            wordVariantVMs[0].Examples[0].IsChecked = true;
            wordVariantVMs[1].Examples[0].IsChecked = true;
            wordVariantVMs[2].Examples[0].IsChecked = true;

            var headwordVM = _fixture.Create<HeadwordViewModel>();
            headwordVM.IsRussianTranslationChecked = false;
            headwordVM.IsEnglishTranslationChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, headwordVM);

            result.Should().Be(
                "1.&nbsp;stor, langstrakt bruskfisk<br>" +
                $"2.&nbsp;<span {StyleAttributeForTag}>SLANG</span>grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning<br>" +
                $"3.&nbsp;<span {StyleAttributeForTag}>SLANG</span>person der er særlig dygtig til et spil, håndværk el.lign.");
        } */

        #endregion

        #region Spanish

        [TestMethod]
        public async Task CompileBackAsync_ForCocheWhenOneExampleSelected_ReturnsOneBackMeaning()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("car (vehicle)");
        }

        [TestMethod]
        public async Task CompileBackAsync_ForCocheWhenTwoExamplesSelected_ReturnsOneBackMeaning()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[1].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be(
                "1.&nbsp;car (vehicle)<br>" +
                "2.&nbsp;coach (vehicle led by horses)");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenImageUrlIsPresentAndSelected_CallsSaveImageFileService()
        {
            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock.Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var definitionVMs = CreateVMForSaltamontes();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].IsImageChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ImageUrl.Should().NotBeNullOrEmpty();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("grasshopper (animal)<br><img src=\"saltamontes.jpg\">");
            saveImageFileServiceMock.Verify(x => x.SaveImageFileAsync(definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ImageUrl, "saltamontes"));
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenImageUrlIsPresentButNotSelected_DoesNotCallSaveImageFileService()
        {
            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock.Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var definitionVMs = CreateVMForSaltamontes();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].IsImageChecked = false;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ImageUrl.Should().NotBeNullOrEmpty();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("grasshopper (animal)");
            saveImageFileServiceMock.Verify(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenImageUrlIsNull_DoesNotCallSaveImageFileService()
        {
            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock.Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var definitionVMs = CreateVMForLuce();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("he looks (masculine) (third person singular)");
            saveImageFileServiceMock.Verify(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenMultipleImagesArePresentAndSelected_SavesImagesUnderDifferentNames()
        {
            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock.Setup(x => x.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var definitionVMs = CreateVMForVeneno();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].IsImageChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ImageUrl.Should().NotBeNullOrEmpty();

            definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].IsImageChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].ImageUrl.Should().NotBeNullOrEmpty();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("1.&nbsp;venom (of an animal) (toxic substance)<br><img src=\"veneno.jpg\"><br>2.&nbsp;poison<br><img src=\"veneno1.jpg\">");
            saveImageFileServiceMock.Verify(x => x.SaveImageFileAsync(definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ImageUrl, "veneno"));
            saveImageFileServiceMock.Verify(x => x.SaveImageFileAsync(definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].ImageUrl, "veneno1"));
        }

        #endregion

        #endregion

        #region Tests for CompileExamplesAsync

        #region Danish

        /*[TestMethod]
        public async Task CompileExamplesAsync_WhenOneExampleSelected_DoesNotAddNumbers()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileExamplesAsync(wordVariantVMs);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 1)\">Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver</span>");
        }

        [TestMethod]
        public async Task CompileExamplesAsync_WhenTwoExamplesSelected_AddsNumbers()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;
            wordVariantVMs[0].Examples[1].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileExamplesAsync(wordVariantVMs);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 1)\">1.&nbsp;Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 1)\">2.&nbsp;Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater</span>");
        } */

        #endregion

        #region Spanish

        [TestMethod]
        public async Task CompileExamplesAsync_ForCocheWhenOneExampleSelected_ReturnsExampleWithoutNumbering()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileExamplesAsync(definitionVMs[0]);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 1)\">Mi coche no prende porque tiene una falla en el motor.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">My car won't start because of a problem with the engine.</span>");
        }

        [TestMethod]
        public async Task CompileExamplesAsync_ForCocheWhenTwoExampleSelected_ReturnsExamplesWithoutNumbering()
        {
            var definitionVMs = CreateVMForCoche();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileExamplesAsync(definitionVMs[0]);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 1)\">1.&nbsp;Mi coche no prende porque tiene una falla en el motor.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">My car won't start because of a problem with the engine.</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 1)\">2.&nbsp;Todos estos coches tienen bolsas de aire.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">All these automobiles have airbags.</span>");
        }

        #endregion

        #endregion

        #region Private Methods

        #region Danish

        /*internal static ObservableCollection<DefinitionViewModel> CreateVMForGrillspyd()
        {
            const string meaning = "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning";

            var definitionVM = new DefinitionViewModel(
                new Meaning(Description: meaning, AlphabeticalPosition: "", Tag: null, ImageUrl: null, Examples: new List<Example>()
                {
                    new Example("Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver", null, null),
                    new Example("Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater", null, null)
                }));

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private static ObservableCollection<DefinitionViewModel> CreateVMForHaj()
        {
            var definitionVM1 = new DefinitionViewModel(
                new Meaning(Description: "stor, langstrakt bruskfisk", AlphabeticalPosition: "", Tag: null, ImageUrl: null, Examples: new List<Example>()
                {
                    new Example("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham", null, null)
                }));

            var definitionVM2 = new DefinitionViewModel(
                new Meaning(Description: "grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning", AlphabeticalPosition: "", Tag: "SLANG", ImageUrl: null, Examples: new List<Example>()
                {
                    new Example("-", null, null)
                }));

            var definitionVM3 = new DefinitionViewModel(
                new Meaning(Description: "person der er særlig dygtig til et spil, håndværk el.lign.", AlphabeticalPosition: "", Tag: "SLANG", ImageUrl: null, Examples: new List<Example>()
                {
                    new Example("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb", null, null)
                }));

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM1, definitionVM2, definitionVM3
            };
        }*/

        #endregion

        #region Spanish

        private ObservableCollection<DefinitionViewModel> CreateVMForCoche()
        {
            var definition = new Definition(new Headword("coche", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(vehicle)", 1,
                        new List<Meaning>
                        {
                            new Meaning("car", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Mi coche no prende porque tiene una falla en el motor.", English: "My car won't start because of a problem with the engine.", Russian: "") }),
                            new Meaning("automobile", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Todos estos coches tienen bolsas de aire.", English: "All these automobiles have airbags.", Russian: "") }),
                        }),

                    new Context("(vehicle led by horses)", 2,
                        new List<Meaning>
                        {
                            new Meaning("carriage", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Los monarcas llegaron en un coche elegante.", English: "The monarchs arrived in an elegant carriage.", Russian: "") }),
                            new Meaning("coach", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.", English: "Horse-drawn coaches were used much more before the invention of the automobile.", Russian: "") }),
                        })
                }
            );

            var definitionVM = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForCasa()
        {
            var definition = new Definition(new Headword("casa", null, null), PartOfSpeech: "feminine noun", Endings: "",
                new List<Context>
                {
                    new Context("(dwelling)", 1,
                        new List<Meaning>
                        {
                            new Meaning("house", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Vivimos en una casa con un gran jardín.", English: "We live in a house with a big garden.", Russian: "") }),
                        }),
                    // ...
                }
            );

            var definitionVM = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForAfeitar()
        {
            var definition1 = new Definition(new Headword("afeitar", null, null), PartOfSpeech: "TRANSITIVE VERB", Endings: "",
                new List<Context>
                {
                    new Context("(to remove hair)", 1,
                        new List<Meaning>
                        {
                            new Meaning("to shave", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Para el verano, papá decidió afeitar al perro.", English: "For the summer, dad decided to shave the dog.", Russian: "") }),
                        }),
                }
            );
            var definitionVM1 = new DefinitionViewModel(definition1, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definition2 = new Definition(new Headword("afeitarse", null, null), PartOfSpeech: "REFLEXIVE VERB", Endings: "",
                new List<Context>
                {
                    new Context("(to shave oneself)", 1,
                        new List<Meaning>
                        {
                            new Meaning("to shave", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "¿Con qué frecuencia te afeitas la barba?", English: "How often do you shave your beard?", Russian: "") }),
                        }),
                }
            );
            var definitionVM2 = new DefinitionViewModel(definition2, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM1,
                definitionVM2
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForBien()
        {
            var definition = new Definition(new Headword("bien", null, null), PartOfSpeech: "ADVERB", Endings: "",
                new List<Context>
                {
                    new Context("(in good health)", 1,
                        new List<Meaning>
                        {
                            new Meaning("well", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Últimamente no me he sentido bien.", English: "I haven't felt well lately.", Russian: "") }),
                        }),
                    new Context("(properly)", 2,
                        new List<Meaning>
                        {
                            new Meaning("well", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Si la carne molida no se cocina bien, las bacterias no mueren.", English: "If the ground meat is not cooked well, the bacteria don't die.", Russian: "") }),
                        }),
                }
                // ...
            );

            var definitionVM = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForGuay()
        {
            var definition1 = new Definition(new Headword("guay", null, null), PartOfSpeech: "INTERJECTION", Endings: "",
                new List<Context>
                {
                    new Context("(colloquial) (used to express approval) (Spain)", 1,
                        new List<Meaning>
                        {
                            new Meaning("cool (colloquial)", "a", Tag: null, ImageUrl: null,
                                new List<Example>() {
                                    new Example(Original: "¿Quieres que veamos la peli en mi ordenador? - ¡Guay, tío!", English: "Do you want to watch the movie on my computer? - Cool, man!", Russian: ""),
                                    new Example(Original: "¡Gané un viaje a Francia! - ¡Guay!", English: "I won a trip to France! - Cool!", Russian: "")
                                }),
                            new Meaning("great (colloquial)", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Puedes tomarte el día libre mañana. - ¡Guay!", English: "You can take the day off tomorrow. - Great!", Russian: "") }),
                        }),
                    // ...
                }
            );
            var definitionVM1 = new DefinitionViewModel(definition1, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definition2 = new Definition(new Headword("guay", null, null), PartOfSpeech: "ADJECTIVE", Endings: "",
                new List<Context>
                {
                    new Context("(colloquial) (extremely good) (Spain)", 1,
                        new List<Meaning>
                        {
                            new Meaning("cool (colloquial)", "a", Tag: null, ImageUrl: null,
                                new List<Example>() {
                                    new Example(Original: "La fiesta de anoche estuvo muy guay.", English: "Last night's party was really cool.", Russian: ""),
                                    new Example(Original: "Tus amigos son guays, Roberto. ¿Dónde los conociste?", English: "Your friends are cool, Roberto. Where did you meet them?", Russian: "")
                                }),
                            new Meaning("super (colloquial)", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "¡Que monopatín tan guay!", English: "That's a super skateboard!", Russian: "") }),
                        }),
                    // ...
                }
            );
            var definitionVM2 = new DefinitionViewModel(definition2, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM1,
                definitionVM2
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForLuce()
        {
            var definition = new Definition(new Headword("luce", null, null), PartOfSpeech: "PHRASE", Endings: "",
                new List<Context>
                {
                    new Context("(third person singular)", 1,
                        new List<Meaning>
                        {
                            new Meaning("he looks (masculine)", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Luce más fuerte. ¿Ha estado yendo al gimnasio?", English: "He looks stronger. Has he been going to the gym?", Russian: ""), }),
                            new Meaning("she looks (feminine)", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Luce muy bien con el pelo corto.", English: "She looks great with short hair.", Russian: "") }),
                            new Meaning("it looks", "c", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "¿Llevaste tu uniforme a la tintorería? Luce impecable el día de hoy.", English: "Did you take your uniform to the cleaners? It looks immaculate today.", Russian: "") }),
                        }),
                    new Context("(formal) (second person singular)", 2,
                        new List<Meaning>
                        {
                            new Meaning("you look", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Luce muy elegante, Sra. Vargas. ¿Tiene planes para hoy?", English: "You look very elegant, Mrs. Vargas. Do you have plans for today?", Russian: ""), }),
                        })
                }
            );

            var definitionVM = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForSaltamontes()
        {
            var definition = new Definition(new Headword("saltamontes", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(animal)", 1,
                        new List<Meaning>
                        {
                            new Meaning("grasshopper", "a", Tag: null, ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/5bf100e5-da54-4be6-a55c-281edcd08b10.jpg",
                                new List<Example>() {
                                    new Example(Original: "Los saltamontes pueden saltar muy alto.", English: "Grasshoppers can jump really high.", Russian: "")
                                })
                        }),
                    // ...
                }
            );

            var definitionVM = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForVeneno()
        {
            var definition = new Definition(new Headword("veneno", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(toxic substance)", 1,
                        new List<Meaning>
                        {
                            new Meaning("venom (of an animal)", "a", Tag: null, ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d533b470-18a4-4cae-ad08-3ee8858ae02c.jpg",
                                new List<Example>() {
                                    new Example(Original: "La herida aún tiene el veneno dentro.", English: "The wound still has venom in it.", Russian: "")
                                }),
                            new Meaning("poison", "b", Tag: null, ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d07aa7fd-a3fd-4d06-9751-656180d8b1ee.jpg",
                                new List<Example>() {
                                    new Example(Original: "Estos hongos contienen un veneno mortal.", English: "These mushrooms contain a deadly poison.", Russian: "")
                                })
                        }),
                    new Context("(ill intent)", 2,
                        new List<Meaning>
                        {
                            new Meaning("venom", "a", Tag: null, ImageUrl: null,
                                new List<Example>() {
                                    new Example(Original: "Le espetó con tal veneno que ni se atrevió a responderle.", English: "She spat at him with such venom that he didn't even dare respond.", Russian: "")
                                })
                        }),
                }
            );
            var definitionVM = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        #endregion

        private static string StyleAttributeForTag => "style=\"color:#404040; background-color:#eaeff2; border:1px solid #CCCCCC; margin-right:10px; font-size: 80%;\"";

        #endregion
    }
}
