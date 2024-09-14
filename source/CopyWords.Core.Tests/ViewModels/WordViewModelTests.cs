using System.Collections.ObjectModel;
using AutoFixture;
using CopyWords.Core.Services;
using CopyWords.Core.Tests.Services;
using CopyWords.Core.ViewModels;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class WordViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for CopyFrontAsync

        [TestMethod]
        public async Task CopyFrontAsync_Should_CallCopySelectedToClipboardService()
        {
            string front = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            sut.Front = front;
            sut.PartOfSpeech = partOfSpeech;

            await sut.CopyFrontAsync();

            copySelectedToClipboardServiceMock.Verify(x => x.CompileFrontAsync(front, partOfSpeech));
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Front copied"));
        }

        #endregion

        #region Tests for CopyBackAsync

        [TestMethod]
        public async Task CopyBackAsync_Should_CallCopySelectedToClipboardService()
        {
            // Arrange
            var definitions = CopySelectedToClipboardServiceTests.CreateVMForGrillspyd();
            string formattedBack = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock
                .Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>(), It.IsAny<HeadwordViewModel>()))
                .ReturnsAsync(formattedBack);

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            foreach (var definition in definitions)
            {
                sut.Definitions.Add(definition);
            }

            // Act
            await sut.CopyBackAsync();

            // Assert
            copySelectedToClipboardServiceMock.Verify(x => x.CompileBackAsync(definitions, It.IsAny<HeadwordViewModel>()));
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Back copied"));
        }

        [TestMethod]
        public async Task CopyBackAsync_WhenWordIsCopied_DisplaysToast()
        {
            var definitions = CopySelectedToClipboardServiceTests.CreateVMForGrillspyd();
            string formattedBack = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock
                .Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>(), It.IsAny<HeadwordViewModel>()))
                .ReturnsAsync(formattedBack);

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            foreach (var definition in definitions)
            {
                sut.Definitions.Add(definition);
            }

            await sut.CopyBackAsync();

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Back copied"));
        }

        [TestMethod]
        public async Task CopyBackAsync_WhenWordIsNotCopied_DisplaysWarning()
        {
            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();

            await sut.CopyBackAsync();

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Text not copied", "Please select at least one example", "OK"));
        }

        [TestMethod]
        public async Task CopyBackAsync_WhenExceptionThrown_ShowsWarning()
        {
            var definitions = CopySelectedToClipboardServiceTests.CreateVMForGrillspyd();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock
                .Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>(), It.IsAny<HeadwordViewModel>()))
                .ThrowsAsync(new Exception("exception from unit test"));

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            foreach (var definition in definitions)
            {
                sut.Definitions.Add(definition);
            }

            await sut.CopyBackAsync();

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy Back", "Error occurred while trying to copy Back: exception from unit test", "OK"));
        }

        #endregion

        #region Tests for CopyExamplesAsync

        [TestMethod]
        public async Task CopyExamplesAsync_Should_CallCopySelectedToClipboardService()
        {
            var definitions = CopySelectedToClipboardServiceTests.CreateVMForGrillspyd();
            string formattedExamples = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamplesAsync(definitions)).ReturnsAsync(formattedExamples);

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            foreach (var definition in definitions)
            {
                sut.Definitions.Add(definition);
            }

            await sut.CopyExamplesAsync();

            copySelectedToClipboardServiceMock.Verify(x => x.CompileExamplesAsync(definitions));
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Examples copied"));
        }

        [TestMethod]
        public async Task CopyExamplesAsync_WhenWordIsCopied_DisplaysToast()
        {
            var definitions = CopySelectedToClipboardServiceTests.CreateVMForGrillspyd();
            string formattedExamples = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock
                .Setup(x => x.CompileExamplesAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>()))
                .ReturnsAsync(formattedExamples);

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            foreach (var definition in definitions)
            {
                sut.Definitions.Add(definition);
            }

            await sut.CopyExamplesAsync();

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()));
            dialogServiceMock.Verify(x => x.DisplayToast("Examples copied"));
        }

        [TestMethod]
        public async Task CopyExamplesAsync_WhenWordIsNotCopied_DisplaysWarning()
        {
            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();

            await sut.CopyExamplesAsync();

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Text not copied", "Please select at least one example", "OK"));
        }

        [TestMethod]
        public async Task CopyExamplesAsync_WhenExceptionThrown_ShowsWarning()
        {
            var definitions = CopySelectedToClipboardServiceTests.CreateVMForGrillspyd();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock
                .Setup(x => x.CompileExamplesAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>()))
                .ThrowsAsync(new Exception("exception from unit test"));

            Mock<IClipboardService> clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            Mock<IDialogService> dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<WordViewModel>();
            foreach (var definition in definitions)
            {
                sut.Definitions.Add(definition);
            }

            await sut.CopyExamplesAsync();

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlert("Cannot copy Examples", "Error occurred while trying to copy Examples: exception from unit test", "OK"));
        }

        #endregion
    }
}
