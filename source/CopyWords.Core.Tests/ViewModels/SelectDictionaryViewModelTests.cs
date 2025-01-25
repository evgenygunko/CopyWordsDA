// Ignore Spelling: Popup

using AutoFixture;
using CommunityToolkit.Maui.Core;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Core.ViewModels.Popups;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class SelectDictionaryViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for Init

        [TestMethod]
        public void Init_Should_SetSelectedParserFromSettings()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Spanish.ToString();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<SelectDictionaryViewModel>();
            sut.Init();

            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.SelectedParser.SourceLanguage.ToString().Should().Be(appSettings.SelectedParser);
        }

        [TestMethod]
        public void Init_WhenNoParserSavedInSettings_SelectsFirstParser()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = "non-supported-parser";

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = _fixture.Create<SelectDictionaryViewModel>();
            sut.Init();

            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.SelectedParser.SourceLanguage.Should().Be(SourceLanguage.Danish);
        }

        #endregion

        #region Tests for SelectDictionaryAsync

        [TestMethod]
        public async Task SelectDictionaryAsync_WhenPopupReturnsValue_SavesSettings()
        {
            SourceLanguage newLanguage = SourceLanguage.Danish;

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Spanish.ToString();

            Models.Parsers parser = _fixture.Create<Models.Parsers>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var popupServiceMock = _fixture.Freeze<Mock<IPopupService>>();
            popupServiceMock
                .Setup(x => x.ShowPopupAsync(It.IsAny<Action<SelectDictionaryPopupViewModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newLanguage);

            var sut = _fixture.Create<SelectDictionaryViewModel>();
            await sut.SelectDictionaryAsync(CancellationToken.None);

            sut.SelectedParser.SourceLanguage.Should().Be(newLanguage);

            settingsServiceMock.Verify(x => x.LoadSettings());
            settingsServiceMock.Verify(x => x.SaveSettings(It.Is<AppSettings>(p => p.SelectedParser == newLanguage.ToString())));
        }

        [TestMethod]
        public async Task SelectDictionaryAsync_WhenPopupReturnsNull_DoesNotSaveSettings()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = "Spanish";

            Models.Parsers parser = _fixture.Create<Models.Parsers>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var popupServiceMock = _fixture.Freeze<Mock<IPopupService>>();
            popupServiceMock
                .Setup(x => x.ShowPopupAsync(It.IsAny<Action<SelectDictionaryPopupViewModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            var sut = _fixture.Create<SelectDictionaryViewModel>();
            await sut.SelectDictionaryAsync(CancellationToken.None);

            sut.SelectedParser.SourceLanguage.Should().Be(SourceLanguage.Spanish);
            settingsServiceMock.Verify(x => x.SaveSettings(It.IsAny<AppSettings>()), Times.Never);
        }

        [TestMethod]
        public async Task SelectDictionaryAsync_WhenSelectedLanguageInPopupIsNotSupported_ThrowsException()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = "Spanish";

            Models.Parsers parser = _fixture.Create<Models.Parsers>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var popupServiceMock = _fixture.Freeze<Mock<IPopupService>>();
            popupServiceMock
                .Setup(x => x.ShowPopupAsync(It.IsAny<Action<SelectDictionaryPopupViewModel>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("non-supported-parser");

            var sut = _fixture.Create<SelectDictionaryViewModel>();
            var act = () => sut.SelectDictionaryAsync(CancellationToken.None);

            await act.Should().ThrowAsync<NotSupportedException>().WithMessage("Source language 'non-supported-parser' selected in the popup is not supported.");

            settingsServiceMock.Verify(x => x.SaveSettings(It.IsAny<AppSettings>()), Times.Never);
            sut.SelectedParser.SourceLanguage.Should().Be(SourceLanguage.Spanish);
        }

        #endregion
    }
}
