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
        public async Task CompileFrontAsync_ForGrillspyd_ReturnsFormattedFront()
        {
            const string meaning = "grillspyd";
            const string partOfSpeech = "substantiv, intetkøn";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("et grillspyd");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForuUderholdning_ReturnsFormattedFront()
        {
            const string meaning = "underholdning";
            const string partOfSpeech = "substantiv, fælleskøn";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("en underholdning");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForKigge_ReturnsFormattedFront()
        {
            const string meaning = "kigge";
            const string partOfSpeech = "verbum";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("at kigge");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForHøj_ReturnsFormattedFront()
        {
            const string meaning = "høj";
            const string partOfSpeech = "adjektiv";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string front = await sut.CompileFrontAsync(meaning, partOfSpeech);

            front.Should().Be("høj <span style=\"color: rgba(0, 0, 0, 0.4)\">ADJEKTIV</span>");
        }

        [TestMethod]
        public async Task CompileFrontAsync_ForIFM_ReturnsFormattedFront()
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
        public async Task CompileBackAsync_WhenTranslationIsSelected_AddsTranslationToResult()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;
            bool isTranslationTranslationChecked = true;
            const string translation = "Шашлыки";

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, isTranslationTranslationChecked, translation);

            result.Should().Be("<span style=\"color: rgba(0, 0, 0, 0.4)\">Шашлыки</span><br>spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenOneExampleIsSelected_DoesNotAddNumbers()
        {
            var wordVariantVMs = CreateVMForGrillspyd();
            wordVariantVMs[0].Examples[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, isTranslationTranslationChecked: false, translation: null);

            result.Should().Be("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenSeveralExamplesAreSelected_AddsNumbers()
        {
            var wordVariantVMs = CreateVMForHaj();
            wordVariantVMs[0].Examples[0].IsChecked = true;
            wordVariantVMs[1].Examples[0].IsChecked = true;
            wordVariantVMs[2].Examples[0].IsChecked = true;

            var sut = _fixture.Create<CopySelectedToClipboardService>();

            string result = await sut.CompileBackAsync(wordVariantVMs, isTranslationTranslationChecked: false, translation: null);

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
                Examples:
                [
                    "Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver",
                    "Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater"
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
                Examples:
                [
                    "Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham"
                ]);

            var definition2 = new Definition(
                Meaning: "grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning",
                Tag: "SLANG",
                Examples:
                [
                    "-"
                ]);

            var definition3 = new Definition(
                Meaning: "person der er særlig dygtig til et spil, håndværk el.lign.",
                Tag: "SLANG",
                Examples:
                [
                    "Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb"
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
