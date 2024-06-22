using AutoFixture;
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
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();
            sut.SearchWord = search;

            await sut.LookUpAsync();

            sut.IsBusy.Should().BeFalse();

            sut.WordViewModel.Should().NotBeNull();
            sut.WordViewModel.Front.Should().Be(wordModel.Headword);
            sut.WordViewModel.PartOfSpeech.Should().Be(wordModel.PartOfSpeech);
            sut.WordViewModel.Forms.Should().Be(wordModel.Endings);
            sut.WordViewModel.SoundUrl.Should().Be(wordModel.SoundUrl);
            sut.WordViewModel.SoundFileName.Should().Be(wordModel.SoundFileName);

            sut.WordViewModel.Definitions.Should().HaveCount(wordModel.Definitions.Count());
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
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>())).ThrowsAsync(new Exception("exception from unit test"));

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
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>())).ReturnsAsync((WordModel?)null);

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
            lookUpWordMock.Setup(x => x.LookUpWordAsync(It.IsAny<string>())).ReturnsAsync(wordModel);

            var sut = _fixture.Create<MainViewModel>();

            WordModel result = await sut.LookUpWordInDictionaryAsync(search);

            result.Should().Be(wordModel);
            dialogServiceMock.Verify(x => x.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
