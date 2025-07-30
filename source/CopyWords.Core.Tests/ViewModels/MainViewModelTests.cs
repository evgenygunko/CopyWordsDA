using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for InitAsync

        [TestMethod]
        public async Task InitAsync_WhenIsBusyTrue_DoesNotRunLookup()
        {
            WordModel wordModel = _fixture.Create<WordModel>();

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = true;
            await sut.InitAsync();

            translationsServiceMock.VerifyNoOtherCalls();
            settingsServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task InitAsync_WhenInstantTranslationServiceHasText_SetsSearchWordAndRunsLookup()
        {
            WordModel wordModel = _fixture.Create<WordModel>();

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel).Verifiable();

            var instantTranslationServiceMock = _fixture.Freeze<Mock<IInstantTranslationService>>();
            instantTranslationServiceMock.Setup(x => x.GetTextAndClear()).Returns("abcdef");

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            await sut.InitAsync();

            sut.SearchWord.Should().Be("abcdef");
            translationsServiceMock.Verify();
        }

        [TestMethod]
        public async Task InitAsync_WhenInstantTranslationServiceDoesNotHaveText_DoesNotRunLookup()
        {
            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();

            var instantTranslationServiceMock = _fixture.Freeze<Mock<IInstantTranslationService>>();
            instantTranslationServiceMock.Setup(x => x.GetTextAndClear()).Returns((string?)null);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());

            var wordViewModelMock = _fixture.Freeze<Mock<IWordViewModel>>();

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.SearchWord = string.Empty;
            await sut.InitAsync();

            sut.SearchWord.Should().BeEmpty();
            translationsServiceMock.VerifyNoOtherCalls();
            wordViewModelMock.Verify(x => x.UpdateUI());
        }

        [DataTestMethod]
        [DataRow(SourceLanguage.Danish, "flag_of_denmark.png")]
        [DataRow(SourceLanguage.Spanish, "flag_of_spain.png")]
        public async Task InitAsync_Should_SetSelectedDictionary(SourceLanguage sourceLanguage, string imageName)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(sourceLanguage.ToString());

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            await sut.InitAsync();

            sut.DictionaryName.Should().Be(sourceLanguage.ToString());
            sut.DictionaryImage.Should().Be(imageName);
        }

        #endregion

        #region Tests for LookUpAsync

        [TestMethod]
        public async Task LookUpAsync_Should_SetWordViewModelProperty()
        {
            // Arrange
            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var wordViewModelMock = _fixture.Freeze<Mock<IWordViewModel>>();

            // Act
            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.SearchWord = search;

            await sut.LookUpAsync(It.IsAny<ITextInput>(), It.IsAny<CancellationToken>());

            // Assert
            sut.IsBusy.Should().BeFalse();

            wordViewModelMock.VerifySet(x => x.SoundUrl = wordModel.SoundUrl);
            wordViewModelMock.VerifySet(x => x.SoundFileName = wordModel.SoundFileName);

            wordViewModelMock.Verify(x => x.ClearDefinitions());
            wordViewModelMock.Verify(x => x.AddDefinition(It.IsAny<DefinitionViewModel>()), Times.Exactly(wordModel.Definitions.Count()));

            wordViewModelMock.Verify(x => x.ClearVariants());
            wordViewModelMock.Verify(x => x.AddVariant(It.IsAny<VariantViewModel>()), Times.Exactly(wordModel.Variations.Count()));
        }

        #endregion

        #region Tests for RefreshAsync

        [TestMethod]
        public async Task RefreshAsync_Should_CallLookUpWordAsync()
        {
            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel).Verifiable();

            var sut = _fixture.Create<MainViewModel>();
            sut.SearchWord = search;

            await sut.RefreshAsync();

            sut.IsRefreshing.Should().BeFalse();
            translationsServiceMock.Verify();
        }

        #endregion

        #region Tests for LookUpWordInDictionaryAsync

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task LookUpWordInDictionaryAsync_WhenSearchWordIsNullOrEmpty_ReturnsNull(string search)
        {
            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchIsInvalid_DisplaysAlerts()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>()))
                .ThrowsAsync(new InvalidInputException("too many commas"));

            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert("Search input is invalid", "too many commas", "OK"));
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenExceptionThrown_DisplaysAlerts()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ThrowsAsync(new Exception("exception from unit test"));

            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert("An error occurred while searching for translations", "exception from unit test", "OK"));
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchReturnsNull_DisplaysAlert()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync((WordModel?)null);

            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot find word", $"Could not find a translation for '{search}'", "OK"));
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchIsSuccessful_ReturnsViewModel()
        {
            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().Be(wordModel);
            dialogServiceMock.Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_Should_PassTranslatorAPIUrlToLookup()
        {
            string translatorApiUrl = _fixture.Create<Uri>().ToString();
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Danish.ToString();

            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorApiUrl).Returns(translatorApiUrl);

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            _ = await sut.LookUpWordInDictionaryAsync(search);

            translationsServiceMock.Verify(x => x.LookUpWordAsync(search, It.Is<Options>(opt => opt.TranslatorApiURL == translatorApiUrl)));
        }

        #endregion

        #region Tests for GetVariantAsync

        [TestMethod]
        public async Task GetVariantAsync_WhenExceptionThrown_DisplaysAlerts()
        {
            string url = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ThrowsAsync(new Exception("exception from unit test"));

            var sut = _fixture.Create<MainViewModel>();

            await sut.GetVariantAsync(url);

            dialogServiceMock.Verify(x => x.DisplayAlert("An error occurred while searching for translations", "exception from unit test", "OK"));
        }

        [TestMethod]
        public async Task GetVariantAsync_WhenSearchReturnsNull_DisplaysAlert()
        {
            string url = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync((WordModel?)null);

            var sut = _fixture.Create<MainViewModel>();

            await sut.GetVariantAsync(url);

            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot find word", $"Could not find a translation for '{url}'", "OK"));
        }

        [TestMethod]
        public async Task GetVariantAsync_WhenSearchIsSuccessful_UpdatesUI()
        {
            // Arrange
            string url = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();
            bool showCopyButtons = _fixture.Create<bool>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = SourceLanguage.Danish.ToString() });
            settingsServiceMock.Setup(x => x.GetShowCopyButtons()).Returns(showCopyButtons);

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var wordViewModelMock = _fixture.Freeze<Mock<IWordViewModel>>();

            // Act
            var sut = _fixture.Create<MainViewModel>();
            await sut.GetVariantAsync(url);

            // Assert
            sut.IsBusy.Should().BeFalse();

            wordViewModelMock.VerifySet(x => x.SoundUrl = wordModel.SoundUrl);
            wordViewModelMock.VerifySet(x => x.SoundFileName = wordModel.SoundFileName);
            wordViewModelMock.VerifySet(x => x.ShowCopyButtons = showCopyButtons);

            wordViewModelMock.Verify(x => x.ClearDefinitions());
            wordViewModelMock.Verify(x => x.AddDefinition(It.IsAny<DefinitionViewModel>()), Times.Exactly(wordModel.Definitions.Count()));

            wordViewModelMock.Verify(x => x.ClearVariants());
            wordViewModelMock.Verify(x => x.AddVariant(It.IsAny<VariantViewModel>()), Times.Exactly(wordModel.Variations.Count()));

            dialogServiceMock.Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Tests for SelectDictionaryAsync

        [DataTestMethod]
        [DataRow(SourceLanguage.Danish, "flag_of_denmark.png")]
        [DataRow(SourceLanguage.Spanish, "flag_of_spain.png")]
        public async Task SelectDictionaryAsync_WhenUserSelectsDictionary_UpdatesSelectedDictionary(SourceLanguage sourceLanguage, string imageName)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheet(
                    "Select dictionary:",
                    "Cancel",
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync(sourceLanguage.ToString())
                .Verifiable();

            var sut = _fixture.Create<MainViewModel>();
            await sut.SelectDictionaryAsync();

            sut.DictionaryName.Should().Be(sourceLanguage.ToString());
            sut.DictionaryImage.Should().Be(imageName);

            dialogServiceMock.Verify();
            settingsServiceMock.Verify(x => x.SetSelectedParser(sourceLanguage.ToString()));
        }

        [TestMethod]
        public async Task SelectDictionaryAsync_WhenUserClicksCancel_DoesNotUpdateSelectedDictionary()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock.Setup(x => x.DisplayActionSheet(
                    "Select dictionary:",
                    "Cancel",
                    It.IsAny<string>(),
                    It.IsAny<FlowDirection>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync((string)null!)
                .Verifiable();

            var sut = _fixture.Create<MainViewModel>();
            sut.DictionaryName = "DictionaryName";
            sut.DictionaryImage = "DictionaryImage";

            await sut.SelectDictionaryAsync();

            sut.DictionaryName.Should().Be("DictionaryName");
            sut.DictionaryImage.Should().Be("DictionaryImage");

            dialogServiceMock.Verify();
            settingsServiceMock.Verify(x => x.SetSelectedParser(It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
