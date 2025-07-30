// Ignore Spelling: App Api

using System.Runtime.InteropServices;
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

            preferencesMock.Verify(x => x.Get("AnkiSoundsFolder", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("FfmpegBinFolder", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("Mp3gainPath", It.IsAny<string>(), It.IsAny<string>()));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                preferencesMock.Verify(x => x.Get("UseMp3gain", It.IsAny<bool>(), It.IsAny<string>()));
            }

            preferencesMock.Verify(x => x.Get("ShowCopyButtons", It.IsAny<bool>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("CopyTranslatedMeanings", It.IsAny<bool>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("SelectedParser", It.IsAny<string>(), It.IsAny<string>()));
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

            preferencesMock.Verify(x => x.Set("AnkiSoundsFolder", appSettings.AnkiSoundsFolder, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("FfmpegBinFolder", appSettings.FfmpegBinFolder, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("Mp3gainPath", appSettings.Mp3gainPath, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("UseMp3gain", appSettings.UseMp3gain, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("ShowCopyButtons", appSettings.ShowCopyButtons, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("CopyTranslatedMeanings", appSettings.CopyTranslatedMeanings, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("SelectedParser", appSettings.SelectedParser, It.IsAny<string>()));
        }

        #endregion

        #region Tests for ExportSettingsAsync

        [TestMethod]
        public async Task ExportSettingsAsync_Should_CallFileIOService()
        {
            string file = _fixture.Create<string>();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();

            var sut = _fixture.Create<SettingsService>();
            await sut.ExportSettingsAsync(file);

            fileIOServiceMock.Verify(x => x.WriteAllTextAsync(file, It.IsAny<string>(), It.IsAny<CancellationToken>()));

            // ExportSettingsAsync reads settings from current preferences storage
            preferencesMock.Verify(x => x.Get("AnkiSoundsFolder", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("FfmpegBinFolder", It.IsAny<string>(), It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("Mp3gainPath", It.IsAny<string>(), It.IsAny<string>()));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                preferencesMock.Verify(x => x.Get("UseMp3gain", false, It.IsAny<string>()));
            }

            preferencesMock.Verify(x => x.Get("ShowCopyButtons", true, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("CopyTranslatedMeanings", true, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Get("SelectedParser", It.IsAny<string>(), It.IsAny<string>()));
        }

        #endregion

        #region Tests for ImportSettingsAsync

        [TestMethod]
        public async Task ImportSettingsAsync_Should_ReturnAppSettings()
        {
            string file = _fixture.Create<string>();
            string json = "{}";

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

            var sut = _fixture.Create<SettingsService>();
            AppSettings? result = await sut.ImportSettingsAsync(file);

            result.Should().NotBeNull();
            fileIOServiceMock.Verify(x => x.ReadAllTextAsync(file, It.IsAny<CancellationToken>()));

            // ImportSettingsAsync also saves settings to current preferences storage
            preferencesMock.Verify(x => x.Set("AnkiSoundsFolder", result!.AnkiSoundsFolder, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("FfmpegBinFolder", result!.FfmpegBinFolder, It.IsAny<string>()));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                preferencesMock.Verify(x => x.Set("UseMp3gain", false, It.IsAny<string>()));
            }

            preferencesMock.Verify(x => x.Set("ShowCopyButtons", false, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("CopyTranslatedMeanings", false, It.IsAny<string>()));
            preferencesMock.Verify(x => x.Set("SelectedParser", It.IsAny<string>(), It.IsAny<string>()));
        }

        #endregion

        [DataTestMethod]
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

        [DataTestMethod]
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

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SetShowCopyButtons_Should_CallPreferencesSet(bool value)
        {
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();

            var sut = _fixture.Create<SettingsService>();
            sut.SetShowCopyButtons(value);

            preferencesMock.Verify(x => x.Set("ShowCopyButtons", value, It.IsAny<string>()));
        }

        [DataTestMethod]
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
    }
}
