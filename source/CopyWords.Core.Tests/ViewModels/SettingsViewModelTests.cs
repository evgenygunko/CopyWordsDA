﻿// Ignore Spelling: Ffmpeg Api

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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());
            sut.Init();

            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.AnkiSoundsFolder.Should().Be(appSettings.AnkiSoundsFolder);
            sut.UseMp3gain.Should().Be(appSettings.UseMp3gain);
            sut.Mp3gainPath.Should().Be(appSettings.Mp3gainPath);
            sut.ShowCopyButtons.Should().Be(appSettings.ShowCopyButtons);
            sut.CopyTranslatedMeanings.Should().Be(appSettings.CopyTranslatedMeanings);
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
                settingsViewModelValidatorMock.Object);

            bool result = sut.CanSaveSettings();

            result.Should().BeFalse();
        }

        #endregion

        #region Tests for ExportSettingsAsync

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [TestMethod]
        public async Task ExportSettingsAsync_WhenFileIsNotSelected_DoesNotExportSettings()
        {
            FileSaverResult fileSaverResult = new FileSaverResult(FilePath: null, Exception: new Exception("folder is not selected"));

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync("CopyWords_Settings.json", It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult)
                .Verifiable();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                fileSaverMock.Object,
                Mock.Of<IValidator<SettingsViewModel>>());
            await sut.ExportSettingsAsync(default);

            settingsServiceMock.Verify(x => x.LoadSettings());
            fileSaverMock.Verify();
            dialogServiceMock.VerifyNoOtherCalls();
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst14.0")]
        [TestMethod]
        public async Task ExportSettingsAsync_WhenFileIsSelected_ExportsSettings()
        {
            FileSaverResult fileSaverResult = new FileSaverResult(FilePath: _fixture.Create<string>(), Exception: null);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var fileSaverMock = _fixture.Freeze<Mock<IFileSaver>>();
            fileSaverMock.Setup(x => x.SaveAsync("CopyWords_Settings.json", It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fileSaverResult)
                .Verifiable();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                fileSaverMock.Object,
                Mock.Of<IValidator<SettingsViewModel>>());
            await sut.ExportSettingsAsync(default);

            settingsServiceMock.Verify(x => x.LoadSettings());
            fileSaverMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast(It.Is<string>(s => s.StartsWith("Settings successfully exported to"))));
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
                filePickerMock.Object,
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
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
                filePickerMock.Object,
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());
            await sut.ImportSettingsAsync();

            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot import setting", It.IsAny<string>(), "OK"));
        }

        [TestMethod]
        public async Task ImportSettingsAsync_WhenFileIsSelected_ImportsSettings()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            var fileResult = _fixture.Create<FileResult>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.ImportSettingsAsync(It.IsAny<string>())).ReturnsAsync(appSettings);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var filePickerMock = _fixture.Freeze<Mock<IFilePicker>>();
            filePickerMock.Setup(x => x.PickAsync(It.IsAny<PickOptions>())).ReturnsAsync(fileResult);

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                dialogServiceMock.Object,
                shellServiceMock.Object,
                filePickerMock.Object,
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());
            await sut.ImportSettingsAsync();

            sut.AnkiSoundsFolder.Should().Be(appSettings.AnkiSoundsFolder);
            sut.UseMp3gain.Should().Be(appSettings.UseMp3gain);
            sut.Mp3gainPath.Should().Be(appSettings.Mp3gainPath);
            sut.ShowCopyButtons.Should().Be(appSettings.ShowCopyButtons);
            sut.CopyTranslatedMeanings.Should().Be(appSettings.CopyTranslatedMeanings);

            settingsServiceMock.Verify(x => x.ImportSettingsAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Settings successfully imported."));
            shellServiceMock.VerifyNoOtherCalls();
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IFileSaver>(),
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IFileSaver>(),
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
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.MacCatalyst),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.CanUpdateIndividualSettings.Should().BeFalse();
        }

        #endregion

        #region Tests for OnShowCopyButtonsChangedInternal

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OnShowCopyButtonsChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnShowCopyButtonsChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetShowCopyButtons(value));
        }

        [TestMethod]
        public void OnShowCopyButtonsChangedInternal_WhenNotInitialized_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.OnShowCopyButtonsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetShowCopyButtons(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void OnShowCopyButtonsChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnShowCopyButtonsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetShowCopyButtons(It.IsAny<bool>()), Times.Never);
        }

        #endregion

        #region Tests for OnCopyTranslatedMeaningsChangedInternal

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OnCopyTranslatedMeaningsChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnCopyTranslatedMeaningsChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetCopyTranslatedMeanings(value));
        }

        [TestMethod]
        public void OnCopyTranslatedMeaningsChangedInternal_WhenNotInitialized_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.Android),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.OnCopyTranslatedMeaningsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetCopyTranslatedMeanings(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void OnCopyTranslatedMeaningsChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(x => x.Platform == DevicePlatform.WinUI),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>());

            sut.Init();
            sut.OnCopyTranslatedMeaningsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetCopyTranslatedMeanings(It.IsAny<bool>()), Times.Never);
        }

        #endregion
    }
}
