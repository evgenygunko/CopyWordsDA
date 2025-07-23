using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.ViewModels;
using FluentAssertions;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class DefinitionViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Ctor_Should_SetObservableProperties()
        {
            Definition definition = _fixture.Create<Definition>();

            var sut = new DefinitionViewModel(definition, SourceLanguage.Spanish, showTranslatedMeanings: true);

            sut.HeadwordViewModel.Should().NotBeNull();
            sut.HeadwordViewModel.Original.Should().Be(definition.Headword.Original);
            sut.PartOfSpeech.Should().Be(definition.PartOfSpeech);
            sut.Endings.Should().Be(definition.Endings);
            sut.ContextViewModels.Should().NotBeNull();
        }
    }
}
