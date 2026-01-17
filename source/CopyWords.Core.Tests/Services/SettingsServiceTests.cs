// Ignore Spelling: App Api

using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SettingsServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for LoadSettings

        [TestMethod]
        public void LoadSettings_Should_CallGetOnPreferences()
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var sut = _fixture.Create<SettingsService>();

            AppSettings appSettings = sut.LoadSettings();
            appSettings.Should().NotBeNull();

            preferencesMock.Verify(x => x.Get("MainWindowWidth", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("MainWindowHeight", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("MainWindowXPos", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("MainWindowYPos", It.IsAny<string>(), It.IsAny<string>()));

            preferencesMock.Verify(x => x.Get("AnkiDeckNameDanish", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("AnkiDeckNameSpanish", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("AnkiModelName", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("AnkiSoundsFolder", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("ShowCopyButtons", It.IsAny<bool>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("ShowCopyWithAnkiConnectButton", It.IsAny<bool>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("CopyTranslatedMeanings", It.IsAny<bool>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("SelectedParser", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("UseDarkTheme", It.IsAny<bool>(), It.IsAny<string>()));
        }

        #endregion

        #region Tests for SaveSettings

        [TestMethod]
        public void SaveSettings_Should_CallSetOnPreferences()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var sut = _fixture.Create<SettingsService>();
            sut.SaveSettings(appSettings);

            preferencesMock.Verify(x => x.Set("MainWindowWidth", appSettings.MainWindowWidth.ToString(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("MainWindowHeight", appSettings.MainWindowHeight.ToString(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("MainWindowXPos", appSettings.MainWindowXPos.ToString(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("MainWindowYPos", appSettings.MainWindowYPos.ToString(), It.IsAny<string>()));

            preferencesMock.Verify(x => x.Set("AnkiDeckNameDanish", appSettings.AnkiDeckNameDanish, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("AnkiDeckNameSpanish", appSettings.AnkiDeckNameSpanish, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("AnkiModelName", appSettings.AnkiModelName, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("AnkiSoundsFolder", appSettings.AnkiSoundsFolder, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("ShowCopyButtons", appSettings.ShowCopyButtons, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("ShowCopyWithAnkiConnectButton", appSettings.ShowCopyWithAnkiConnectButton, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("CopyTranslatedMeanings", appSettings.CopyTranslatedMeanings, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("SelectedParser", appSettings.SelectedParser, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("UseDarkTheme", appSettings.UseDarkTheme, It.IsAny<string>()));
        }

        #endregion

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void GetShowCopyButtons_OnAndroid_CallPreferencesWithDefaultValueFalse(bool expectedValue)
        {
            const bool defaultValue = false;

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("ShowCopyButtons", defaultValue, null)).Returns(expectedValue).Verifiable();

            var sut = _fixture.Create<SettingsService>();
            bool result = sut.GetShowCopyButtons();

            result.Should().Be(expectedValue);
            preferencesMock.Verify();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void GetShowCopyButtons_OnDesktop_CallPreferencesWithDefaultValueTrue(bool expectedValue)
        {
            const bool defaultValue = true;

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("ShowCopyButtons", defaultValue, null)).Returns(expectedValue).Verifiable();

            var sut = _fixture.Create<SettingsService>();
            bool result = sut.GetShowCopyButtons();

            result.Should().Be(expectedValue);
            preferencesMock.Verify();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetShowCopyButtons_Should_CallPreferencesSet(bool value)
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var sut = _fixture.Create<SettingsService>();
            sut.SetShowCopyButtons(value);

            preferencesMock.Verify(x => x.Set("ShowCopyButtons", value, It.IsAny<string>()));
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetCopyTranslatedMeanings_Should_CallPreferencesSet(bool value)
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var sut = _fixture.Create<SettingsService>();
            sut.SetCopyTranslatedMeanings(value);

            preferencesMock.Verify(x => x.Set("CopyTranslatedMeanings", value, It.IsAny<string>()));
        }

        [TestMethod]
        public void GetSelectedParser_Should_CallPreferencesSet()
        {
            const string expectedValue = "Spanish";

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("SelectedParser", SourceLanguage.Danish.ToString(), null)).Returns(expectedValue).Verifiable();

            var sut = _fixture.Create<SettingsService>();
            string result = sut.GetSelectedParser();

            result.Should().Be(expectedValue);
            preferencesMock.Verify();
        }

        [TestMethod]
        public void SetSelectedParser_Should_CallPreferencesSet()
        {
            const string value = "Spanish";

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var sut = _fixture.Create<SettingsService>();
            sut.SetSelectedParser(value);

            preferencesMock.Verify(x => x.Set("SelectedParser", value, It.IsAny<string>()));
        }

        [TestMethod]
        public void LoadHistory_Should_ReturnEmptyCollectionWhenNoHistory()
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("SelectedParser", SourceLanguage.Danish.ToString(), null)).Returns("Danish");
            preferencesMock.Setup(x => x.Get("History_Danish", string.Empty, null)).Returns(string.Empty);

            var sut = _fixture.Create<SettingsService>();
            IEnumerable<string> result = sut.LoadHistory();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void LoadHistory_Should_ReturnHistoryItems()
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("SelectedParser", SourceLanguage.Danish.ToString(), null)).Returns("Danish");
            preferencesMock.Setup(x => x.Get("History_Danish", string.Empty, null)).Returns("word1;word2;word3");

            var sut = _fixture.Create<SettingsService>();
            IEnumerable<string> result = sut.LoadHistory();

            result.Should().HaveCount(3);
            result.Should().Equal("word1", "word2", "word3");
        }

        [TestMethod]
        public void ClearHistory_Should_CallPreferencesRemove()
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("SelectedParser", SourceLanguage.Danish.ToString(), null)).Returns("Danish");

            var sut = _fixture.Create<SettingsService>();
            sut.ClearHistory();

            preferencesMock.Verify(x => x.Remove("History_Danish", It.IsAny<string>()));
        }
    }
}