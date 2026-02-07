// Ignore Spelling: Verbum Coche Substantiv Intetkøn Foru Forkortelse Guay Fælleskøn Frontl Bien Adverbium Adjektiv Online

using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class CopySelectedToClipboardServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for CompileFrontAsync

        #region Danish

        [TestMethod]
        public void CompileFrontAsync_WhenNoExamplesSelected_UsesFirstDefinition()
        {
            var definitionVM = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();
            string front = sut.CompileFront(definitionVM);

            front.Should().Be("et grillspyd");
        }

        [TestMethod]
        public void CompileFrontAsync_ForSubstantivIntetkøn_AddsArticle()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("et grillspyd");
        }

        [TestMethod]
        public void CompileFrontAsync_ForuSubstantivFælleskøn_AddsArticle()
        {
            var definitionVM = CreateVMForUnderholdning();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("en underholdning");
        }

        [TestMethod]
        public void CompileFrontAsync_ForVerbum_AddsAt()
        {
            var definitionVM = CreateVMForKigge();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("at kigge");
        }

        [TestMethod]
        public void CompileFrontAsync_ForAdjektiv_CopiesFront()
        {
            var definitionVM = CreateVMForHøj();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("høj");
        }

        [TestMethod]
        public void CompileFrontAsync_ForAdverbium_CopiesFront()
        {
            var definitionVM = CreateVMForLigeud();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("ligeud");
        }

        [TestMethod]
        public void CompileFrontAsync_ForForkortelse_CopiesFrontl()
        {
            var definitionVM = CreateVMForIForbindleseMed();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("i forb. med");
        }

        #endregion

        #region Spanish

        [TestMethod]
        public void CompileFrontAsync_ForCocheWhenOneExampleSelected_ReturnsOneFrontMeaning()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("un coche");
        }

        [TestMethod]
        public void CompileFrontAsync_ForCocheWhenTwoExamplesSelected_ReturnsOneFrontMeaning()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[1].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("un coche");
        }

        [TestMethod]
        public void CompileFrontAsync_ForMasculineNoun_AddsUn()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("un coche");
        }

        [TestMethod]
        public void CompileFrontAsync_ForFeminineNoun_AddsUna()
        {
            var definitionVM = CreateVMForCasa();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("una casa");
        }

        [TestMethod]
        public void CompileFrontAsync_ForFeminineNounStartingWithSTressedA_AddsEl()
        {
            var definitionVM = CreateVMForHampa();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("el hampa");
        }

        [TestMethod]
        public void CompileFrontAsync_ForMasculineAndFeminineNouns_ReplacesElAndLaWithUnAndUna()
        {
            var definitionVM = CreateVMForMorador();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("un morador, una moradora");
        }

        [TestMethod]
        public void CompileFrontAsync_ForBien_CopiesFront()
        {
            var definitionVM = CreateVMForBien();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("bien");
        }

        [TestMethod]
        public void CompileFrontAsync_ForLuce_CopiesFront()
        {
            var definitionVM = CreateVMForLuce();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[2].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = sut.CompileFront(definitionVM);

            front.Should().Be("luce");
        }

        #endregion

        #endregion

        #region Tests for CompileBack

        #region Danish

        [TestMethod]
        public void CompileBack_WhenNoExamplesSelected_ReturnsEmptyString()
        {
            var definitionVM = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();
            string result = sut.CompileBack(definitionVM);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void CompileBackWhenTranslationInMeaningIsNotEmpty_AddsHRTagAsADelimiterBetweenMeanings()
        {
            var definitionVM = CreateVMForHaj();
            foreach (var maningVM in definitionVM.ContextViewModels[0].MeaningViewModels)
            {
                foreach (var exampleVM in maningVM.ExampleViewModels)
                {
                    exampleVM.IsChecked = true;
                }
            }

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = true;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "1.&nbsp;stor, langstrakt bruskfisk<br><span style=\"color: rgba(0, 0, 0, 0.4)\">крупная, удлиненная хрящевая рыба</span><br>" +
                $"2.&nbsp;<span {StyleAttributeForTag}>SLANG</span>grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning<br><span style=\"color: rgba(0, 0, 0, 0.4)\">жадный, беспринципный человек, который незаконными или нечестными методами получает финансовую выгоду за счет других</span><br>" +
                $"3.&nbsp;<span {StyleAttributeForTag}>SLANG</span>person der er særlig dygtig til et spil, håndværk el.lign.<br><span style=\"color: rgba(0, 0, 0, 0.4)\">человек, который особенно умел в игре, ремесле и т. д.</span>");
        }

        [TestMethod]
        public void CompileBack_WhenTranslationInMeaningIsEmpty_OnlyAddsOriginalMeaningToResult()
        {
            var definitionVM = CreateVMForLigeud();
            definitionVM.ContextViewModels[0].MeaningViewModels[2].ExampleViewModels[0].IsChecked = true;
            definitionVM.HeadwordViewModel.IsDestinationTranslationChecked = false;
            definitionVM.HeadwordViewModel.IsEnglishTranslationChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be("billet til kørsel uden omstigning med et offentligt transportmiddel");
        }

        [TestMethod]
        public void CompileBack_WhenDestinationTranslationIsSelected_AddsTranslationToResult()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.HeadwordViewModel.IsDestinationTranslationChecked = true;
            definitionVM.HeadwordViewModel.IsEnglishTranslationChecked = false;

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = true;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">шампур</span><br>" +
                "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning<br>" +
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">заостренная палочка из дерева или металла для прокалывания мяса и овощей во время жарки на гриле</span>");
        }

        [TestMethod]
        public void CompileBack_WhenEnglishTranslationIsSelected_AddsTranslationToResult()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.HeadwordViewModel.IsDestinationTranslationChecked = false;
            definitionVM.HeadwordViewModel.IsEnglishTranslationChecked = true;

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = true;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">kebab skewer</span><br>" +
                "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning<br>" +
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">заостренная палочка из дерева или металла для прокалывания мяса и овощей во время жарки на гриле</span>");
        }

        [TestMethod]
        public void CompileBack_WhenBothTranslationsAreSelected_AddsTranslationsToResult()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.HeadwordViewModel.IsDestinationTranslationChecked = true;
            definitionVM.HeadwordViewModel.IsEnglishTranslationChecked = true;

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = true;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">шампур</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">kebab skewer</span><br>" +
                "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning<br>" +
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">заостренная палочка из дерева или металла для прокалывания мяса и овощей во время жарки на гриле</span>");
        }

        [TestMethod]
        public void CompileBack_WhenCopyTranslatedMeaningsIsFalse_DoesNotCopyTranslationForMeaningsToResult()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.HeadwordViewModel.IsDestinationTranslationChecked = true;
            definitionVM.HeadwordViewModel.IsEnglishTranslationChecked = true;

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = false;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">шампур</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">kebab skewer</span><br>" +
                "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public void CompileBack_WhenOneExampleIsSelected_DoesNotAddNumbers()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.HeadwordViewModel.IsDestinationTranslationChecked = false;
            definitionVM.HeadwordViewModel.IsEnglishTranslationChecked = false;

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = true;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning<br>" +
                "<span style=\"color: rgba(0, 0, 0, 0.4)\">заостренная палочка из дерева или металла для прокалывания мяса и овощей во время жарки на гриле</span>");
        }

        [TestMethod]
        public void CompileBack_WhenSeveralExamplesAreSelected_AddsNumbers()
        {
            var definitionVM = CreateVMForHaj();
            foreach (var maningVM in definitionVM.ContextViewModels[0].MeaningViewModels)
            {
                foreach (var exampleVM in maningVM.ExampleViewModels)
                {
                    exampleVM.IsChecked = true;
                }
            }

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.CopyTranslatedMeanings = true;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "1.&nbsp;stor, langstrakt bruskfisk<br><span style=\"color: rgba(0, 0, 0, 0.4)\">крупная, удлиненная хрящевая рыба</span><br>" +
                $"2.&nbsp;<span {StyleAttributeForTag}>SLANG</span>grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning<br><span style=\"color: rgba(0, 0, 0, 0.4)\">жадный, беспринципный человек, который незаконными или нечестными методами получает финансовую выгоду за счет других</span><br>" +
                $"3.&nbsp;<span {StyleAttributeForTag}>SLANG</span>person der er særlig dygtig til et spil, håndværk el.lign.<br><span style=\"color: rgba(0, 0, 0, 0.4)\">человек, который особенно умел в игре, ремесле и т. д.</span>");
        }

        [TestMethod]
        public void CompileBack_Should_InsertTagsForImages()
        {
            string? imageUrl = null;

            var definitionVM = CreateVMForCasa();
            foreach (var maningVM in definitionVM.ContextViewModels[0].MeaningViewModels)
            {
                maningVM.IsImageChecked = true;
                imageUrl = maningVM.ImageUrl;

                foreach (var exampleVM in maningVM.ExampleViewModels)
                {
                    exampleVM.IsChecked = true;
                }
            }

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be("house (dwelling)<br><img src=\"casa.jpg\">");
        }

        #endregion

        #region Spanish

        [TestMethod]
        public void CompileBack_ForCocheWhenOneExampleSelected_ReturnsOneBackMeaning()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be("car (vehicle)");
        }

        [TestMethod]
        public void CompileBack_ForCocheWhenTwoExamplesSelected_ReturnsOneBackMeaning()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[1].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileBack(definitionVM);

            result.Should().Be(
                "1.&nbsp;car (vehicle)<br>" +
                "2.&nbsp;coach (vehicle led by horses)");
        }

        #endregion

        #endregion

        #region Tests for CompilePartOfSpeech

        [TestMethod]
        public void CompilePartOfSpeech_Should_CopyPartOfSpeech()
        {
            var definitionVM = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompilePartOfSpeech(definitionVM);

            result.Should().Be("substantiv, intetkøn");
        }

        #endregion

        #region Tests for CompileEndings

        [TestMethod]
        public void CompileEndings_Should_CopyEndings()
        {
            var definitionVM = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileEndings(definitionVM);

            result.Should().Be("-det eller (uofficielt) -et, -, -dene");
        }

        #endregion

        #region Tests for CompileExamples

        #region Danish

        [TestMethod]
        public void CompileExamples_WhenNoExamplesSelected_ReturnsEmptyString()
        {
            var definitionVM = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();
            string result = sut.CompileExamples(definitionVM);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void CompileExamples_WhenOneExampleSelected_DoesNotAddNumbers()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileExamples(definitionVM);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 1)\">Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver</span>");
        }

        [TestMethod]
        public void CompileExamples_WhenTwoExamplesSelected_AddsNumbers()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[1].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileExamples(definitionVM);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 1)\">1.&nbsp;Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 1)\">2.&nbsp;Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater</span>");
        }

        #endregion

        #region Spanish

        [TestMethod]
        public void CompileExamples_ForCocheWhenOneExampleSelected_ReturnsExampleWithoutNumbering()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileExamples(definitionVM);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 1)\">Mi coche no prende porque tiene una falla en el motor.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">My car won't start because of a problem with the engine.</span>");
        }

        [TestMethod]
        public void CompileExamples_ForCocheWhenTwoExampleSelected_ReturnsExamplesWithoutNumbering()
        {
            var definitionVM = CreateVMForCoche();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[1].ExampleViewModels[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileExamples(definitionVM);

            result.Should().Be(
                "<span style=\"color: rgba(0, 0, 0, 1)\">1.&nbsp;Mi coche no prende porque tiene una falla en el motor.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">My car won't start because of a problem with the engine.</span><br>" +
                "<span style=\"color: rgba(0, 0, 0, 1)\">2.&nbsp;Todos estos coches tienen bolsas de aire.</span>&nbsp;<span style=\"color: rgba(0, 0, 0, 0.4)\">All these automobiles have airbags.</span>");
        }

        #endregion

        #endregion

        #region Tests for CompileTranslations

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void CompileTranslations_WhenHeadwordOriginalIsNullOrEmpty_ReturnsEmptyString(string headwordOriginal)
        {
            var definition = new Definition(new Headword(headwordOriginal, null, null), string.Empty, string.Empty, []);
            var definitionVM = new DefinitionViewModel(definition, SourceLanguage.Danish, true);

            var sut = _fixture.Create<CopySelectedToClipboardService>();
            string result = sut.CompileTranslations(definitionVM);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void CompileTranslations_Should_ReturnTranslations()
        {
            var definitionVM = CreateVMForGrillspyd();

            var sut = _fixture.Create<CopySelectedToClipboardService>();
            string result = sut.CompileTranslations(definitionVM);

            result.Should().Be("grillspyd (substantiv, intetkøn)" + Environment.NewLine + "шампур" + Environment.NewLine + "kebab skewer");
        }

        #endregion

        #region Tests for CompileSoundFileName

        [TestMethod]
        public void CompileSoundFileName_Should_FormatSoundFileNameWithMp3Extension()
        {
            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = sut.CompileSoundFileName("ejemplo");

            result.Should().Be("[sound:ejemplo.mp3]");
        }

        #endregion

        #region Tests for CompileImages

        [TestMethod]
        public void CompileImages_WhenNoExamplesSelected_ReturnsEmptyCollection()
        {
            var definitionVM = CreateVMForCasa();

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            var result = sut.CompileImages(definitionVM);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void CompileImages_WhenExampleSelectedButImageNotChecked_ReturnsEmptyCollection()
        {
            var definitionVM = CreateVMForCasa();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[0].IsImageChecked = false;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            var result = sut.CompileImages(definitionVM);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void CompileImages_WhenExampleSelectedAndImageChecked_ReturnsImageFile()
        {
            var definitionVM = CreateVMForCasa();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[0].IsImageChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            var result = sut.CompileImages(definitionVM).ToList();

            result.Should().HaveCount(1);
            result[0].FileName.Should().Be("casa.jpg");
            result[0].ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/1ccc644c-898c-49b1-be9b-01eee0375a72.jpg");
        }

        [TestMethod]
        public void CompileImages_WhenMultipleImagesSelected_ReturnsImageFilesWithIndexedFileNames()
        {
            var definitionVM = CreateVMForVeneno();
            foreach (var contextVM in definitionVM.ContextViewModels)
            {
                foreach (var meaningVM in contextVM.MeaningViewModels)
                {
                    if (!string.IsNullOrEmpty(meaningVM.ImageUrl))
                    {
                        meaningVM.IsImageChecked = true;
                        meaningVM.ExampleViewModels[0].IsChecked = true;
                    }
                }
            }

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            var result = sut.CompileImages(definitionVM).ToList();

            result.Should().HaveCount(2);
            result[0].FileName.Should().Be("veneno.jpg");
            result[0].ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d533b470-18a4-4cae-ad08-3ee8858ae02c.jpg");
            result[1].FileName.Should().Be("veneno1.jpg");
            result[1].ImageUrl.Should().Be("https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d07aa7fd-a3fd-4d06-9751-656180d8b1ee.jpg");
        }

        [TestMethod]
        public void CompileImages_WhenImageUrlIsEmpty_ReturnsEmptyCollection()
        {
            var definitionVM = CreateVMForGrillspyd();
            definitionVM.ContextViewModels[0].MeaningViewModels[0].ExampleViewModels[0].IsChecked = true;
            definitionVM.ContextViewModels[0].MeaningViewModels[0].IsImageChecked = true;
            // ImageUrl is null for grillspyd

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            var result = sut.CompileImages(definitionVM);

            result.Should().BeEmpty();
        }

        #endregion

        #region Tests for GetImageFileNameWithoutExtension

        [TestMethod]
        public void GetImageFileNameWithoutExtension_Should_ReplaceElAtStart()
        {
            CopySelectedToClipboardService.GetImageFileNameWithoutExtension("el alfiler").Should().Be("alfiler");
        }

        [TestMethod]
        public void GetImageFileNameWithoutExtension_Should_ReplaceLaAtStart()
        {
            CopySelectedToClipboardService.GetImageFileNameWithoutExtension("la casa").Should().Be("casa");
        }

        [TestMethod]
        public void GetImageFileNameWithoutExtension_WhenMasculineAndFeminineMeanings_ReturnsOnlyMasculine()
        {
            CopySelectedToClipboardService.GetImageFileNameWithoutExtension("el camionero, la camionera").Should().Be("camionero");
        }

        #endregion

        #region Private Methods

        #region Danish

        private DefinitionViewModel CreateVMForGrillspyd()
        {
            var definition = new Definition(new Headword(Original: "grillspyd", English: "kebab skewer", Translation: "шампур"), PartOfSpeech: "substantiv, intetkøn", Endings: "-det eller (uofficielt) -et, -, -dene",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning",
                                Translation: "заостренная палочка из дерева или металла для прокалывания мяса и овощей во время жарки на гриле",
                                AlphabeticalPosition: "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver", Translation: null),
                                    new Example(Original : "Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater", Translation: null)
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        private DefinitionViewModel CreateVMForUnderholdning()
        {
            var definition = new Definition(new Headword("underholdning", null, null), PartOfSpeech: "substantiv, fælleskøn", Endings: "-en",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning(Original: "noget der morer, glæder eller adspreder nogen, fx optræden, et lettere og ikke særlig krævende åndsprodukt eller en fornøjelig beskæftigelse",
                                Translation: "что-то, что развлекает, радует или отвлекает кого-то, например, представление, более легкий и не особенно требовательный интеллектуальный продукт или приятное занятие",
                                AlphabeticalPosition: "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "8000 medarbejdere skal til fest med god mad og underholdning af bl.a. Hans Otto Bisgaard", Translation: null),
                                    new Example(Original : "vi havde jo ikke radio, TV eller video, så underholdningen bestod mest af kortspil i familien", Translation: null)
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        private DefinitionViewModel CreateVMForKigge()
        {
            var definition = new Definition(new Headword("kigge", null, null), PartOfSpeech: "verbum", Endings: "-r, -de, -t",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning
                                (Original : "rette blikket i en bestemt retning",
                                Translation : "направить взгляд в определенном направлении",
                                AlphabeticalPosition: "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Børnene kiggede spørgende på hinanden", Translation: null),
                                    new Example(Original : "kig lige en gang!", Translation: null),
                                    new Example(Original : "Han kiggede sig rundt, som om han ledte efter noget", Translation: null),
                                }),
                            new Meaning(
                                Original: "undersøge nærmere; sætte sig ind i",
                                Translation: "присмотритесь повнимательнее; попасть в",
                                AlphabeticalPosition: "2",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "hun har kigget på de psykiske eftervirkninger hos voldtagne piger og kvinder", Translation: null)
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        private DefinitionViewModel CreateVMForHøj()
        {
            var definition = new Definition(new Headword("høj", null, null), PartOfSpeech: "adjektiv", Endings: "-t, -e || -ere, -est",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original : "med forholdsvis stor udstrækning i lodret retning",
                                Translation : "с относительно большой протяженностью в вертикальном направлении",
                                AlphabeticalPosition : "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Overalt var der rejst høje flagstænger med røde og hvide bannere", Translation: null),
                                    new Example(Original : "I de sidste skoleår var jeg den absolut højeste i klassen – med mine 1,90 meter", Translation: null),
                                }),
                            new Meaning(
                                Original : "med en forholdsvis stor værdi på en eksisterende eller tænkt skala; af stor størrelse, omfang el.lign.",
                                Translation : "с относительно большой ценностью в существующих или воображаемых масштабах; большого размера, масштаба и т. д.",
                                AlphabeticalPosition : "2",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "I år ser det ud til at antallet af rapporterede salmonellatilfælde bliver 40 pct. højere end i fjor", Translation: null),
                                    new Example(Original : "medarbejdere med en negativ holdning til erhvervslivet er ikke i høj kurs", Translation: null),
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        private DefinitionViewModel CreateVMForLigeud()
        {
            var definition = new Definition(new Headword("ligeud", null, null), PartOfSpeech: "adverbium", Endings: "",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original : "uden at dreje eller skifte kurs om bevægelse eller retning",
                                Translation : "не поворачиваясь и не меняя курс или направление движения",
                                AlphabeticalPosition : "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Vi skulle stå med samlede ben, med vægten lagt på hælene og se ligeud", Translation: null)
                                }),
                            new Meaning(
                                Original : "uden omsvøb",
                                Translation : "без каких-либо обременений",
                                AlphabeticalPosition : "2",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples : new List<Example>()
                                {
                                    new Example(Original : "det er for at sige det ligeud: skidehamrende-irriterende", Translation: null)
                                }),
                            new Meaning(
                                Original : "billet til kørsel uden omstigning med et offentligt transportmiddel",
                                Translation : null,
                                AlphabeticalPosition : "3",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples : new List<Example>()
                                {
                                    new Example(Original : "To ligeud, sagde moderen da konduktøren kom, – pigerne kører på én billet", Translation: null)
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        private DefinitionViewModel CreateVMForIForbindleseMed()
        {
            var definition = new Definition(new Headword("i forb. med", null, null), PartOfSpeech: "forkortelse", Endings: "",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "= i forbindelse med",
                                Translation: "= в связи с",
                                AlphabeticalPosition: "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Stillingen som sognepræst i Hellebæk og Hornbæk pastorater skal besættes midlertidigt i forb. med et barselsvikariat", Translation: null),
                                    new Example(Original : "Afviklingsselskabet Finansiel Stabilitet forventer at skære yderligere stillinger væk i de næste par måneder ifm. en koncernomlægning", Translation: null),
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        private DefinitionViewModel CreateVMForHaj()
        {
            var definition = new Definition(new Headword("haj", null, null), PartOfSpeech: "substantiv, fælleskøn", Endings: "-en, -er, -erne",
                new List<Context>
                {
                    new Context(ContextEN: "", Position: "",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "stor, langstrakt bruskfisk",
                                Translation: "крупная, удлиненная хрящевая рыба",
                                AlphabeticalPosition: "1",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham", Translation: null),
                                }),
                            new Meaning(
                                Original: "grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning",
                                Translation: "жадный, беспринципный человек, который незаконными или нечестными методами получает финансовую выгоду за счет других",
                                AlphabeticalPosition: "2",
                                Tag: "SLANG",
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "-", Translation: null),
                                }),
                            new Meaning(
                                Original: "person der er særlig dygtig til et spil, håndværk el.lign.",
                                Translation: "человек, который особенно умел в игре, ремесле и т. д.",
                                AlphabeticalPosition: "3",
                                Tag: "SLANG",
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                {
                                    new Example(Original : "Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb", Translation: null),
                                })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Danish,
                true);
        }

        #endregion

        #region Spanish

        private DefinitionViewModel CreateVMForCoche()
        {
            var definition = new Definition(new Headword("el coche", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(vehicle)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "car",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Mi coche no prende porque tiene una falla en el motor.", Translation: "My car won't start because of a problem with the engine.")
                                    }),
                            new Meaning(
                                Original: "automobile",
                                Translation: null,
                                AlphabeticalPosition: "b",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Todos estos coches tienen bolsas de aire.", Translation: "All these automobiles have airbags.")
                                    }),
                        }),

                    new Context("(vehicle led by horses)", "2",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "carriage",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Los monarcas llegaron en un coche elegante.", Translation: "The monarchs arrived in an elegant carriage.")
                                    }),
                            new Meaning(
                                Original: "coach",
                                Translation: null,
                                AlphabeticalPosition: "b",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.", Translation: "Horse-drawn coaches were used much more before the invention of the automobile.")
                                    }),
                        })
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForCasa()
        {
            var definition = new Definition(new Headword("la casa", null, null), PartOfSpeech: "feminine noun", Endings: "",
                new List<Context>
                {
                    new Context("(dwelling)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "house",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/1ccc644c-898c-49b1-be9b-01eee0375a72.jpg",
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Vivimos en una casa con un gran jardín.", Translation: "We live in a house with a big garden.")
                                    }),
                        }),
                    // ...
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForHampa()
        {
            var definition = new Definition(new Headword("el hampa", null, null), PartOfSpeech: "feminine noun", Endings: "",
                new List<Context>
                {
                    new Context("(mobsters)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "underworld",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Enrico se involucró con el hampa y hace tres días está desaparecido.", Translation: "Enrico got involved with the underworld and has been missing for three days.")
                                    }),
                        }),
                    // ...
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForMorador()
        {
            var definition = new Definition(new Headword("el morador, la moradora", null, null), PartOfSpeech: "masculine or feminine noun", Endings: "",
                new List<Context>
                {
                    new Context("(general)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "underworld",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Los moradores de esta aldea viven en casas hechas de adobe.", Translation: "The inhabitants of this village live in adobe houses.")
                                    }),
                        }),
                    // ...
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForBien()
        {
            var definition = new Definition(new Headword("bien", null, null), PartOfSpeech: "ADVERB", Endings: "",
                new List<Context>
                {
                    new Context("(in good health)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "well",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Últimamente no me he sentido bien.", Translation: "I haven't felt well lately.")
                                    }),
                        }),
                    new Context("(properly)", "2",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "well",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Si la carne molida no se cocina bien, las bacterias no mueren.", Translation: "If the ground meat is not cooked well, the bacteria don't die.")
                                    }),
                        }),
                }
                // ...
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForGuay()
        {
            var definition = new Definition(new Headword("guay", null, null), PartOfSpeech: "INTERJECTION", Endings: "",
                new List<Context>
                {
                    new Context("(colloquial) (used to express approval) (Spain)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "cool (colloquial)",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "¿Quieres que veamos la peli en mi ordenador? - ¡Guay, tío!", Translation: "Do you want to watch the movie on my computer? - Cool, man!"),
                                        new Example(Original: "¡Gané un viaje a Francia! - ¡Guay!", Translation: "I won a trip to France! - Cool!")
                                    }),
                            new Meaning(
                                Original: "great (colloquial)",
                                Translation: null,
                                AlphabeticalPosition: "b",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Puedes tomarte el día libre mañana. - ¡Guay!", Translation: "You can take the day off tomorrow. - Great!")
                                    }),
                        }),
                    // ...
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForLuce()
        {
            var definition = new Definition(new Headword("luce", null, null), PartOfSpeech: "PHRASE", Endings: "",
                new List<Context>
                {
                    new Context("(third person singular)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "he looks (masculine)",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Luce más fuerte. ¿Ha estado yendo al gimnasio?", Translation: "He looks stronger. Has he been going to the gym?"),
                                    }),
                            new Meaning(
                                Original: "she looks (feminine)",
                                Translation: "она выглядит",
                                AlphabeticalPosition: "b",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Luce muy bien con el pelo corto.", Translation: "She looks great with short hair.")
                                    }),
                            new Meaning(
                                Original: "it looks",
                                Translation: null,
                                AlphabeticalPosition: "c",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "¿Llevaste tu uniforme a la tintorería? Luce impecable el día de hoy.", Translation: "Did you take your uniform to the cleaners? It looks immaculate today.")
                                    }),
                        }),
                    new Context("(formal) (second person singular)", "2",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "you look",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Luce muy elegante, Sra. Vargas. ¿Tiene planes para hoy?", Translation: "You look very elegant, Mrs. Vargas. Do you have plans for today?"),
                                    }),
                        })
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForSaltamontes()
        {
            var definition = new Definition(new Headword("el saltamontes", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(animal)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "grasshopper",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/5bf100e5-da54-4be6-a55c-281edcd08b10.jpg",
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Los saltamontes pueden saltar muy alto.", Translation: "Grasshoppers can jump really high.")
                                    })
                        }),
                    // ...
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        private DefinitionViewModel CreateVMForVeneno()
        {
            var definition = new Definition(new Headword("el veneno", null, null), PartOfSpeech: "MASCULINE NOUN", Endings: "",
                new List<Context>
                {
                    new Context("(toxic substance)", "1",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "venom (of an animal)",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d533b470-18a4-4cae-ad08-3ee8858ae02c.jpg",
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "La herida aún tiene el veneno dentro.", Translation: "The wound still has venom in it.")
                                    }),
                            new Meaning(
                                Original: "poison",
                                Translation: null,
                                AlphabeticalPosition: "b",
                                Tag: null,
                                ImageUrl: "https://d25rq8gxcq0p71.cloudfront.net/dictionary-images/300/d07aa7fd-a3fd-4d06-9751-656180d8b1ee.jpg",
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Estos hongos contienen un veneno mortal.", Translation: "These mushrooms contain a deadly poison.")
                                    })
                        }),
                    new Context("(ill intent)", "2",
                        new List<Meaning>
                        {
                            new Meaning(
                                Original: "venom",
                                Translation: null,
                                AlphabeticalPosition: "a",
                                Tag: null,
                                ImageUrl: null,
                                LookupUrl: null,
                                Examples: new List<Example>()
                                    {
                                        new Example(Original: "Le espetó con tal veneno que ni se atrevió a responderle.", Translation: "She spat at him with such venom that he didn't even dare respond.")
                                    })
                        }),
                }
            );

            return new DefinitionViewModel(
                definition,
                SourceLanguage.Spanish,
                true);
        }

        #endregion

        private static string StyleAttributeForTag => "style=\"color:#404040; background-color:#eaeff2; border:1px solid #CCCCCC; margin-right:10px; font-size: 80%;\"";

        #endregion
    }
}
