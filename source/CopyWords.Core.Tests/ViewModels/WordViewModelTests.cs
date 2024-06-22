using System.Collections.ObjectModel;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class WordViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsCopied_DisplaysToast()
        {
            const string textToCopy = "test word";

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var func = new Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>>();
            func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).Returns(Task.FromResult(textToCopy));

            var sut = _fixture.Create<WordViewModel>();

            await sut.CompileAndCopyToClipboard("front", func.Object);

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(textToCopy));
            dialogServiceMock.Verify(x => x.DisplayToast("Front copied"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsNotCopied_DisplaysWarning()
        {
            string textToCopy = string.Empty;

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var func = new Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>>();
            func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).Returns(Task.FromResult(textToCopy));

            var sut = _fixture.Create<WordViewModel>();

            await sut.CompileAndCopyToClipboard("front", func.Object);

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Text not copied", "Please select at least one example", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenPrepareWordForCopyingExceptionThrown_ShowsWarning()
        {
            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var func = new Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>>();
            func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ThrowsAsync(new PrepareWordForCopyingException("exception from unit tests"));

            var sut = _fixture.Create<WordViewModel>();

            await sut.CompileAndCopyToClipboard("front", func.Object);

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy front", "exception from unit tests", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenExceptionThrown_ShowsWarning()
        {
            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var func = new Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>>();
            func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ThrowsAsync(new Exception("exception from unit tests"));

            var sut = _fixture.Create<WordViewModel>();

            await sut.CompileAndCopyToClipboard("front", func.Object);

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy front", "Error occurred while trying to copy front: exception from unit tests", "OK"));
        }
    }
}
