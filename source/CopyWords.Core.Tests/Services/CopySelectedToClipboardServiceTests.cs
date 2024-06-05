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
        public async Task CompileBackAsync_WhenExampleForOneDefinitionIsSelected_DoesNotAddNumbers()
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
        public async Task CompileExamplesAsync_WhenTwoExampleSelected_AddsNumbers()
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
            var definitionVM = new DefinitionViewModel(new Definition(Meaning: "spids pind af træ eller metal til at stikke gennem kød og grøntsager under grilning",
                Examples:
                [
                    "Form kødet til små boller og stik dem på et grillspyd – ca. 4-5 stykker på hver",
                    "Det lykkedes mig at få bestilt hovedretten – den velkendte, græske specialitet, som består af grillspyd med skiftevis lammekød og tomater"
                ]));

            return new ObservableCollection<DefinitionViewModel>()
            {
                definitionVM
            };
        }

        #endregion
    }
}
