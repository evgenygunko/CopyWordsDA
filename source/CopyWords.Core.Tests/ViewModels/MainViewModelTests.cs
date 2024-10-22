using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for LookUpAsync

        [TestMethod]
        public async Task LookUpAsync_Should_SetWordViewModelProperty()
        {
            string search = _fixture.Create<string>();

            WordModel wordModel = _fixture.Create<WordModel>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.CheckThatWordIsValid(It.IsAny<string>())).Returns(() => (true, null));
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();
            sut.SearchWord = search;

            await sut.LookUpAsync();

            sut.IsBusy.Should().BeFalse();

            sut.WordViewModel.Should().NotBeNull();
            sut.WordViewModel.SoundUrl.Should().Be(wordModel.SoundUrl);
            sut.WordViewModel.SoundFileName.Should().Be(wordModel.SoundFileName);

            sut.WordViewModel.DefinitionViewModels.Should().HaveCount(wordModel.Definitions.Count());
            sut.WordViewModel.Variants.Should().HaveCount(wordModel.Variations.Count());
        }

        #endregion

        #region Tests for LookUpWordInDictionaryAsync

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task LookUpWordInDictionaryAsync_WhenSearchWordIsNullOrEmpty_ReturnsNull(string search)
        {
            var sut = _fixture.Create<MainViewModel>();

            WordModel result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchIsInvalid_DisplaysAlerts()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.CheckThatWordIsValid(It.IsAny<string>())).Returns(() => (false, "too many commas"));

            var sut = _fixture.Create<MainViewModel>();

            WordModel result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert("Invalid search term", "too many commas", "OK"));
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenExceptionThrown_DisplaysAlerts()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.CheckThatWordIsValid(It.IsAny<string>())).Returns(() => (true, null));
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ThrowsAsync(new Exception("exception from unit test"));

            var sut = _fixture.Create<MainViewModel>();

            WordModel result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert("Error occurred while searching translations", "exception from unit test", "OK"));
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchReturnsNull_DisplaysAlert()
        {
            string search = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.CheckThatWordIsValid(It.IsAny<string>())).Returns(() => (true, null));
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync((WordModel?)null);

            var sut = _fixture.Create<MainViewModel>();

            WordModel result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().BeNull();
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot find word", $"Could not find a translation for '{search}'", "OK"));
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_WhenSearchIsSuccessful_ReturnsViewModel()
        {
            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.CheckThatWordIsValid(It.IsAny<string>())).Returns(() => (true, null));
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            WordModel result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().Be(wordModel);
            dialogServiceMock.Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task LookUpWordInDictionaryAsync_Should_PassTranslatorAPIUrlToLookup()
        {
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.UseTranslator = true;

            string search = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.CheckThatWordIsValid(It.IsAny<string>())).Returns(() => (true, null));
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            _ = await sut.LookUpWordInDictionaryAsync(search);

            lookUpWordMock.Verify(x => x.LookUpWordAsync(search, It.Is<Options>(opt => opt.TranslatorApiURL == appSettings.TranslatorApiUrl)));
        }

        #endregion

        #region Tests for GetVariantAsync

        [TestMethod]
        public async Task GetVariantAsync_WhenExceptionThrown_DisplaysAlerts()
        {
            string url = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.GetWordByUrlAsync(It.IsAny<string>(), It.IsAny<Options>())).ThrowsAsync(new Exception("exception from unit test"));

            var sut = _fixture.Create<MainViewModel>();

            await sut.GetVariantAsync(url);

            dialogServiceMock.Verify(x => x.DisplayAlert("Error occurred while parsing the word", "exception from unit test", "OK"));
        }

        [TestMethod]
        public async Task GetVariantAsync_WhenSearchReturnsNull_DisplaysAlert()
        {
            string url = _fixture.Create<string>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.GetWordByUrlAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync((WordModel?)null);

            var sut = _fixture.Create<MainViewModel>();

            await sut.GetVariantAsync(url);

            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot find word", $"Could not parse the word by URL '{url}'", "OK"));
        }

        [TestMethod]
        public async Task GetVariantAsync_WhenSearchIsSuccessful_UpdatesUI()
        {
            string url = _fixture.Create<string>();
            WordModel wordModel = _fixture.Create<WordModel>();

            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            Mock<ILookUpWord> lookUpWordMock = _fixture.Freeze<Mock<ILookUpWord>>();
            lookUpWordMock.Setup(x => x.GetWordByUrlAsync(It.IsAny<string>(), It.IsAny<Options>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            await sut.GetVariantAsync(url);

            sut.IsBusy.Should().BeFalse();

            sut.WordViewModel.Should().NotBeNull();
            sut.WordViewModel.SoundUrl.Should().Be(wordModel.SoundUrl);
            sut.WordViewModel.SoundFileName.Should().Be(wordModel.SoundFileName);

            sut.WordViewModel.DefinitionViewModels.Should().HaveCount(wordModel.Definitions.Count());
            sut.WordViewModel.Variants.Should().HaveCount(wordModel.Variations.Count());

            dialogServiceMock.Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
