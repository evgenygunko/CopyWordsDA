using System.Runtime.Versioning;
using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
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
            AppSettings appSettings = _fixture.Create<AppSettings>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                new Mock<IFileIOService>().Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.AnkiSoundsFolder.Should().Be(appSettings.AnkiSoundsFolder);
            sut.UseMp3gain.Should().Be(appSettings.UseMp3gain);
            sut.Mp3gainPath.Should().Be(appSettings.Mp3gainPath);
            sut.UseTranslator.Should().Be(appSettings.UseTranslator);
            sut.TranslatorApiUrl.Should().Be(appSettings.TranslatorApiUrl);
        }

        #region Tests for CanSaveSettings

        [TestMethod]
        public void CanSaveSettings_WhenAnkiSoundsFolderExistsAndFfmpegBinFolderIsSet_ReturnsTrue()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.UseMp3gain = false;

            const bool ankiSoundsFolderExists = true;
            const bool ffmpegBinFolderExists = true;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.FfmpegBinFolder)).Returns(ffmpegBinFolderExists);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.CanSaveSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenAnkiSoundsFolderDoesNotExistAndFfmpegBinFolderIsSet_ReturnsFalse()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            const bool ankiSoundsFolderExists = false;
            const bool ffmpegBinFolderExists = true;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.FfmpegBinFolder)).Returns(ffmpegBinFolderExists);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSettings_WhenAnkiSoundsFolderExistsButFfmpegBinFolderDoesNot_ReturnsFalse()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            const bool ankiSoundsFolderExists = true;
            const bool ffmpegBinFolderExists = false;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(ankiSoundsFolderExists);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.FfmpegBinFolder)).Returns(ffmpegBinFolderExists);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSettings_WhenUseMp3gainAndMp3gainPathIsValid_ReturnsTrue()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.UseMp3gain = true;
            appSettings.UseTranslator = false;

            const bool mp3gainPathExists = true;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.FfmpegBinFolder)).Returns(true); ;
            fileIOServiceMock.Setup(x => x.FileExists(appSettings.Mp3gainPath)).Returns(mp3gainPathExists);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.CanSaveSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenUseMp3gainAndMp3gainPathIsInvalid_ReturnsFalse()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.UseMp3gain = true;
            appSettings.UseTranslator = false;

            const bool mp3gainPathExists = false;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.FfmpegBinFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.FileExists(appSettings.Mp3gainPath)).Returns(mp3gainPathExists);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.CanSaveSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSettings_WhenTranslatorApiUrlIsNotNullAndValidUrl_ReturnsTrue()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.UseMp3gain = false;
            appSettings.UseTranslator = true;
            appSettings.TranslatorApiUrl = new Uri("https://google.com").ToString();

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.FfmpegBinFolder)).Returns(true);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.CanSaveSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenUseTranslatorAndTranslatorApiUrlIsInvalidUrl_ReturnsFalse()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = true;
            appSettings.UseMp3gain = false;

            const bool ankiSoundsFolderExists = true;

            Mock<ISettingsService> settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(ankiSoundsFolderExists);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                new Mock<IShellService>().Object,
                fileIOServiceMock.Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

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

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                new Mock<IDialogService>().Object,
                shellServiceMock.Object,
                new Mock<IFileIOService>().Object,
                new Mock<IFolderPicker>().Object,
                new Mock<IFilePicker>().Object);

            sut.AnkiSoundsFolder = ankiSoundsFolder;
            sut.UseMp3gain = useMp3gain;
            sut.Mp3gainPath = mp3gainPath;
            sut.UseTranslator = useTranslator;
            sut.TranslatorApiUrl = translatorApiUrl;

            await sut.SaveSettingsAsync();

            settingsServiceMock.Verify(x => x.SaveSettings(It.IsAny<AppSettings>()));

            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(st => st.Location.ToString() == "..")));
        }

        #endregion

        #region Tests for ExportSettingsAsync

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [TestMethod]
        public async Task ExportSettingsAsync_WhenFolderIsNotSelected_DoesNotExportSettings()
        {
            var folder = _fixture.Create<CommunityToolkit.Maui.Core.Primitives.Folder>();
            FolderPickerResult folderPickerResult = new FolderPickerResult(folder, Exception: new Exception("folder is not selected"));

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var folderPickerMock = _fixture.Freeze<Mock<IFolderPicker>>();
            folderPickerMock.Setup(x => x.PickAsync(It.IsAny<CancellationToken>())).ReturnsAsync(folderPickerResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                new Mock<IFileIOService>().Object,
                folderPickerMock.Object,
                new Mock<IFilePicker>().Object);
            await sut.ExportSettingsAsync(default);

            settingsServiceMock.Verify(x => x.ExportSettingsAsync(It.IsAny<string>()), Times.Never);
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [TestMethod]
        public async Task ExportSettingsAsync_WhenFileExistsAndNoOverwrite_DoesNotExportSettings()
        {
            const bool overwrite = false;

            var folder = _fixture.Create<CommunityToolkit.Maui.Core.Primitives.Folder>();
            FolderPickerResult folderPickerResult = new FolderPickerResult(folder, Exception: null);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlert("File already exists", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(overwrite);

            var folderPickerMock = _fixture.Freeze<Mock<IFolderPicker>>();
            folderPickerMock.Setup(x => x.PickAsync(It.IsAny<CancellationToken>())).ReturnsAsync(folderPickerResult);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                fileIOServiceMock.Object,
                folderPickerMock.Object,
                new Mock<IFilePicker>().Object);
            await sut.ExportSettingsAsync(default);

            dialogServiceMock.Verify(x => x.DisplayAlert("File already exists", It.IsAny<string>(), "Yes", "No"));
            settingsServiceMock.Verify(x => x.ExportSettingsAsync(It.IsAny<string>()), Times.Never);
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [TestMethod]
        public async Task ExportSettingsAsync_WhenFileExistsAndOverwrite_ExportsSettings()
        {
            const bool overwrite = true;

            var folder = _fixture.Create<CommunityToolkit.Maui.Core.Primitives.Folder>();
            FolderPickerResult folderPickerResult = new FolderPickerResult(folder, Exception: null);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlert("File already exists", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(overwrite);

            var folderPickerMock = _fixture.Freeze<Mock<IFolderPicker>>();
            folderPickerMock.Setup(x => x.PickAsync(It.IsAny<CancellationToken>())).ReturnsAsync(folderPickerResult);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                fileIOServiceMock.Object,
                folderPickerMock.Object,
                new Mock<IFilePicker>().Object);
            await sut.ExportSettingsAsync(default);

            dialogServiceMock.Verify(x => x.DisplayAlert("File already exists", It.IsAny<string>(), "Yes", "No"));
            settingsServiceMock.Verify(x => x.ExportSettingsAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast(It.Is<string>(s => s.StartsWith("Settings exported to"))));
            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(s => s.Location.ToString() == "..")));
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [TestMethod]
        public async Task ExportSettingsAsync_WhenUserSelectsFolder_ExportsSettingsToAFile()
        {
            var folder = _fixture.Create<CommunityToolkit.Maui.Core.Primitives.Folder>();
            FolderPickerResult folderPickerResult = new FolderPickerResult(folder, Exception: null);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var folderPickerMock = _fixture.Freeze<Mock<IFolderPicker>>();
            folderPickerMock.Setup(x => x.PickAsync(It.IsAny<CancellationToken>())).ReturnsAsync(folderPickerResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                new Mock<IFileIOService>().Object,
                folderPickerMock.Object,
                new Mock<IFilePicker>().Object);
            await sut.ExportSettingsAsync(default);

            settingsServiceMock.Verify(x => x.ExportSettingsAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast(It.Is<string>(s => s.StartsWith("Settings exported to"))));
            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(s => s.Location.ToString() == "..")));
        }

        #endregion

        #region Tests for ImportSettingsAsync

        [TestMethod]
        public async Task ImportSettingsAsync_WhenFileIsNotSelected_DoesNotImportSettings()
        {
            FileResult? fileResult = null;

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var filePickerMock = _fixture.Freeze<Mock<IFilePicker>>();
            filePickerMock.Setup(x => x.PickAsync(It.IsAny<PickOptions>())).ReturnsAsync(fileResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                new Mock<IFileIOService>().Object,
                new Mock<IFolderPicker>().Object,
                filePickerMock.Object);
            await sut.ImportSettingsAsync();

            settingsServiceMock.Verify(x => x.ImportSettingsAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task ImportSettingsAsync_WhenExceptionIsThrown_ShowsAlert()
        {
            var fileResult = _fixture.Create<FileResult>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.ImportSettingsAsync(It.IsAny<string>())).ThrowsAsync(new Exception("exception from unit test"));

            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var filePickerMock = _fixture.Freeze<Mock<IFilePicker>>();
            filePickerMock.Setup(x => x.PickAsync(It.IsAny<PickOptions>())).ReturnsAsync(fileResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                new Mock<IFileIOService>().Object,
                new Mock<IFolderPicker>().Object,
                filePickerMock.Object);
            await sut.ImportSettingsAsync();

            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot import setting", It.IsAny<string>(), "OK"));
        }

        [TestMethod]
        public async Task ImportSettingsAsync_WhenFileIsSelected_ImportsSettings()
        {
            var fileResult = _fixture.Create<FileResult>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var filePickerMock = _fixture.Freeze<Mock<IFilePicker>>();
            filePickerMock.Setup(x => x.PickAsync(It.IsAny<PickOptions>())).ReturnsAsync(fileResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                new Mock<IFileIOService>().Object,
                new Mock<IFolderPicker>().Object,
                filePickerMock.Object);
            await sut.ImportSettingsAsync();

            settingsServiceMock.Verify(x => x.ImportSettingsAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Settings imported."));
            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(s => s.Location.ToString() == "..")));
        }

        #endregion
    }
}
