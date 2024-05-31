using System.Collections.ObjectModel;
using Autofac.Extras.Moq;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class WordViewModelTests
    {
        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsCopied_DisplaysToast()
        {
            using (var mock = AutoMock.GetLoose())
            {
                const string textToCopy = "test word";

                var func = new Mock<Func<ObservableCollection<WordVariantViewModel>, Task<string>>>();
                func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<WordVariantViewModel>>())).Returns(Task.FromResult(textToCopy));

                var sut = mock.Create<WordViewModel>();

                await sut.CompileAndCopyToClipboard("front", func.Object);

                mock.Mock<IClipboardService>().Verify(x => x.CopyTextToClipboardAsync(textToCopy));
                mock.Mock<IDialogService>().Verify(x => x.DisplayToast("Front copied"));
            }
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsNotCopied_DisplaysWarning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                string textToCopy = string.Empty;

                var func = new Mock<Func<ObservableCollection<WordVariantViewModel>, Task<string>>>();
                func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<WordVariantViewModel>>())).Returns(Task.FromResult(textToCopy));

                var sut = mock.Create<WordViewModel>();

                await sut.CompileAndCopyToClipboard("front", func.Object);

                mock.Mock<IClipboardService>().Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
                mock.Mock<IDialogService>().Verify(x => x.DisplayAlert("Text not copied", "Please select at least one example", "OK"));
            }
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenPrepareWordForCopyingExceptionThrown_ShowsWarning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var func = new Mock<Func<ObservableCollection<WordVariantViewModel>, Task<string>>>();
                func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<WordVariantViewModel>>())).ThrowsAsync(new PrepareWordForCopyingException("exception from unit tests"));

                var sut = mock.Create<WordViewModel>();

                await sut.CompileAndCopyToClipboard("front", func.Object);

                mock.Mock<IClipboardService>().Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
                mock.Mock<IDialogService>().Verify(x => x.DisplayAlert("Cannot copy front", "exception from unit tests", "OK"));
            }
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenExceptionThrown_ShowsWarning()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var func = new Mock<Func<ObservableCollection<WordVariantViewModel>, Task<string>>>();
                func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<WordVariantViewModel>>())).ThrowsAsync(new Exception("exception from unit tests"));

                var sut = mock.Create<WordViewModel>();

                await sut.CompileAndCopyToClipboard("front", func.Object);

                mock.Mock<IClipboardService>().Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
                mock.Mock<IDialogService>().Verify(x => x.DisplayAlert("Cannot copy front", "Error occurred while trying to copy front: exception from unit tests", "OK"));
            }
        }
    }
}
