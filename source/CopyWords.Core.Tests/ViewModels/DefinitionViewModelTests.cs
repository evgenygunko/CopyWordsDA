using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class DefinitionViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Ctor_Should_SetObservableProperties()
        {
            string word = _fixture.Create<string>();
            Definition definition = _fixture.Create<Definition>();

            AppSettings appSettings = _fixture.Create<AppSettings>();

            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new DefinitionViewModel(word, definition, settingsServiceMock.Object);

            sut.Word.Should().Be(word);
            sut.HeadwordViewModel.Should().NotBeNull();
            sut.PartOfSpeech.Should().Be(definition.PartOfSpeech);
            sut.Endings.Should().Be(definition.Endings);
            sut.ContextViewModels.Should().NotBeNull();
        }
    }
}
