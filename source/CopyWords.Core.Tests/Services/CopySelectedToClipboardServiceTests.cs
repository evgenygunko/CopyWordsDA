using System.Collections.ObjectModel;
using AutoFixture;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class CopySelectedToClipboardServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for CompileFrontAsync

        [TestMethod]
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
        }

        #endregion

        #region Tests for CompileBackAsync

        [TestMethod]
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
        }

        #endregion

        #region Tests for CompileExamplesAsync

        [TestMethod]
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
        }

        #endregion

        #region Private Methods

        internal static ObservableCollection<DefinitionViewModel> CreateVMForGrillspyd()
        {
            var definition = new Definition(
                Meaning: "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning",
                Tag: null,
                PartOfSpeech: "substantiv",
                Position: 1,
                Translations: [
                    new Translation("", "", null, new List<Example>()
                    {
                        new Example("Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver", null, null),
                        new Example("Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater", null, null)
                    })
                ]);

            var definitionVM = new DefinitionViewModel(definition, pos: 1);

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        private static ObservableCollection<DefinitionViewModel> CreateVMForHaj()
        {
            var definition1 = new Definition(
                Meaning: "stor, langstrakt bruskfisk",
                Tag: null,
                PartOfSpeech: "substantiv",
                Position: 1,
                Translations: [
                    new Translation("", "", null, new List<Example>()
                    {
                        new Example("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham", null, null)
                    })
                ]);

            var definition2 = new Definition(
                Meaning: "grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning",
                Tag: "SLANG",
                PartOfSpeech: "substantiv",
                Position: 2,
                Translations: [
                    new Translation("", "", null, new List<Example>()
                    {
                        new Example("-", null, null)
                    })
                ]);

            var definition3 = new Definition(
                Meaning: "person der er særlig dygtig til et spil, håndværk el.lign.",
                Tag: "SLANG",
                PartOfSpeech: "substantiv",
                Position: 3,
                Translations: [
                    new Translation("", "", null, new List<Example>()
                    {
                        new Example("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb", null, null)
                    })
                ]);

            return new ObservableCollection<DefinitionViewModel>()
            {
                new DefinitionViewModel(definition1, pos: 1),
                new DefinitionViewModel(definition2, pos: 2),
                new DefinitionViewModel(definition3, pos: 3),
            };
        }

        private static string StyleAttributeForTag => "style=\"color:#404040; background-color:#eaeff2; border:1px solid #CCCCCC; margin-right:10px; font-size: 80%;\"";

        #endregion
    }
}
