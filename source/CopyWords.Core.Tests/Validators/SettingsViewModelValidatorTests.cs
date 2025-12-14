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
        public void Validate_WhenAllRequiredFieldsHaveValues_ReturnsTrue()
        {
            var appSettings = _fixture.Create<AppSettings>();

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeTrue(result.Errors.FirstOrDefault()?.ErrorMessage);
        }

        [TestMethod]
        public void Validate_WhenAnkiSoundsFolderIsEmpty_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiSoundsFolder = string.Empty;

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'Anki Sounds Folder' must not be empty.");
        }

        private SettingsViewModel CreateSettingsViewModel(AppSettings appSettings)
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
                Mock.Of<IValidator<SettingsViewModel>>());

            settingsViewModel.Init();

            return settingsViewModel;
        }
    }
}
