// Ignore Spelling: Validator Api

using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.Validators;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace CopyWords.Core.Tests.Validators
{
    [TestClass]
    public class SettingsViewModelValidatorTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public async Task Validate_WhenAllRequiredFieldsHaveValues_ReturnsTrue()
        {
            var appSettings = _fixture.Create<AppSettings>();

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(true);

            SettingsViewModel settingsViewModel = await CreateSettingsViewModelAsync(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeTrue(result.Errors.FirstOrDefault()?.ErrorMessage);
        }

        [TestMethod]
        public async Task Validate_WhenAnkiSoundsFolderIsEmpty_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = string.Empty;

            SettingsViewModel settingsViewModel = await CreateSettingsViewModelAsync(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("Path to Anki media collection cannot be empty");
        }

        [TestMethod]
        public async Task Validate_WhenAnkiSoundsFolderIsNull_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = null!;

            SettingsViewModel settingsViewModel = await CreateSettingsViewModelAsync(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("Path to Anki media collection cannot be empty");
        }

        [TestMethod]
        public async Task Validate_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = @"C:\NonExistentDirectory";

            Mock<IFileIOService> fileIOServiceMock = _fixture.Freeze<Mock<IFileIOService>>();
            fileIOServiceMock.Setup(x => x.DirectoryExists(appSettings.AnkiSoundsFolder)).Returns(false);

            SettingsViewModel settingsViewModel = await CreateSettingsViewModelAsync(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("The specified directory does not exist");
        }

        private async Task<SettingsViewModel> CreateSettingsViewModelAsync(AppSettings appSettings)
        {
            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var settingsViewModel = new SettingsViewModel(
                settingsServiceMock.Object,
                Mock.Of<IDialogService>(),
                Mock.Of<IShellService>(),
                Mock.Of<IFilePicker>(),
                Mock.Of<IDeviceInfo>(),
                Mock.Of<IFileSaver>(),
                Mock.Of<IValidator<SettingsViewModel>>(),
                Mock.Of<IAnkiConnectService>());

            await settingsViewModel.InitAsync(CancellationToken.None);

            return settingsViewModel;
        }
    }
}
