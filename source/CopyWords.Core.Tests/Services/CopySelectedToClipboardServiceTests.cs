using System.Collections.ObjectModel;
using Autofac.Extras.Moq;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class CopySelectedToClipboardServiceTests
    {
        #region Tests for CompileFrontAsync

        [TestMethod]
        public async Task CompileFrontAsync_WhenExampleForOneDefinitionIsSelected_ReturnsFormattedHeadWord()
        {
            using (var mock = AutoMock.GetLoose())
            {
                const string meaning = "grillspyd";

                var sut = mock.Create<CopySelectedToClipboardService>();

                string front = await sut.CompileFrontAsync(meaning);

                front.Should().Be("grillspyd");
            }
        }

        #endregion

        #region Tests for CompileBackAsync

        [TestMethod]
        public async Task CompileBackAsync_WhenOneExampleIsSelected_DoesNotAddNumbers()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForGrillspyd();
                wordVariantVMs[0].Examples[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning");
            }
        }

        [TestMethod]
        public async Task CompileBackAsync_WhenSeveralExamplesAreSelected_AddsNumbers()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForHaj();
                wordVariantVMs[0].Examples[0].IsChecked = true;
                wordVariantVMs[1].Examples[0].IsChecked = true;
                wordVariantVMs[2].Examples[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileBackAsync(wordVariantVMs);

                result.Should().Be("1. stor, langstrakt bruskfisk" + Environment.NewLine +
                    "2. grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning" + Environment.NewLine +
                    "3. person der er særlig dygtig til et spil, håndværk el.lign.");
            }
        }

        #endregion

        #region Tests for CompileExamplesAsync

        [TestMethod]
        public async Task CompileExamplesAsync_WhenOneExampleSelected_DoesNotAddNumbers()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForGrillspyd();
                wordVariantVMs[0].Examples[0].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileExamplesAsync(wordVariantVMs);

                result.Should().Be("Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver");
            }
        }

        [TestMethod]
        public async Task CompileExamplesAsync_WhenTwoExamplesSelected_AddsNumbers()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var wordVariantVMs = CreateVMForGrillspyd();
                wordVariantVMs[0].Examples[0].IsChecked = true;
                wordVariantVMs[0].Examples[1].IsChecked = true;

                var sut = mock.Create<CopySelectedToClipboardService>();

                string result = await sut.CompileExamplesAsync(wordVariantVMs);

                result.Should().Be(
                    "1. Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver" + Environment.NewLine +
                    "2. Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater");
            }
        }

        #endregion

        #region Private Methods

        private static ObservableCollection<DefinitionViewModel> CreateVMForGrillspyd()
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
                Tag: null,
                Examples:
                [
                    "-"
                ]);

            var definition3 = new Definition(
                Meaning: "person der er særlig dygtig til et spil, håndværk el.lign.",
                Tag: null,
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

        #endregion
    }
}
