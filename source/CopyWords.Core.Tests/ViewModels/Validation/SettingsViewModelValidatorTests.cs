// Ignore Spelling: Validator Ffmpeg Api

using AutoFixture;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Core.ViewModels.Validation;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace CopyWords.Core.Tests.ViewModels.Validation
{
    [TestClass]
    public class SettingsViewModelValidatorTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Validate_WhenAllRequiredFieldsHaveValues_ReturnsTrue()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = true;
            appSettings.TranslatorApiUrl = _fixture.Create<Uri>().ToString();

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeTrue(result.Errors.FirstOrDefault()?.ErrorMessage);
        }

        [TestMethod]
        public void Validate_WhenAnkiSoundsFolderIsEmpty_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = false;
            appSettings.AnkiSoundsFolder = string.Empty;

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'Anki Sounds Folder' must not be empty.");
        }

        [TestMethod]
        public void Validate_WhenFfmpegBinFolderIsEmpty_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = false;
            appSettings.FfmpegBinFolder = string.Empty;

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'Path to Ffmpeg' must not be empty.");
        }

        [TestMethod]
        public void Validate_WhenUseMPGainAndMp3gainPathIsEmpty_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = false;
            appSettings.UseMp3gain = true;
            appSettings.Mp3gainPath = string.Empty;

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'Path to mp3gain' must not be empty.");
        }

        [TestMethod]
        public void Validate_WhenUseTranslatorIsTrueAndTranslatorApiUrlIsNotAValidUrl_ReturnsFalse()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = true;
            appSettings.TranslatorApiUrl = "abcdef";

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("'TranslatorAPI URL' must be a valid URL.");
        }

        [TestMethod]
        public void Validate_WhenUseTranslatorIsFalseAndTranslatorApiUrlIsNotAValidUrl_ReturnsTrue()
        {
            var appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = false;
            appSettings.TranslatorApiUrl = "abcdef";

            SettingsViewModel settingsViewModel = CreateSettingsViewModel(appSettings);

            var sut = _fixture.Create<SettingsViewModelValidator>();
            ValidationResult result = sut.Validate(settingsViewModel);

            result.IsValid.Should().BeTrue(result.Errors.FirstOrDefault()?.ErrorMessage);
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
