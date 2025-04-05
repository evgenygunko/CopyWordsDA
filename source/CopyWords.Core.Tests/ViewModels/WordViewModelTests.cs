using System.Collections.ObjectModel;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class WordViewModelTests
    {
        private Fixture _fixture = default!;

        private Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>> _func = default!;

        [TestInitialize]
        public void Initialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _func = new Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>>();
            _func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).Returns((ObservableCollection<DefinitionViewModel> vms) => Task.FromResult(vms.First().Word));
        }

        #region Tests for CanSaveSoundFile

        [TestMethod]
        public void CanSaveSoundFile_WhenIsBusy_ReturnsFalse()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.IsBusy = true;

            sut.CanSaveSoundFile.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSoundFile_WhenSoundFileNameIsNull_ReturnsFalse()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.IsBusy = false;
            sut.SoundFileName = null;

            sut.CanSaveSoundFile.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSoundFile_WhenSoundFileNameIsNotNull_ReturnsTrue()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.IsBusy = false;
            sut.SoundFileName = _fixture.Create<string>();

            sut.CanSaveSoundFile.Should().BeTrue();
        }

        #endregion

        #region Tests for SaveSoundFileAsync

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenFileSaved_ShowsToast()
        {
            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            await sut.SaveSoundFileAsync(It.IsAny<CancellationToken>());

            sut.IsBusy.Should().BeFalse();
            saveSoundFileServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast(It.IsAny<string>()));
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenFileNotSaved_DoesNotShowToast()
        {
            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            await sut.SaveSoundFileAsync(It.IsAny<CancellationToken>());

            sut.IsBusy.Should().BeFalse();
            saveSoundFileServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenExceptionThrown_ShowsAlert()
        {
            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("exception from unit test"));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            await sut.SaveSoundFileAsync(It.IsAny<CancellationToken>());

            sut.IsBusy.Should().BeFalse();
            saveSoundFileServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot save sound file", It.IsAny<string>(), "OK"));
        }

        #endregion

        #region Tests for CompileAndCopyToClipboard

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenTextIsCopied_DisplaysToast()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            Mock<IClipboardService> clipboardServiceMock = new Mock<IClipboardService>();
            Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                clipboardServiceMock.Object,
                Mock.Of<ICopySelectedToClipboardService>());
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(definitionViewModel.Word));
            dialogServiceMock.Verify(x => x.DisplayToast("Front copied"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsNotCopied_DisplaysWarning()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.Word = string.Empty;

            Mock<IClipboardService> clipboardServiceMock = new Mock<IClipboardService>();
            Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                clipboardServiceMock.Object,
                Mock.Of<ICopySelectedToClipboardService>());
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlert("Text not copied", "Please select at least one example", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenPrepareWordForCopyingExceptionThrown_ShowsWarning()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.Word = string.Empty;

            Mock<IClipboardService> clipboardServiceMock = new Mock<IClipboardService>();
            Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();

            _func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ThrowsAsync(new ExamplesFromSeveralDefinitionsSelectedException("exception from unit tests"));

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                clipboardServiceMock.Object,
                Mock.Of<ICopySelectedToClipboardService>());
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy front", "exception from unit tests", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenExceptionThrown_ShowsWarning()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.Word = string.Empty;

            Mock<IClipboardService> clipboardServiceMock = new Mock<IClipboardService>();
            Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();

            _func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ThrowsAsync(new Exception("exception from unit tests"));

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                clipboardServiceMock.Object,
                Mock.Of<ICopySelectedToClipboardService>());
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy front", "Error occurred while trying to copy front: exception from unit tests", "OK"));
        }

        #endregion

        #region Tests for OpenCopyMenuAsync

        [TestMethod]
        public async Task OpenCopyMenuAsync_WhenFrontIsSelected_CallsCopyFrontAsync()
        {
            string text = _fixture.Create<string>();

            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock
                .Setup(x => x.DisplayActionSheet("Select field to copy:", "Cancel", (string)null!, FlowDirection.LeftToRight, "Front", "Back", "Examples"))
                .ReturnsAsync("Front")
                .Verifiable();

            var copySelectedToClipboardServiceMock = new Mock<ICopySelectedToClipboardService>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(text);

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                Mock.Of<IClipboardService>(),
                copySelectedToClipboardServiceMock.Object);
            sut.CanCopyFront = true;

            await sut.OpenCopyMenuAsync();

            dialogServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast("Front copied"));
        }

        [TestMethod]
        public async Task OpenCopyMenuAsync_WhenBackIsSelected_CallsCopyBackAsync()
        {
            string text = _fixture.Create<string>();

            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock
                .Setup(x => x.DisplayActionSheet("Select field to copy:", "Cancel", (string)null!, FlowDirection.LeftToRight, "Front", "Back", "Examples"))
                .ReturnsAsync("Back")
                .Verifiable();

            var copySelectedToClipboardServiceMock = new Mock<ICopySelectedToClipboardService>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(text);

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                Mock.Of<IClipboardService>(),
                copySelectedToClipboardServiceMock.Object);
            sut.CanCopyFront = true;

            await sut.OpenCopyMenuAsync();

            dialogServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast("Back copied"));
        }

        [TestMethod]
        public async Task OpenCopyMenuAsync_WhenPartOfSpeechIsSelected_CallsCopyPartOfSpeechAsync()
        {
            string text = _fixture.Create<string>();

            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock
                .Setup(x => x.DisplayActionSheet("Select field to copy:", "Cancel", (string)null!, FlowDirection.LeftToRight, "Part of speech"))
                .ReturnsAsync("Part of speech")
                .Verifiable();

            var copySelectedToClipboardServiceMock = new Mock<ICopySelectedToClipboardService>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeechAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(text);

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                Mock.Of<IClipboardService>(),
                copySelectedToClipboardServiceMock.Object);
            sut.CanCopyPartOfSpeech = true;

            await sut.OpenCopyMenuAsync();

            dialogServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast("Word type copied"));
        }

        [TestMethod]
        public async Task OpenCopyMenuAsync_WhenEndingsIsSelected_CallsCopyEndingsAsync()
        {
            string text = _fixture.Create<string>();

            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock
                .Setup(x => x.DisplayActionSheet("Select field to copy:", "Cancel", (string)null!, FlowDirection.LeftToRight, "Endings"))
                .ReturnsAsync("Endings")
                .Verifiable();

            var copySelectedToClipboardServiceMock = new Mock<ICopySelectedToClipboardService>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndingsAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(text);

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                Mock.Of<IClipboardService>(),
                copySelectedToClipboardServiceMock.Object);
            sut.CanCopyEndings = true;

            await sut.OpenCopyMenuAsync();

            dialogServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast("Endings copied"));
        }

        [TestMethod]
        public async Task OpenCopyMenuAsync_WhenExamplesIsSelected_CallsCopyExamplesAsync()
        {
            string text = _fixture.Create<string>();

            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock
                .Setup(x => x.DisplayActionSheet("Select field to copy:", "Cancel", (string)null!, FlowDirection.LeftToRight, "Front", "Back", "Examples"))
                .ReturnsAsync("Examples")
                .Verifiable();

            var copySelectedToClipboardServiceMock = new Mock<ICopySelectedToClipboardService>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamplesAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(text);

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                dialogServiceMock.Object,
                Mock.Of<IClipboardService>(),
                copySelectedToClipboardServiceMock.Object);
            sut.CanCopyFront = true;

            await sut.OpenCopyMenuAsync();

            dialogServiceMock.Verify();
            dialogServiceMock.Verify(x => x.DisplayToast("Examples copied"));
        }

        #endregion

        #region Tests for UpdateUI

        [TestMethod]
        public void UpdateUI_WhenThereIsAtLeastOneDefinitionVM_SetsCanCopyFrontToTrue()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IClipboardService>(),
                Mock.Of<ICopySelectedToClipboardService>());
            sut.CanCopyFront.Should().BeFalse();

            sut.DefinitionViewModels.Add(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyFront.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow("test", true)]
        [DataRow("", false)]
        public void UpdateUI_WhenThereIsAtLeastOnePartOfSpeech_SetsCanCopyPartOfSpeechToTrue(string partOfSpeech, bool expected)
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.PartOfSpeech = partOfSpeech;

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IClipboardService>(),
                Mock.Of<ICopySelectedToClipboardService>());
            sut.CanCopyPartOfSpeech.Should().BeFalse();

            sut.DefinitionViewModels.Add(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyPartOfSpeech.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("test", true)]
        [DataRow("", false)]
        public void UpdateUI_WhenThereIsAtLeastOneEnding_SetsCanCopyEndingsToTrue(string ending, bool expected)
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.Endings = ending;

            WordViewModel sut = new WordViewModel(
                Mock.Of<ISaveSoundFileService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IClipboardService>(),
                Mock.Of<ICopySelectedToClipboardService>());
            sut.CanCopyEndings.Should().BeFalse();

            sut.DefinitionViewModels.Add(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyEndings.Should().Be(expected);
        }

        #endregion
    }
}
