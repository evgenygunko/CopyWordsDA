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

        #region Properties

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CanNavigateBack_Should_CallNavigationHistory(bool value)
        {
            var navigationHistoryMock = _fixture.Freeze<Mock<INavigationHistory>>();
            navigationHistoryMock.SetupGet(x => x.CanNavigateBack).Returns(value);

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.IsRefreshing = false;
            sut.SearchWord = "test";
            sut.CanNavigateBack.Should().Be(value);

            navigationHistoryMock.Verify(x => x.CanNavigateBack);
        }

        #endregion

        #region #region Commands

        #region Tests for InitAsync

        [TestMethod]
        public async Task InitAsync_WhenIsBusyTrue_DoesNotRunLookup()
        {
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
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel)
                .Verifiable();

            var instantTranslationServiceMock = _fixture.Freeze<Mock<IInstantTranslationService>>();
            instantTranslationServiceMock.Setup(x => x.GetTextAndClear()).Returns("abcdef");

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));

            var wordViewModelMock = _fixture.Freeze<Mock<IWordViewModel>>();

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.SearchWord = string.Empty;
            await sut.InitAsync();

            sut.SearchWord.Should().BeEmpty();
            translationsServiceMock.VerifyNoOtherCalls();
            wordViewModelMock.Verify(x => x.UpdateUI());
        }

        [TestMethod]
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
            WordModel wordModel = _fixture.Create<WordModel>() with { SourceLanguage = SourceLanguage.Danish };

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel);

            var navigationHistoryMock = _fixture.Freeze<Mock<INavigationHistory>>();
            var wordViewModelMock = _fixture.Freeze<Mock<IWordViewModel>>();

            // Act
            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.SearchWord = search;

            await sut.LookUpAsync();

            // Assert
            sut.IsBusy.Should().BeFalse();

            wordViewModelMock.VerifySet(x => x.SoundUrl = wordModel.SoundUrl);
            wordViewModelMock.VerifySet(x => x.SoundFileName = wordModel.SoundFileName);

            wordViewModelMock.Verify(x => x.ClearDefinitions());
            wordViewModelMock.Verify(x => x.AddDefinition(It.IsAny<DefinitionViewModel>()), Times.Exactly(wordModel.Definitions.Count()));

            wordViewModelMock.Verify(x => x.ClearVariants());
            wordViewModelMock.Verify(x => x.AddVariant(It.IsAny<VariantViewModel>()), Times.Exactly(wordModel.Variations.Count()));

            navigationHistoryMock.Verify(x => x.Push(wordModel.Word, wordModel.SourceLanguage.ToString()));
            settingsServiceMock.Verify(x => x.AddToHistory(wordModel.Word));
        }

        #endregion

        #region Tests for RefreshAsync

        [TestMethod]
        public async Task RefreshAsync_Should_CallLookUpWordAsync()
        {
            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel)
                .Verifiable();

            var sut = _fixture.Create<MainViewModel>();
            sut.SearchWord = search;

            await sut.RefreshAsync();

            sut.IsRefreshing.Should().BeFalse();
            translationsServiceMock.Verify();
        }

        #endregion

        #region Tests for LookUpWordInDictionaryAsync

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task LookUpWordInDictionaryAsync_WhenSearchWordIsNullOrEmpty_ReturnsNull(string search)
        {
            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenTaskCanceledException_ReturnsNull()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("exception from unit test"));

            var sut = _fixture.Create<MainViewModel>();

            WordModel? result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchIsInvalid_DisplaysAlerts()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("exception from unit test"));

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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WordModel?)null);

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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel);

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
            appSettings.SelectedParser = nameof(SourceLanguage.Danish);

            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<IGlobalSettings> globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            globalSettingsMock.Setup(x => x.TranslatorApiUrl).Returns(translatorApiUrl);

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            _ = await sut.LookUpWordInDictionaryAsync(search);

            translationsServiceMock.Verify(x => x.LookUpWordAsync(search, It.Is<Options>(opt => opt.TranslatorApiURL == translatorApiUrl), It.IsAny<CancellationToken>()));
        }

        #endregion

        #region Tests for GetVariantAsync

        [TestMethod]
        public async Task GetVariantAsync_WhenExceptionThrown_DisplaysAlerts()
        {
            string url = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("exception from unit test"));

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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WordModel?)null);

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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });
            settingsServiceMock.Setup(x => x.GetShowCopyButtons()).Returns(showCopyButtons);

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel);

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

        [TestMethod]
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

        #region Tests for GetSuggestionsAsync

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task GetSuggestionsAsync_WhenInputTextIsNullOrWhiteSpace_ReturnsEmptyList(string inputText)
        {
            // Arrange
            var sut = _fixture.Create<MainViewModel>();

            // Act
            var result = await sut.GetSuggestionsAsync(inputText, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsDanish_CallsDanishSuggestionsService()
        {
            // Arrange
            string inputText = _fixture.Create<string>();
            var danishSuggestions = new[] { "haj", "hus", "have" };

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));

            var suggestionsServiceMock = _fixture.Freeze<Mock<ISuggestionsService>>();
            suggestionsServiceMock.Setup(x => x.GetDanishWordsSuggestionsAsync(inputText, It.IsAny<CancellationToken>()))
                .ReturnsAsync(danishSuggestions)
                .Verifiable();

            var sut = _fixture.Create<MainViewModel>();

            // Act
            var result = await sut.GetSuggestionsAsync(inputText, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(danishSuggestions);
            suggestionsServiceMock.Verify();
            suggestionsServiceMock.Verify(x => x.GetSpanishWordsSuggestionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsSpanish_CallsSpanishSuggestionsService()
        {
            // Arrange
            string inputText = _fixture.Create<string>();
            var spanishSuggestions = new[] { "hola", "casa", "perro" };

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Spanish));

            var suggestionsServiceMock = _fixture.Freeze<Mock<ISuggestionsService>>();
            suggestionsServiceMock.Setup(x => x.GetSpanishWordsSuggestionsAsync(inputText, It.IsAny<CancellationToken>()))
                .ReturnsAsync(spanishSuggestions)
                .Verifiable();

            var sut = _fixture.Create<MainViewModel>();

            // Act
            var result = await sut.GetSuggestionsAsync(inputText, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(spanishSuggestions);
            suggestionsServiceMock.Verify();
            suggestionsServiceMock.Verify(x => x.GetDanishWordsSuggestionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSelectedParserIsInvalid_ReturnsEmptyList()
        {
            // Arrange
            string inputText = _fixture.Create<string>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns("InvalidLanguage");

            var suggestionsServiceMock = _fixture.Freeze<Mock<ISuggestionsService>>();

            var sut = _fixture.Create<MainViewModel>();

            // Act
            var result = await sut.GetSuggestionsAsync(inputText, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            suggestionsServiceMock.Verify(x => x.GetDanishWordsSuggestionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            suggestionsServiceMock.Verify(x => x.GetSpanishWordsSuggestionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenCancellationTokenIsCancelled_PassesCancellationToken()
        {
            // Arrange
            string inputText = _fixture.Create<string>();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));

            var tokens = new List<CancellationToken>();

            var suggestionsServiceMock = _fixture.Freeze<Mock<ISuggestionsService>>();
            suggestionsServiceMock.Setup(x => x.GetDanishWordsSuggestionsAsync(inputText, Capture.In(tokens)))
                .ReturnsAsync(new[] { "test" })
                .Verifiable();

            var sut = _fixture.Create<MainViewModel>();

            // Act
            var result = await sut.GetSuggestionsAsync(inputText, cancellationTokenSource.Token);

            // Assert
            var token = tokens.Single();
            token.IsCancellationRequested.Should().BeTrue();
            suggestionsServiceMock.Verify();
        }

        [TestMethod]
        public async Task GetSuggestionsAsync_WhenSuggestionsServiceReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            string inputText = _fixture.Create<string>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));

            var suggestionsServiceMock = _fixture.Freeze<Mock<ISuggestionsService>>();
            suggestionsServiceMock.Setup(x => x.GetDanishWordsSuggestionsAsync(inputText, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<string>());

            var sut = _fixture.Create<MainViewModel>();

            // Act
            var result = await sut.GetSuggestionsAsync(inputText, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #endregion

        #region Public Methods

        [TestMethod]
        public async Task NavigateBackAsync_WhenCanNavigateBackIsFalse_ReturnsFalse()
        {
            var sut = _fixture.Create<MainViewModel>();
            bool result = await sut.NavigateBackAsync();

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task NavigateBackAsync_WhenDoesNotHaveItemDifferentFromSearchWord_ReturnsFalse()
        {
            var navigationHistoryMock = _fixture.Freeze<Mock<INavigationHistory>>();
            navigationHistoryMock.SetupGet(x => x.CanNavigateBack).Returns(true);
            navigationHistoryMock.Setup(x => x.Pop()).Returns(new NavigationEntry("test1", "dict1"));
            navigationHistoryMock.SetupSequence(x => x.Count)
                .Returns(1)
                .Returns(0);

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.IsRefreshing = false;
            sut.SearchWord = "test1";

            bool result = await sut.NavigateBackAsync();

            result.Should().BeFalse();
            navigationHistoryMock.Verify(x => x.Pop(), Times.Exactly(1));
        }

        [TestMethod]
        public async Task NavigateBackAsync_WhenPreviousItemDifferentFromSearchWord_ReturnsTrue()
        {
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel);

            var navigationHistoryMock = _fixture.Freeze<Mock<INavigationHistory>>();
            navigationHistoryMock.SetupGet(x => x.CanNavigateBack).Returns(true);
            navigationHistoryMock.SetupSequence(x => x.Pop())
                .Returns(new NavigationEntry("test2", "dict1"))
                .Returns(new NavigationEntry("test1", "dict1"));
            navigationHistoryMock.SetupSequence(x => x.Count)
                .Returns(2)
                .Returns(1);

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.IsRefreshing = false;
            sut.SearchWord = "test2";

            bool result = await sut.NavigateBackAsync();

            result.Should().BeTrue();
            sut.SearchWord.Should().Be("test1");

            navigationHistoryMock.Verify(x => x.Pop(), Times.Exactly(2));
            translationsServiceMock.Verify(x => x.LookUpWordAsync("test1", It.IsAny<Options>(), It.IsAny<CancellationToken>()));
            settingsServiceMock.Verify(x => x.SetSelectedParser("dict1"));
        }

        [TestMethod]
        public async Task NavigateBackAsync_WhenCurrentItemDifferentFromSearchWord_ReturnsTrue()
        {
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(new AppSettings { SelectedParser = nameof(SourceLanguage.Danish) });

            var translationsServiceMock = _fixture.Freeze<Mock<ITranslationsService>>();
            translationsServiceMock
                .Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(wordModel);

            var navigationHistoryMock = _fixture.Freeze<Mock<INavigationHistory>>();
            navigationHistoryMock.SetupGet(x => x.CanNavigateBack).Returns(true);
            navigationHistoryMock.Setup(x => x.Pop()).Returns(new NavigationEntry("test1", "dict1"));
            navigationHistoryMock.SetupSequence(x => x.Count)
                .Returns(1)
                .Returns(0);

            var sut = _fixture.Create<MainViewModel>();
            sut.IsBusy = false;
            sut.IsRefreshing = false;
            sut.SearchWord = "test2";

            bool result = await sut.NavigateBackAsync();

            result.Should().BeTrue();
            sut.SearchWord.Should().Be("test1");

            translationsServiceMock.Verify(x => x.LookUpWordAsync("test1", It.IsAny<Options>(), It.IsAny<CancellationToken>()));
            translationsServiceMock.Verify(x => x.LookUpWordAsync("test1", It.IsAny<Options>(), It.IsAny<CancellationToken>()));
            settingsServiceMock.Verify(x => x.SetSelectedParser("dict1"));
        }

        #endregion

        #region Internal Methods

        #region Tests for UpdateUI

        [TestMethod]
        [DataRow(SourceLanguage.Danish)]
        [DataRow(SourceLanguage.Spanish)]
        public void UpdateUI_UpdatesDictionary_WithSourceLanguageFromReturnedModel(SourceLanguage sourceLanguageInModel)
        {
            WordModel wordModel = _fixture.Create<WordModel>();
            wordModel = wordModel with { SourceLanguage = sourceLanguageInModel };
            wordModel.SourceLanguage.Should().Be(sourceLanguageInModel);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();

            var sut = _fixture.Create<MainViewModel>();
            sut.UpdateUI(wordModel);

            sut.DictionaryName.Should().Be(sourceLanguageInModel.ToString());
            settingsServiceMock.Verify(x => x.SetSelectedParser(sourceLanguageInModel.ToString()));
        }

        #endregion

        #endregion
    }
}
