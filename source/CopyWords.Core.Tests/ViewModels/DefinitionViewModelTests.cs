﻿using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class DefinitionViewModelTests
    {
        private Mock<ICopySelectedToClipboardService> _copySelectedToClipboardServiceMock = default!;
        private Mock<IClipboardService> _clipboardServiceMock = default!;
        private Mock<IDialogService> _dialogServiceMock = default!;

        private Mock<Func<DefinitionViewModel, Task<string>>> _func = default!;

        [TestInitialize]
        public void Initialize()
        {
            _copySelectedToClipboardServiceMock = new Mock<ICopySelectedToClipboardService>();
            _clipboardServiceMock = new Mock<IClipboardService>();
            _dialogServiceMock = new Mock<IDialogService>();

            _func = new Mock<Func<DefinitionViewModel, Task<string>>>();
            _func.Setup(x => x.Invoke(It.IsAny<DefinitionViewModel>())).Returns((DefinitionViewModel vm) => Task.FromResult(vm.Word));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsCopied_DisplaysToast()
        {
            const string textToCopy = "test word";
            Definition definition = new Definition(new Headword(textToCopy, null, null), PartOfSpeech: "verb", Endings: "", Enumerable.Empty<Context>());

            var sut = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);
            await sut.CompileAndCopyToClipboard("front", _func.Object);

            _clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(textToCopy));
            _dialogServiceMock.Verify(x => x.DisplayToast("Front copied"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsNotCopied_DisplaysWarning()
        {
            string textToCopy = string.Empty;
            Definition definition = new Definition(new Headword(textToCopy, null, null), PartOfSpeech: "verb", Endings: "", Enumerable.Empty<Context>());

            var sut = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);
            await sut.CompileAndCopyToClipboard("front", _func.Object);

            _clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            _dialogServiceMock.Verify(x => x.DisplayAlert("Text not copied", "Please select at least one example", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenPrepareWordForCopyingExceptionThrown_ShowsWarning()
        {
            const string textToCopy = "test word";
            Definition definition = new Definition(new Headword(textToCopy, null, null), PartOfSpeech: "verb", Endings: "", Enumerable.Empty<Context>());

            _func.Setup(x => x.Invoke(It.IsAny<DefinitionViewModel>())).ThrowsAsync(new PrepareWordForCopyingException("exception from unit tests"));

            var sut = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);
            await sut.CompileAndCopyToClipboard("front", _func.Object);

            _clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            _dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy front", "exception from unit tests", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenExceptionThrown_ShowsWarning()
        {
            const string textToCopy = "test word";
            Definition definition = new Definition(new Headword(textToCopy, null, null), PartOfSpeech: "verb", Endings: "", Enumerable.Empty<Context>());

            _func.Setup(x => x.Invoke(It.IsAny<DefinitionViewModel>())).ThrowsAsync(new Exception("exception from unit tests"));

            var sut = new DefinitionViewModel(definition, _copySelectedToClipboardServiceMock.Object, _dialogServiceMock.Object, _clipboardServiceMock.Object);
            await sut.CompileAndCopyToClipboard("front", _func.Object);

            _clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            _dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy front", "Error occurred while trying to copy front: exception from unit tests", "OK"));
        }
    }
}
