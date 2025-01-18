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
        private Mock<ISettingsService> _settingsServiceMock = default!;

        [TestInitialize]
        public void Initialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            _clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            _dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            _settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
        }

        #region Tests for CompileFrontAsync

        #region Danish

        [TestMethod]
        public async Task CompileFrontAsync_ForSubstantivIntetkøn_AddsArticle()
        {
            var definitionVMs = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("et grillspyd");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForuSubstantivFælleskøn_AddsArticle()
        {
            var definitionVMs = CreateVMForUnderholdning();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("en underholdning");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForVerbum_AddsAt()
        {
            var definitionVMs = CreateVMForKigge();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("at kigge");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForAdjektiv_CopiesFront()
        {
            var definitionVMs = CreateVMForHøj();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("høj");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForAdverbium_CopiesFront()
        {
            var definitionVMs = CreateVMForLigeud();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("ligeud");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForForkortelse_CopiesFrontl()
        {
            var definitionVMs = CreateVMForIForbindleseMed();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("i forb. med");
        }

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
        public async Task CompileFrontAsync_ForBien_CopiesFront()
        {
            var definitionVMs = CreateVMForBien();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("bien");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForGuayWhenAdjectiveSelected_CopiesFront()
        {
            var definitionVMs = CreateVMForGuay();
            definitionVMs[1].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[1].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[1].IsChecked = true;
            definitionVMs[1].ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[1]);

            front.Should().Be("guay");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForLuce_CopiesFront()
        {
            var definitionVMs = CreateVMForLuce();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[2].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(definitionVMs[0]);

            front.Should().Be("luce");
        }

        #endregion

        #endregion

        #region Tests for CompileBackAsync

        #region Danish

        [TestMethod]
        public async Task CompileBackAsync_WhenRussianTranslationIsSelected_AddsTranslationToResult()
        {
            var definitionVMs = CreateVMForGrillspyd();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].HeadwordViewModel.IsRussianTranslationChecked = true;
            definitionVMs[0].HeadwordViewModel.IsEnglishTranslationChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">шашлыки</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenOnlyRussianTranslationIsSelected_RemovedTralingBrTag()
        {
            var definitionVMs = CreateVMForGrillspyd();
            //definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].HeadwordViewModel.IsRussianTranslationChecked = true;
            definitionVMs[0].HeadwordViewModel.IsEnglishTranslationChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">шашлыки</span>");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenEnglishTranslationIsSelected_AddsTranslationToResult()
        {
            var definitionVMs = CreateVMForGrillspyd();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].HeadwordViewModel.IsRussianTranslationChecked = false;
            definitionVMs[0].HeadwordViewModel.IsEnglishTranslationChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">kebabs</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenBothTranslationsAreSelected_AddsTranslationsToResult()
        {
            var definitionVMs = CreateVMForGrillspyd();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].HeadwordViewModel.IsRussianTranslationChecked = true;
            definitionVMs[0].HeadwordViewModel.IsEnglishTranslationChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">шашлыки</span><br><span style=\"color: rgba(0, 0, 0, 0.4)\">kebabs</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenOneExampleIsSelected_DoesNotAddNumbers()
        {
            var definitionVMs = CreateVMForGrillspyd();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].HeadwordViewModel.IsRussianTranslationChecked = false;
            definitionVMs[0].HeadwordViewModel.IsEnglishTranslationChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenSeveralExamplesAreSelected_AddsNumbers()
        {
            var definitionVMs = CreateVMForHaj();
            foreach (var maningVM in definitionVMs[0].ContextViewModels[0].MeaningViewModels)
            {
                foreach (var exampleVM in maningVM.ExampleViewModels)
                {
                    exampleVM.IsChecked = true;
                }
            }

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(definitionVMs[0]);

            result.Should().Be(
                "1.&nbsp;stor, langstrakt bruskfisk<br>" +
                $"2.&nbsp;<span {StyleAttributeForTag}>SLANG</span>grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning<br>" +
                $"3.&nbsp;<span {StyleAttributeForTag}>SLANG</span>person der er særlig dygtig til et spil, håndværk el.lign.");
        }

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

        #region Tests for CompilePartOfSpeechAsync

        [TestMethod]
        public async Task CompilePartOfSpeechAsync_Should_CopyPartOfSpeech()
        {
            var definitionVMs = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompilePartOfSpeechAsync(definitionVMs[0]);

            result.Should().Be("substantiv, intetkøn");
        }

        #endregion

        #region Tests for CompileEndingsAsync

        [TestMethod]
        public async Task CompileEndingsAsync_Should_CopyEndings()
        {
            var definitionVMs = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileEndingsAsync(definitionVMs[0]);

            result.Should().Be("-det eller (uofficielt) -et, -, -dene");
        }

        #endregion

        #region Tests for CompileExamplesAsync

        #region Danish

        [TestMethod]
        public async Task CompileExamplesAsync_WhenOneExampleSelected_DoesNotAddNumbers()
        {
            var definitionVMs = CreateVMForGrillspyd();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileExamplesAsync(definitionVMs[0]);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 1)\">Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver</span>");
        }

        [TestMethod]
        public async Task CompileExamplesAsync_WhenTwoExamplesSelected_AddsNumbers()
        {
            var definitionVMs = CreateVMForGrillspyd();
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVMs[0].ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[1].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileExamplesAsync(definitionVMs[0]);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 1)\">1.&nbsp;Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 1)\">2.&nbsp;Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater</span>");
        }

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

        private ObservableCollection<DefinitionViewModel> CreateVMForGrillspyd()
        {
            var definition = new Definition(new Headword(Original: "grillspyd", English: "Kebabs", Russian: "Шашлыки"), PartOfSpeech: "substantiv, intetkøn", Endings: "-det eller (uofficielt) -et, -, -dene",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver", null, null),
                                    new Example("Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater", null, null)
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "grillspyd",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForUnderholdning()
        {
            var definition = new Definition(new Headword("underholdning", null, null), PartOfSpeech: "substantiv, fælleskøn", Endings: "-en",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("noget der morer, glæder eller adspreder nogen, fx optræden, et lettere og ikke særlig krævende åndsprodukt eller en fornøjelig beskæftigelse", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("8000 medarbejdere skal til fest med god mad og underholdning af bl.a. Hans Otto Bisgaard", null, null),
                                    new Example("vi havde jo ikke radio, TV eller video, så underholdningen bestod mest af kortspil i familien", null, null)
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "underholdning",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForKigge()
        {
            var definition = new Definition(new Headword("kigge", null, null), PartOfSpeech: "verbum", Endings: "-r, -de, -t",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("rette blikket i en bestemt retning", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Børnene kiggede spørgende på hinanden", null, null),
                                    new Example("kig lige en gang!", null, null),
                                    new Example("Han kiggede sig rundt, som om han ledte efter noget", null, null),
                                }),
                            new Meaning("undersøge nærmere; sætte sig ind i", "2", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("hun har kigget på de psykiske eftervirkninger hos voldtagne piger og kvinder", null, null)
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "kigge",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForHøj()
        {
            var definition = new Definition(new Headword("høj", null, null), PartOfSpeech: "adjektiv", Endings: "-t, -e || -ere, -est",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("med forholdsvis stor udstrækning i lodret retning", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Overalt var der rejst høje flagstænger med røde og hvide bannere", null, null),
                                    new Example("I de sidste skoleår var jeg den absolut højeste i klassen – med mine 1,90 meter", null, null),
                                }),
                            new Meaning("med en forholdsvis stor værdi på en eksisterende eller tænkt skala; af stor størrelse, omfang el.lign.", "2", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("I år ser det ud til at antallet af rapporterede salmonellatilfælde bliver 40 pct. højere end i fjor", null, null),
                                    new Example("medarbejdere med en negativ holdning til erhvervslivet er ikke i høj kurs", null, null),
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "høj",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForLigeud()
        {
            var definition = new Definition(new Headword("ligeud", null, null), PartOfSpeech: "adverbium", Endings: "",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("uden at dreje eller skifte kurs om bevægelse eller retning", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Vi skulle stå med samlede ben, med vægten lagt på hælene og se ligeud", null, null)
                                }),
                            new Meaning("uden omsvøb", "2", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("det er for at sige det ligeud: skidehamrende-irriterende", null, null)
                                }),
                            new Meaning("billet til kørsel uden omstigning med et offentligt transportmiddel", "3", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("To ligeud, sagde moderen da konduktøren kom, – pigerne kører på én billet", null, null)
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "ligeud",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForIForbindleseMed()
        {
            var definition = new Definition(new Headword("i forb. med", null, null), PartOfSpeech: "forkortelse", Endings: "",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("= i forbindelse med", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Stillingen som sognepræst i Hellebæk og Hornbæk pastorater skal besættes midlertidigt i forb. med et barselsvikariat", null, null),
                                    new Example("Afviklingsselskabet Finansiel Stabilitet forventer at skære yderligere stillinger væk i de næste par måneder ifm. en koncernomlægning", null, null),
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "i forb. med",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForHaj()
        {
            var definition = new Definition(new Headword("haj", null, null), PartOfSpeech: "substantiv, fælleskøn", Endings: "-en, -er, -erne",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning("stor, langstrakt bruskfisk", "1", Tag: null, ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham", null, null),
                                }),
                            new Meaning("grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning", "2", Tag: "SLANG", ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("-", null, null),
                                }),
                            new Meaning("person der er særlig dygtig til et spil, håndværk el.lign.", "3", Tag: "SLANG", ImageUrl: null,
                                new List<Example>()
                                {
                                    new Example("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb", null, null),
                                })
                        }),
                }
            );

            var definitionVM = new DefinitionViewModel(
                "haj",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        #endregion

        #region Spanish

        private ObservableCollection<DefinitionViewModel> CreateVMForCoche()
        {
            var definition = new Definition(new Headword("el coche", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(vehicle)", "1",
                        new List<Meaning>
                        {
                            new Meaning("car", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Mi coche no prende porque tiene una falla en el motor.", English: "My car won't start because of a problem with the engine.", Russian: "") }),
                            new Meaning("automobile", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Todos estos coches tienen bolsas de aire.", English: "All these automobiles have airbags.", Russian: "") }),
                        }),

                    new Context("(vehicle led by horses)", "2",
                        new List<Meaning>
                        {
                            new Meaning("carriage", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Los monarcas llegaron en un coche elegante.", English: "The monarchs arrived in an elegant carriage.", Russian: "") }),
                            new Meaning("coach", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.", English: "Horse-drawn coaches were used much more before the invention of the automobile.", Russian: "") }),
                        })
                }
            );

            var definitionVM = new DefinitionViewModel(
                "coche",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForCasa()
        {
            var definition = new Definition(new Headword("la casa", null, null), PartOfSpeech: "feminine noun", Endings: "",
                new List<Context>
                {
                    new Context("(dwelling)", "1",
                        new List<Meaning>
                        {
                            new Meaning("house", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Vivimos en una casa con un gran jardín.", English: "We live in a house with a big garden.", Russian: "") }),
                        }),
                    // ...
                }
            );

            var definitionVM = new DefinitionViewModel(
                "casa",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForBien()
        {
            var definition = new Definition(new Headword("bien", null, null), PartOfSpeech: "ADVERB", Endings: "",
                new List<Context>
                {
                    new Context("(in good health)", "1",
                        new List<Meaning>
                        {
                            new Meaning("well", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Últimamente no me he sentido bien.", English: "I haven't felt well lately.", Russian: "") }),
                        }),
                    new Context("(properly)", "2",
                        new List<Meaning>
                        {
                            new Meaning("well", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Si la carne molida no se cocina bien, las bacterias no mueren.", English: "If the ground meat is not cooked well, the bacteria don't die.", Russian: "") }),
                        }),
                }
                // ...
            );

            var definitionVM = new DefinitionViewModel(
                "bien",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

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
                    new Context("(colloquial) (used to express approval) (Spain)", "1",
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
            var definitionVM1 = new DefinitionViewModel(
                "guay",
                definition1,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            var definition2 = new Definition(new Headword("guay", null, null), PartOfSpeech: "ADJECTIVE", Endings: "",
                new List<Context>
                {
                    new Context("(colloquial) (extremely good) (Spain)", "1",
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
            var definitionVM2 = new DefinitionViewModel(
                "guay",
                definition2,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

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
                    new Context("(third person singular)", "1",
                        new List<Meaning>
                        {
                            new Meaning("he looks (masculine)", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Luce más fuerte. ¿Ha estado yendo al gimnasio?", English: "He looks stronger. Has he been going to the gym?", Russian: ""), }),
                            new Meaning("she looks (feminine)", "b", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Luce muy bien con el pelo corto.", English: "She looks great with short hair.", Russian: "") }),
                            new Meaning("it looks", "c", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "¿Llevaste tu uniforme a la tintorería? Luce impecable el día de hoy.", English: "Did you take your uniform to the cleaners? It looks immaculate today.", Russian: "") }),
                        }),
                    new Context("(formal) (second person singular)", "2",
                        new List<Meaning>
                        {
                            new Meaning("you look", "a", Tag: null, ImageUrl: null,
                                new List<Example>() { new Example(Original: "Luce muy elegante, Sra. Vargas. ¿Tiene planes para hoy?", English: "You look very elegant, Mrs. Vargas. Do you have plans for today?", Russian: ""), }),
                        })
                }
            );

            var definitionVM = new DefinitionViewModel(
                "luce",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForSaltamontes()
        {
            var definition = new Definition(new Headword("el saltamontes", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(animal)", "1",
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

            var definitionVM = new DefinitionViewModel(
                "saltamontes",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

            var definitionVMs = new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };

            return definitionVMs;
        }

        private ObservableCollection<DefinitionViewModel> CreateVMForVeneno()
        {
            var definition = new Definition(new Headword("el veneno", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(toxic substance)", "1",
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
                    new Context("(ill intent)", "2",
                        new List<Meaning>
                        {
                            new Meaning("venom", "a", Tag: null, ImageUrl: null,
                                new List<Example>() {
                                    new Example(Original: "Le espetó con tal veneno que ni se atrevió a responderle.", English: "She spat at him with such venom that he didn't even dare respond.", Russian: "")
                                })
                        }),
                }
            );
            var definitionVM = new DefinitionViewModel(
                "veneno",
                definition,
                _copySelectedToClipboardServiceMock.Object,
                _dialogServiceMock.Object,
                _clipboardServiceMock.Object,
                _settingsServiceMock.Object);

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
