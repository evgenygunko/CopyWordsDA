// Ignore Spelling: Ffmpeg Api

using System.Runtime.Versioning;
using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class SettingsViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for Init

        [TestMethod]
        public void Init_Should_ReadValuesFromSettingsService()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
            sut.Init();

            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.AnkiSoundsFolder.Should().Be(appSettings.AnkiSoundsFolder);
            sut.UseMp3gain.Should().Be(appSettings.UseMp3gain);
            sut.Mp3gainPath.Should().Be(appSettings.Mp3gainPath);
            sut.UseTranslator.Should().Be(appSettings.UseTranslator);
            sut.TranslatorApiUrl.Should().Be(appSettings.TranslatorApiUrl);
            sut.TranslateMeanings.Should().Be(appSettings.TranslateMeanings);
            sut.TranslateHeadword.Should().Be(appSettings.TranslateHeadword);
        }

        #endregion

        #region Tests for SaveSettingsAsync

        [TestMethod]
        public async Task SaveSettingsAsync_Should_CallSettingsService()
        {
            string validationErrors = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            Mock<IShellService> shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                shellServiceMock.Object,
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            await sut.SaveSettingsAsync();

            settingsServiceMock.Verify(x => x.SaveSettings(It.IsAny<AppSettings>()));
            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(st => st.Location.ToString() == "..")));
        }

        #endregion

        #region Tests for CanSaveSettings

        [TestMethod]
        public void CanSaveSettings_WhenNoValidationErrors_ReturnsTrue()
        {
            var validationResult = _fixture.Create<ValidationResult>();
            validationResult.Errors.Clear();

            var settingsViewModelValidatorMock = _fixture.Freeze<Mock<IValidator<SettingsViewModel>>>();
            settingsViewModelValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<SettingsViewModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

            var sut = new SettingsViewModel(
                Mock.Of<ISettingsService>(x => x.LoadSettings() == new AppSettings()),
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                settingsViewModelValidatorMock.Object);

            bool result = sut.CanSaveSettings();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void CanSaveSettings_WhenThereAreValidationErrors_ReturnsFalse()
        {
            var validationResult = _fixture.Create<ValidationResult>();
            validationResult.Errors.Clear();
            validationResult.Errors.Add(new ValidationFailure("property1", "property1 cannot be null"));

            var settingsViewModelValidatorMock = _fixture.Freeze<Mock<IValidator<SettingsViewModel>>>();
            settingsViewModelValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<SettingsViewModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

            var sut = new SettingsViewModel(
                Mock.Of<ISettingsService>(x => x.LoadSettings() == new AppSettings()),
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                settingsViewModelValidatorMock.Object);

            bool result = sut.CanSaveSettings();

            result.Should().BeFalse();
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

            var folderPickerMock = _fixture.Freeze<Mock<IFolderPicker>>();
            folderPickerMock.Setup(x => x.PickAsync(It.IsAny<CancellationToken>())).ReturnsAsync(folderPickerResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                folderPickerMock.Object,
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
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

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayAlert("File already exists", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(overwrite);

            var folderPickerMock = _fixture.Freeze<Mock<IFolderPicker>>();
            folderPickerMock.Setup(x => x.PickAsync(It.IsAny<CancellationToken>())).ReturnsAsync(folderPickerResult);

            var fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                Mock.Of<IShellService>(),
                fileIOServiceMock.Object,
                folderPickerMock.Object,
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
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
                Mock.Of<IFileIOService>(),
                folderPickerMock.Object,
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
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
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                filePickerMock.Object,
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
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
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                filePickerMock.Object,
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
            await sut.ImportSettingsAsync();

            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot import setting", It.IsAny<string>(), "OK"));
        }

        [TestMethod]
        public async Task ImportSettingsAsync_WhenFileIsSelected_ImportsSettings()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.ImportSettingsAsync(It.IsAny<string>())).ReturnsAsync(appSettings);

            var fileResult = _fixture.Create<FileResult>();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var filePickerMock = _fixture.Freeze<Mock<IFilePicker>>();
            filePickerMock.Setup(x => x.PickAsync(It.IsAny<PickOptions>())).ReturnsAsync(fileResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                filePickerMock.Object,
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IValidator<SettingsViewModel>>());
            await sut.ImportSettingsAsync();

            sut.AnkiSoundsFolder.Should().Be(appSettings.AnkiSoundsFolder);
            sut.UseMp3gain.Should().Be(appSettings.UseMp3gain);
            sut.Mp3gainPath.Should().Be(appSettings.Mp3gainPath);
            sut.UseTranslator.Should().Be(appSettings.UseTranslator);
            sut.TranslatorApiUrl.Should().Be(appSettings.TranslatorApiUrl);
            sut.TranslateMeanings.Should().Be(appSettings.TranslateMeanings);
            sut.TranslateHeadword.Should().Be(appSettings.TranslateHeadword);

            settingsServiceMock.Verify(x => x.ImportSettingsAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Settings imported."));
        }

        #endregion

        #region Tests for CanUpdateIndividualSettings

        [TestMethod]
        public void CanUpdateIndividualSettings_OnAndroid_ReturnsTrue()
        {
            var sut = new SettingsViewModel(
                Mock.Of<ISettingsService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.CanUpdateIndividualSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanUpdateIndividualSettings_OnWindows_ReturnsFalse()
        {
            var sut = new SettingsViewModel(
                Mock.Of<ISettingsService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.CanUpdateIndividualSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanUpdateIndividualSettings_OnMacOS_ReturnsFalse()
        {
            var sut = new SettingsViewModel(
                Mock.Of<ISettingsService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.MacCatalyst),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.CanUpdateIndividualSettings.Should().BeFalse();
        }

        #endregion

        #region Tests for OnUseTranslatorChangedInternal

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OnUseTranslatorChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnUseTranslatorChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetUseTranslator(value));
        }

        [TestMethod]
        public void OnUseTranslatorChangedInternal_WhenNotInitialized_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.OnUseTranslatorChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetUseTranslator(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void OnUseTranslatorChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnUseTranslatorChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetUseTranslator(It.IsAny<bool>()), Times.Never);

        }

        #endregion

        #region Tests for OnTranslatorApiUrlChangedInternal

        [TestMethod]
        public void OnTranslatorApiUrlChangedInternal_WhenTranslatorApiUrlIsValid_CallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslatorApiUrlChangedInternal("http://localhost:7014/api/Translate");

            settingsServiceMock.Verify(x => x.SetTranslatorApiUrl("http://localhost:7014/api/Translate"));
        }

        [TestMethod]
        public void OnTranslatorApiUrlChangedInternal_WhenTranslatorApiUrlIsNoValid_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslatorApiUrlChangedInternal("abcdef");

            settingsServiceMock.Verify(x => x.SetTranslatorApiUrl(It.IsAny<string?>()), Times.Never);
        }

        [TestMethod]
        public void OnTranslatorApiUrlChangedInternal_WhenNotInitialized_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.OnTranslatorApiUrlChangedInternal(null);

            settingsServiceMock.Verify(x => x.SetTranslatorApiUrl(It.IsAny<string?>()), Times.Never);
        }

        [TestMethod]
        public void OnTranslatorApiUrlChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslatorApiUrlChangedInternal(null);

            settingsServiceMock.Verify(x => x.SetTranslatorApiUrl(It.IsAny<string?>()), Times.Never);

        }

        #endregion

        #region Tests for OnTranslateHeadwordChangedInternal

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OnTranslateHeadwordChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslateHeadwordChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetTranslateHeadword(value));
        }

        [TestMethod]
        public void OnTranslateHeadwordChangedInternal_WhenNotInitialized_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.OnTranslateHeadwordChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetTranslateHeadword(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void OnTranslateHeadwordChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslateHeadwordChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetTranslateHeadword(It.IsAny<bool>()), Times.Never);

        }
        #endregion

        #region Tests for OnTranslateMeaningsChangedInternal

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OnTranslateMeaningsChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslateMeaningsChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetTranslateMeanings(value));
        }

        [TestMethod]
        public void OnTranslateMeaningsChangedInternal_WhenNotInitialized_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.OnTranslateMeaningsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetTranslateMeanings(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void OnTranslateMeaningsChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFileIOService>(),
                Mock.Of<IFolderPicker>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnTranslateMeaningsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetTranslateMeanings(It.IsAny<bool>()), Times.Never);

        }

        #endregion
    }
}
