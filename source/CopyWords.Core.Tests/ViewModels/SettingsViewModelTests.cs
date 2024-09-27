using AutoFixture;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class SettingsViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Constructor_Should_ReadValuesFromSettingsService()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            bool useMp3gain = _fixture.Create<bool>();
            string mp3gainPath = _fixture.Create<string>();
            bool useTranslator = _fixture.Create<bool>();
            string translatorApiUrl = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, new Mock<IFileIOService>().Object);

            sut.AnkiSoundsFolder.Should().Be(ankiSoundsFolder);
            sut.UseMp3gain.Should().Be(useMp3gain);
            sut.Mp3gainPath.Should().Be(mp3gainPath);
            sut.UseTranslator.Should().Be(useTranslator);
            sut.TranslatorApiUrl.Should().Be(translatorApiUrl);
        }

        #region Tests for CanSaveSettings

        [TestMethod]
        public void CanSaveSettings_WhenAnkiSoundsFolderExistsAndFfmpegBinFolderIsSet_ReturnsTrue()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = true;

            string ffmpegBinFolder = _fixture.Create<string>();
            const bool ffmpegBinFolderExists = true;

            const bool useMp3gain = false;
            string mp3gainPath = string.Empty;
            const bool useTranslator = false;
            string translatorApiUrl = string.Empty;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.GetFfmpegBinFolder()).Returns(ffmpegBinFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(ankiSoundsFolder)).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.DirectoryExists(ffmpegBinFolder)).Returns(ffmpegBinFolderExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenAnkiSoundsFolderDoesNotExistAndFfmpegBinFolderIsSet_ReturnsFalse()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = false;

            string ffmpegBinFolder = _fixture.Create<string>();
            const bool ffmpegBinFolderExists = true;

            const bool useMp3gain = false;
            string mp3gainPath = string.Empty;
            const bool useTranslator = false;
            string translatorApiUrl = string.Empty;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.GetFfmpegBinFolder()).Returns(ffmpegBinFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(ankiSoundsFolder)).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.DirectoryExists(ffmpegBinFolder)).Returns(ffmpegBinFolderExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSettings_WhenAnkiSoundsFolderExistsButFfmpegBinFolderDoesNot_ReturnsFalse()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = true;

            string ffmpegBinFolder = _fixture.Create<string>();
            const bool ffmpegBinFolderExists = false;

            const bool useMp3gain = false;
            string mp3gainPath = string.Empty;
            const bool useTranslator = false;
            string translatorApiUrl = string.Empty;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.GetFfmpegBinFolder()).Returns(ffmpegBinFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(ankiSoundsFolder)).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.DirectoryExists(ffmpegBinFolder)).Returns(ffmpegBinFolderExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSettings_WhenUseMp3gainAndMp3gainPathIsValid_ReturnsTrue()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = true;

            const bool useMp3gain = true;
            string mp3gainPath = _fixture.Create<string>();
            const bool mp3gainPathExists = true;

            const bool useTranslator = false;
            string translatorApiUrl = string.Empty;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string?>())).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string?>())).Returns(mp3gainPathExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenUseMp3gainAndMp3gainPathIsInvalid_ReturnsFalse()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = true;

            const bool useMp3gain = true;
            string mp3gainPath = _fixture.Create<string>();
            const bool mp3gainPathExists = false;

            const bool useTranslator = false;
            string translatorApiUrl = string.Empty;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string?>())).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string?>())).Returns(mp3gainPathExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSettings_WhenTranslatorApiUrlIsNotNullAndValidUrl_ReturnsTrue()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = true;

            const bool useMp3gain = false;
            string mp3gainPath = string.Empty;

            const bool useTranslator = true;
            string translatorApiUrl = new Uri("https://google.com").ToString();

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string?>())).Returns(ankiSoundsFolderExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenUseTranslatorAndTranslatorApiUrlIsInvalidUrl_ReturnsFalse()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            const bool ankiSoundsFolderExists = true;

            const bool useMp3gain = false;
            string mp3gainPath = string.Empty;

            const bool useTranslator = true;
            string translatorApiUrl = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetAnkiSoundsFolder()).Returns(ankiSoundsFolder);
            settingsServiceMock.Setup(x => x.UseMp3gain).Returns(useMp3gain);
            settingsServiceMock.Setup(x => x.GetMp3gainPath()).Returns(mp3gainPath);
            settingsServiceMock.Setup(x => x.UseTranslator).Returns(useTranslator);
            settingsServiceMock.Setup(x => x.GetTranslatorApiUrl()).Returns(translatorApiUrl);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(It.IsAny<string?>())).Returns(ankiSoundsFolderExists);

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, new Mock<IShellService>().Object, fileIOServiceMock.Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        #endregion

        #region Tests for SaveSettingsAsync

        [TestMethod]
        public async Task SaveSettingsAsync_Should_CallSettingsService()
        {
            string ankiSoundsFolder = _fixture.Create<string>();
            bool useMp3gain = _fixture.Create<bool>();
            string mp3gainPath = _fixture.Create<string>();
            bool useTranslator = _fixture.Create<bool>();
            string translatorApiUrl = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            Mock<IShellService> shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new SettingsViewModel(settingsServiceMock.Object, new Mock<IDialogService>().Object, shellServiceMock.Object, new Mock<IFileIOService>().Object);

            sut.AnkiSoundsFolder = ankiSoundsFolder;
            sut.UseMp3gain = useMp3gain;
            sut.Mp3gainPath = mp3gainPath;
            sut.UseTranslator = useTranslator;
            sut.TranslatorApiUrl = translatorApiUrl;

            await sut.SaveSettingsAsync();

            settingsServiceMock.Verify(x => x.SetAnkiSoundsFolder(ankiSoundsFolder));
            settingsServiceMock.VerifySet(x => x.UseMp3gain = useMp3gain);
            settingsServiceMock.Verify(x => x.SetMp3gainPath(mp3gainPath));
            settingsServiceMock.VerifySet(x => x.UseTranslator = useTranslator);
            settingsServiceMock.Verify(x => x.SetTranslatorApiUrl(translatorApiUrl));

            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(st => st.Location.ToString() == "..")));
        }

        #endregion
    }
}
