// Ignore Spelling: Api

using System.Runtime.Versioning;
using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
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

        #region Tests for SaveSettingsAsync

        [TestMethod]
        public async Task SaveSettingsAsync_Should_CallSettingsService()
        {
            string validationErrors = _fixture.Create<string>();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            Mock<IShellService> shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = _fixture.Create<SettingsViewModel>();

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

            var sut = _fixture.Create<SettingsViewModel>();

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

            var sut = _fixture.Create<SettingsViewModel>();

            bool result = sut.CanSaveSettings();

            result.Should().BeFalse();
        }

        #endregion

        #region Tests for SelectAnkiDeckNameDanishAsync

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameDanishAsync_WhenDecksAreAvailableAndUserSelectsDeck_SetsAnkiDeckNameDanish()
        {
            // Arrange
            var deckNames = new List<string> { "Default", "Japanese", "English" };
            string selectedDeckName = "Japanese";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deckNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(selectedDeckName);

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiDeckNameDanishAsync(CancellationToken.None);

            // Assert
            sut.AnkiDeckNameDanish.Should().Be(selectedDeckName);
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayActionSheetAsync(
                "Select deck:",
                "Cancel",
                null!,
                FlowDirection.LeftToRight,
                It.Is<string[]>(arr => arr.SequenceEqual(deckNames))), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameDanishAsync_WhenDecksAreAvailableAndUserCancels_DoesNotChangeAnkiDeckNameDanish()
        {
            // Arrange
            var deckNames = new List<string> { "Default", "Japanese", "English" };
            string originalDeckName = "Original";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deckNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync("Cancel");

            var sut = _fixture.Create<SettingsViewModel>();
            sut.AnkiDeckNameDanish = originalDeckName;

            // Act
            await sut.SelectAnkiDeckNameDanishAsync(CancellationToken.None);

            // Assert
            sut.AnkiDeckNameDanish.Should().Be(originalDeckName);
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameDanishAsync_WhenDecksAreAvailableAndUserSelectsEmptyResult_DoesNotChangeAnkiDeckNameDanish()
        {
            // Arrange
            var deckNames = new List<string> { "Default", "Japanese", "English" };
            string originalDeckName = "Original";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deckNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(string.Empty);

            var sut = _fixture.Create<SettingsViewModel>();
            sut.AnkiDeckNameDanish = originalDeckName;

            // Act
            await sut.SelectAnkiDeckNameDanishAsync(CancellationToken.None);

            // Assert
            sut.AnkiDeckNameDanish.Should().Be(originalDeckName);
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameDanishAsync_WhenNoDecksAreAvailable_ShowsAlert()
        {
            // Arrange
            var deckNames = new List<string>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deckNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiDeckNameDanishAsync(CancellationToken.None);

            // Assert
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Fetching deck names",
                "Cannot get deck names from AnkiConnect.",
                "OK"), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayActionSheetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FlowDirection>(),
                It.IsAny<string[]>()), Times.Never);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameDanishAsync_WhenExceptionIsThrown_ShowsErrorAlert()
        {
            // Arrange
            string exceptionMessage = "AnkiConnect is not running";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiDeckNameDanishAsync(CancellationToken.None);

            // Assert
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot select deck",
                $"Error occurred while trying to select deck name: {exceptionMessage}",
                "OK"), Times.Once);
        }

        #endregion

        #region Tests for SelectAnkiDeckNameSpanishAsync

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameSpanishAsync_WhenDecksAreAvailableAndUserSelectsDeck_SetsAnkiDeckNameSpanish()
        {
            // Arrange
            var deckNames = new List<string> { "Default", "Japanese", "Spanish" };
            string selectedDeckName = "Spanish";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deckNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(selectedDeckName);

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiDeckNameSpanishAsync(CancellationToken.None);

            // Assert
            sut.AnkiDeckNameSpanish.Should().Be(selectedDeckName);
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayActionSheetAsync(
                "Select deck:",
                "Cancel",
                null!,
                FlowDirection.LeftToRight,
                It.Is<string[]>(arr => arr.SequenceEqual(deckNames))), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiDeckNameSpanishAsync_WhenDecksAreAvailableAndUserCancels_DoesNotChangeAnkiDeckNameSpanish()
        {
            // Arrange
            var deckNames = new List<string> { "Default", "Japanese", "Spanish" };
            string originalDeckName = "Original";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deckNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync("Cancel");

            var sut = _fixture.Create<SettingsViewModel>();
            sut.AnkiDeckNameSpanish = originalDeckName;

            // Act
            await sut.SelectAnkiDeckNameSpanishAsync(CancellationToken.None);

            // Assert
            sut.AnkiDeckNameSpanish.Should().Be(originalDeckName);
            ankiConnectServiceMock.Verify(x => x.GetDeckNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Tests for SelectAnkiModelNameAsync

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiModelNameAsync_WhenModelsAreAvailableAndUserSelectsModel_SetsAnkiModelName()
        {
            // Arrange
            var modelNames = new List<string> { "Basic", "Basic (and reversed card)", "Cloze" };
            string selectedModelName = "Basic (and reversed card)";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(modelNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(selectedModelName);

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiModelNameAsync(CancellationToken.None);

            // Assert
            sut.AnkiModelName.Should().Be(selectedModelName);
            ankiConnectServiceMock.Verify(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayActionSheetAsync(
                "Select model:",
                "Cancel",
                null!,
                FlowDirection.LeftToRight,
                It.Is<string[]>(arr => arr.SequenceEqual(modelNames))), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiModelNameAsync_WhenModelsAreAvailableAndUserCancels_DoesNotChangeAnkiModelName()
        {
            // Arrange
            var modelNames = new List<string> { "Basic", "Basic (and reversed card)", "Cloze" };
            string originalModelName = "Original";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(modelNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync("Cancel");

            var sut = _fixture.Create<SettingsViewModel>();
            sut.AnkiModelName = originalModelName;

            // Act
            await sut.SelectAnkiModelNameAsync(CancellationToken.None);

            // Assert
            sut.AnkiModelName.Should().Be(originalModelName);
            ankiConnectServiceMock.Verify(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiModelNameAsync_WhenModelsAreAvailableAndUserSelectsEmptyResult_DoesNotChangeAnkiModelName()
        {
            // Arrange
            var modelNames = new List<string> { "Basic", "Basic (and reversed card)", "Cloze" };
            string originalModelName = "Original";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(modelNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(string.Empty);

            var sut = _fixture.Create<SettingsViewModel>();
            sut.AnkiModelName = originalModelName;

            // Act
            await sut.SelectAnkiModelNameAsync(CancellationToken.None);

            // Assert
            sut.AnkiModelName.Should().Be(originalModelName);
            ankiConnectServiceMock.Verify(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiModelNameAsync_WhenNoModelsAreAvailable_ShowsAlert()
        {
            // Arrange
            var modelNames = new List<string>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(modelNames);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiModelNameAsync(CancellationToken.None);

            // Assert
            ankiConnectServiceMock.Verify(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Fetching model names",
                "Cannot get model names from AnkiConnect.",
                "OK"), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayActionSheetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FlowDirection>(),
                It.IsAny<string[]>()), Times.Never);
        }

        [SupportedOSPlatform("windows")]
        [TestMethod]
        public async Task SelectAnkiModelNameAsync_WhenExceptionIsThrown_ShowsErrorAlert()
        {
            // Arrange
            string exceptionMessage = "AnkiConnect is not running";

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.SelectAnkiModelNameAsync(CancellationToken.None);

            // Assert
            ankiConnectServiceMock.Verify(x => x.GetModelNamesAsync(It.IsAny<CancellationToken>()), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot select model",
                $"Error occurred while trying to select model name: {exceptionMessage}",
                "OK"), Times.Once);
        }

        #endregion

        #region Tests for InitAsync

        [TestMethod]
        public async Task InitAsync_Should_SetAllPropertiesFromAppSettings()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = _fixture.Create<string>(); // Ensure it's not empty

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitAsync(CancellationToken.None);

            // Assert
            settingsServiceMock.Verify(x => x.LoadSettings());

            sut.AnkiDeckNameDanish.Should().Be(appSettings.AnkiDeckNameDanish);
            sut.AnkiDeckNameSpanish.Should().Be(appSettings.AnkiDeckNameSpanish);
            sut.AnkiModelName.Should().Be(appSettings.AnkiModelName);
            sut.AnkiSoundsFolder.Should().Be(appSettings.AnkiSoundsFolder);
            sut.ShowCopyButtons.Should().Be(appSettings.ShowCopyButtons);
            sut.ShowAnkiButton.Should().Be(appSettings.ShowAnkiButton);
            sut.CopyTranslatedMeanings.Should().Be(appSettings.CopyTranslatedMeanings);
        }

        [TestMethod]
        public async Task InitAsync_WhenAnkiSoundsFolderIsEmptyAndPlatformIsWinUI_CallsGetAnkiMediaDirectoryPathAsync()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = string.Empty;
            string expectedMediaPath = "C:\\Users\\User\\AppData\\Roaming\\Anki2\\User 1\\collection.media";

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.GetAnkiMediaDirectoryPathAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedMediaPath);

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitAsync(CancellationToken.None);

            // Assert
            sut.AnkiSoundsFolder.Should().Be(expectedMediaPath);
            ankiConnectServiceMock.Verify(x => x.GetAnkiMediaDirectoryPathAsync(CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task InitAsync_WhenAnkiSoundsFolderIsEmptyAndPlatformIsAndroid_UsesEmptyValue()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = string.Empty;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitAsync(CancellationToken.None);

            // Assert
            sut.AnkiSoundsFolder.Should().Be(string.Empty);
            ankiConnectServiceMock.Verify(x => x.GetAnkiMediaDirectoryPathAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Tests for CanUpdateIndividualSettings

        [TestMethod]
        public void CanUpdateIndividualSettings_OnAndroid_ReturnsTrue()
        {
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.CanUpdateIndividualSettings.Should().BeTrue();
        }

        [TestMethod]
        public void CanUpdateIndividualSettings_OnWindows_ReturnsFalse()
        {
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.CanUpdateIndividualSettings.Should().BeFalse();
        }

        [TestMethod]
        public void CanUpdateIndividualSettings_OnMacOS_ReturnsFalse()
        {
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.MacCatalyst);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.CanUpdateIndividualSettings.Should().BeFalse();
        }

        #endregion

        #region Tests for OnShowCopyButtonsChangedInternal

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task OnShowCopyButtonsChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnShowCopyButtonsChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetShowCopyButtons(value));
        }

        [TestMethod]
        public void OnShowCopyButtonsChangedInternal_WhenNotInitialized_DoesNotCallSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.OnShowCopyButtonsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetShowCopyButtons(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task OnShowCopyButtonsChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnShowCopyButtonsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetShowCopyButtons(It.IsAny<bool>()), Times.Never);
        }

        #endregion

        #region Tests for OnCopyTranslatedMeaningsChangedInternal

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task OnCopyTranslatedMeaningsChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnCopyTranslatedMeaningsChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetCopyTranslatedMeanings(value));
        }

        [TestMethod]
        public void OnCopyTranslatedMeaningsChangedInternal_WhenNotInitialized_DoesNotCallSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.OnCopyTranslatedMeaningsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetCopyTranslatedMeanings(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task OnCopyTranslatedMeaningsChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnCopyTranslatedMeaningsChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetCopyTranslatedMeanings(It.IsAny<bool>()), Times.Never);
        }

        #endregion

        #region Tests for OnUseDarkThemeChangedInternal

        [TestMethod]
        [DataRow(true, AppTheme.Dark)]
        [DataRow(false, AppTheme.Light)]
        public async Task OnUseDarkThemeChangedInternal_WhenInitialized_CallsApplyTheme(bool value, AppTheme expectedTheme)
        {
            var appThemeServiceMock = _fixture.Freeze<Mock<IAppThemeService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnUseDarkThemeChangedInternal(value);

            appThemeServiceMock.Verify(x => x.ApplyTheme(expectedTheme), Times.Once);
        }

        [TestMethod]
        public void OnUseDarkThemeChangedInternal_WhenNotInitialized_DoesNotCallApplyTheme()
        {
            var appThemeServiceMock = _fixture.Freeze<Mock<IAppThemeService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.OnUseDarkThemeChangedInternal(true);

            appThemeServiceMock.Verify(x => x.ApplyTheme(It.IsAny<AppTheme>()), Times.Never);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task OnUseDarkThemeChangedInternal_WhenInitializedAndCanUpdateIndividualSettings_CallsSettingsService(bool value)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnUseDarkThemeChangedInternal(value);

            settingsServiceMock.Verify(x => x.SetUseDarkTheme(value));
        }

        [TestMethod]
        public void OnUseDarkThemeChangedInternal_WhenNotInitialized_DoesNotCallSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var sut = _fixture.Create<SettingsViewModel>();

            sut.OnUseDarkThemeChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetUseDarkTheme(It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public async Task OnUseDarkThemeChangedInternal_WhenCannotUpdateIndividualSettings_DoesNotCallSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _fixture.Freeze<Mock<IDeviceInfo>>().Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = _fixture.Create<SettingsViewModel>();

            await sut.InitAsync(CancellationToken.None);
            sut.OnUseDarkThemeChangedInternal(true);

            settingsServiceMock.Verify(x => x.SetUseDarkTheme(It.IsAny<bool>()), Times.Never);
        }

        #endregion

        #region Tests for CancelAsync

        [TestMethod]
        [DataRow(true, AppTheme.Dark)]
        [DataRow(false, AppTheme.Light)]
        public async Task CancelAsync_RestoresThemeFromSettings(bool useDarkTheme, AppTheme expectedTheme)
        {
            // Arrange
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetUseDarkTheme()).Returns(useDarkTheme);

            var appThemeServiceMock = _fixture.Freeze<Mock<IAppThemeService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.CancelAsync();

            // Assert
            settingsServiceMock.Verify(x => x.GetUseDarkTheme(), Times.Once);
            appThemeServiceMock.Verify(x => x.ApplyTheme(expectedTheme), Times.Once);
        }

        [TestMethod]
        public async Task CancelAsync_NavigatesBack()
        {
            // Arrange
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.CancelAsync();

            // Assert
            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(st => st.Location.ToString() == "..")), Times.Once);
        }

        #endregion

        #region Tests for InitializeAnkiDroidAsync

        [TestMethod]
        public async Task InitializeAnkiDroidAsync_WhenAnkiDroidIsAvailable_SetsIsAnkiIntegrationAvailableToTrue()
        {
            // Arrange
            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            ankiDroidServiceMock.Setup(x => x.GetDeckNames())
                .Returns(new List<string>());
            ankiDroidServiceMock.Setup(x => x.GetModelNames())
                .Returns(new List<string>());

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitializeAnkiDroidAsync();

            // Assert
            sut.IsAnkiIntegrationAvailable.Should().BeTrue();
            ankiDroidServiceMock.Verify(x => x.IsAvailable(), Times.Once);
        }

        [TestMethod]
        public async Task InitializeAnkiDroidAsync_WhenAnkiDroidIsNotAvailable_SetsIsAnkiIntegrationAvailableToFalse()
        {
            // Arrange
            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(false);
            ankiDroidServiceMock.Setup(x => x.GetDeckNames())
                .Returns(new List<string>());
            ankiDroidServiceMock.Setup(x => x.GetModelNames())
                .Returns(new List<string>());

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitializeAnkiDroidAsync();

            // Assert
            sut.IsAnkiIntegrationAvailable.Should().BeFalse();
            ankiDroidServiceMock.Verify(x => x.IsAvailable(), Times.Once);
            ankiDroidServiceMock.Verify(x => x.GetDeckNames(), Times.Never);
            ankiDroidServiceMock.Verify(x => x.GetModelNames(), Times.Never);
        }

        [TestMethod]
        public async Task InitializeAnkiDroidAsync_PopulatesDeckNamesFromService()
        {
            // Arrange
            var expectedDeckNames = new List<string> { "Default", "Japanese", "Spanish" };

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            ankiDroidServiceMock.Setup(x => x.HasPermission()).Returns(true);
            ankiDroidServiceMock.Setup(x => x.GetDeckNames())
                .Returns(expectedDeckNames);
            ankiDroidServiceMock.Setup(x => x.GetModelNames())
                .Returns(new List<string>());

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitializeAnkiDroidAsync();

            // Assert
            sut.DeckNames.Should().BeEquivalentTo(expectedDeckNames);
            ankiDroidServiceMock.Verify(x => x.GetDeckNames(), Times.Once);
        }

        [TestMethod]
        public async Task InitializeAnkiDroidAsync_PopulatesModelNamesFromService()
        {
            // Arrange
            var expectedModelNames = new List<string> { "Basic", "Basic (and reversed card)", "Cloze" };

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            ankiDroidServiceMock.Setup(x => x.HasPermission()).Returns(true);
            ankiDroidServiceMock.Setup(x => x.GetDeckNames())
                .Returns(new List<string>());
            ankiDroidServiceMock.Setup(x => x.GetModelNames())
                .Returns(expectedModelNames);

            var sut = _fixture.Create<SettingsViewModel>();

            // Act
            await sut.InitializeAnkiDroidAsync();

            // Assert
            sut.ModelNames.Should().BeEquivalentTo(expectedModelNames);
            ankiDroidServiceMock.Verify(x => x.GetModelNames(), Times.Once);
        }

        #endregion
    }
}
