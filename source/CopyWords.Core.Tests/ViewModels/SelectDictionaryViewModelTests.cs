using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class SelectDictionaryViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Constructor_Should_SetSelectedParser()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new SelectDictionaryViewModel(settingsServiceMock.Object);

            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.SelectedParser.SourceLanguage.ToString().Should().Be(appSettings.SelectedParser);
        }

        [TestMethod]
        public void SaveSelectedParserInSettings_Should_SaveSettings()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            Models.Parsers parser = _fixture.Create<Models.Parsers>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new SelectDictionaryViewModel(settingsServiceMock.Object);
            sut.SaveSelectedParserInSettings(parser);

            settingsServiceMock.Verify(x => x.LoadSettings());
            settingsServiceMock.Verify(x => x.SaveSettings(It.Is<AppSettings>(p => p.SelectedParser == parser.SourceLanguage.ToString())));
        }
    }
}
