using System.Collections.ObjectModel;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class WordViewModelTests
    {
        private Fixture _fixture = default!;
        private AppSettings _appSettings = default!;

        private Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>> _func = default!;

        [TestInitialize]
        public void Initialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _appSettings = _fixture.Create<AppSettings>();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(_appSettings);

            _func = new Mock<Func<ObservableCollection<DefinitionViewModel>, Task<string>>>();
            _func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>()))
                .Returns((ObservableCollection<DefinitionViewModel> vms) => Task.FromResult(vms.First().HeadwordViewModel.Original!));
        }

        #region Tests for CanSaveSoundFile

        [TestMethod]
        public void CanSaveSoundFile_WhenIsBusy_ReturnsFalse()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.IsBusy = true;
            sut.SoundUrl = _fixture.Create<string>();

            sut.CanSaveSoundFile.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void CanSaveSoundFile_WhenSoundUrlIsNullOrEmpty_ReturnsFalse(string soundUrl)
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.IsBusy = false;
            sut.SoundUrl = soundUrl;

            sut.CanSaveSoundFile.Should().BeFalse();
        }

        [TestMethod]
        public void CanSaveSoundFile_WhenSoundUrlIsProvided_ReturnsTrue()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.IsBusy = false;
            sut.SoundUrl = _fixture.Create<string>();

            sut.CanSaveSoundFile.Should().BeTrue();
        }

        #endregion

        #region Tests for SaveSoundFileAsync

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenFileSaved_ShowsToast()
        {
            var soundFileName = _fixture.Create<string>();
            var compiledFileName = $"[sound:{soundFileName}.mp3]";

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileSoundFileName(It.IsAny<string>()))
                .Returns(compiledFileName);

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.Word = soundFileName;
            await sut.SaveSoundFileAsync(It.IsAny<CancellationToken>());

            sut.IsBusy.Should().BeFalse();
            saveSoundFileServiceMock.Verify();
            copySelectedToClipboardServiceMock.Verify(x => x.CompileSoundFileName(soundFileName), Times.Once);
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(compiledFileName), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayToast("Sound file saved"));
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenFileNotSaved_DoesNotShowToast()
        {
            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            await sut.SaveSoundFileAsync(It.IsAny<CancellationToken>());

            sut.IsBusy.Should().BeFalse();
            saveSoundFileServiceMock.Verify();
            copySelectedToClipboardServiceMock.Verify(x => x.CompileSoundFileName(It.IsAny<string>()), Times.Never);
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayToast(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task SaveSoundFileAsync_WhenExceptionThrown_ShowsAlert()
        {
            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("exception from unit test"));

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            await sut.SaveSoundFileAsync(It.IsAny<CancellationToken>());

            sut.IsBusy.Should().BeFalse();
            saveSoundFileServiceMock.Verify();
            copySelectedToClipboardServiceMock.Verify(x => x.CompileSoundFileName(It.IsAny<string>()), Times.Never);
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot save sound file", It.IsAny<string>(), "OK"));
        }

        #endregion

        #region Tests for CompileAndCopyToClipboard

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenTextIsCopied_DisplaysToast()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync(definitionViewModel.HeadwordViewModel.Original!));
            dialogServiceMock.Verify(x => x.DisplayToast("Front copied"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenWordIsNotCopied_DisplaysWarning()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.HeadwordViewModel.Original = string.Empty;

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Text not copied", "Please select at least one example", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenExamplesFromSeveralDefinitionsSelectedExceptionThrown_ShowsWarning()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.HeadwordViewModel.Original = string.Empty;

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            _func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ThrowsAsync(new ExamplesFromSeveralDefinitionsSelectedException("exception from unit tests"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot copy front", "exception from unit tests", "OK"));
        }

        [TestMethod]
        public async Task CompileAndCopyToClipboard_WhenExceptionThrown_ShowsWarning()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.HeadwordViewModel.Original = string.Empty;

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            _func.Setup(x => x.Invoke(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ThrowsAsync(new Exception("exception from unit tests"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.DefinitionViewModels.Add(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot copy front", "Error occurred while trying to copy front: exception from unit tests", "OK"));
        }

        #endregion

        #region Tests for ShareAsync

        [TestMethod]
        public async Task ShareAsync_WhenCopyButtonsShownAndCompileFrontAsyncReturnsValue_CallsCompileBackAsync()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var sharedTextRequests = new List<ShareTextRequest>();

            var shareMock = _fixture.Freeze<Mock<IShare>>();
            shareMock.Setup(x => x.RequestAsync(Capture.In(sharedTextRequests)));

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.ShowCopyButtons = true;

            // Act
            await sut.ShareAsync();

            // Assert
            shareMock.Verify(x => x.RequestAsync(It.IsAny<ShareTextRequest>()));

            sharedTextRequests[0].Subject.Should().Be(front);
            sharedTextRequests[0].Text.Should().Be(back);
            sharedTextRequests[0].Title.Should().Be("Share Translations");

            copySelectedToClipboardServiceMock.Verify(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>()));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>()));
        }

        [TestMethod]
        public async Task ShareAsync_WhenCopyButtonsHidden_CallsCompileHeadword()
        {
            // Arrange
            string textToShare = _fixture.Create<string>();

            var sharedTextRequests = new List<ShareTextRequest>();

            var shareMock = _fixture.Freeze<Mock<IShare>>();
            shareMock.Setup(x => x.RequestAsync(Capture.In(sharedTextRequests)));

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileHeadword(It.IsAny<ObservableCollection<DefinitionViewModel>>())).Returns(textToShare);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.ShowCopyButtons = false;

            // Act
            await sut.ShareAsync();

            // Assert
            shareMock.Verify(x => x.RequestAsync(It.IsAny<ShareTextRequest>()));

            sharedTextRequests[0].Subject.Should().Be(textToShare);
            sharedTextRequests[0].Text.Should().Be(textToShare);
            sharedTextRequests[0].Title.Should().Be("Share Translations");

            copySelectedToClipboardServiceMock.Verify(x => x.CompileHeadword(It.IsAny<ObservableCollection<DefinitionViewModel>>()));
        }

        [TestMethod]
        public async Task ShareAsync_WhenCopyButtonsShownAndCompileFrontAsyncDoesNotReturnValue_CallsCompileHeadword()
        {
            // Arrange
            string textToShare = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(string.Empty);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(string.Empty);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileHeadword(It.IsAny<ObservableCollection<DefinitionViewModel>>())).Returns(textToShare);

            var sharedTextRequests = new List<ShareTextRequest>();

            var shareMock = _fixture.Freeze<Mock<IShare>>();
            shareMock.Setup(x => x.RequestAsync(Capture.In(sharedTextRequests)));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.ShowCopyButtons = true;

            // Act
            await sut.ShareAsync();

            // Assert
            shareMock.Verify(x => x.RequestAsync(It.IsAny<ShareTextRequest>()));

            sharedTextRequests[0].Subject.Should().Be(textToShare);
            sharedTextRequests[0].Text.Should().Be(textToShare);
            sharedTextRequests[0].Title.Should().Be("Share Translations");

            copySelectedToClipboardServiceMock.Verify(x => x.CompileHeadword(It.IsAny<ObservableCollection<DefinitionViewModel>>()));
        }

        [TestMethod]
        public async Task ShareAsync_WhenExamplesFromSeveralDefinitionsSelectedExceptionIsThrown_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var shareMock = _fixture.Freeze<Mock<IShare>>();
            shareMock.Setup(x => x.RequestAsync(It.IsAny<ShareTextRequest>()))
                .ThrowsAsync(new ExamplesFromSeveralDefinitionsSelectedException("please select examples from one definition!"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.ShowCopyButtons = true;

            // Act
            await sut.ShareAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot share the word", "please select examples from one definition!", "OK"));
        }

        [TestMethod]
        public async Task ShareAsync_WhenExceptionIsThrown_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var shareMock = _fixture.Freeze<Mock<IShare>>();
            shareMock.Setup(x => x.RequestAsync(It.IsAny<ShareTextRequest>()))
                .ThrowsAsync(new Exception("exception from unit test"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.ShowCopyButtons = true;

            // Act
            await sut.ShareAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot share the word", "Error occurred while trying to share the word: exception from unit test", "OK"));
        }

        #endregion

        #region Tests for UpdateUI

        [TestMethod]
        public void UpdateUI_WhenThereIsAtLeastOneDefinitionVM_SetsCanCopyFrontToTrue()
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = false; // Explicitly set initial state
            sut.CanCopyFront.Should().BeFalse();

            sut.DefinitionViewModels.Add(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyFront.Should().BeTrue();
        }

        [TestMethod]
        [DataRow("test", true)]
        [DataRow("", false)]
        public void UpdateUI_WhenThereIsAtLeastOnePartOfSpeech_SetsCanCopyPartOfSpeechToTrue(string partOfSpeech, bool expected)
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.PartOfSpeech = partOfSpeech;

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyPartOfSpeech = false; // Explicitly set initial state
            sut.CanCopyPartOfSpeech.Should().BeFalse();

            sut.DefinitionViewModels.Add(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyPartOfSpeech.Should().Be(expected);
        }

        [TestMethod]
        [DataRow("test", true)]
        [DataRow("", false)]
        public void UpdateUI_WhenThereIsAtLeastOneEnding_SetsCanCopyEndingsToTrue(string ending, bool expected)
        {
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();
            definitionViewModel.Endings = ending;

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyEndings = false; // Explicitly set initial state
            sut.CanCopyEndings.Should().BeFalse();

            sut.DefinitionViewModels.Add(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyEndings.Should().Be(expected);
        }

        #endregion

        #region Tests for PlayNewSoundAsync

        [TestMethod]
        public async Task PlayNewSoundAsync_OnWindows_CallsPlayTwoTimes()
        {
            string soundUrl = _fixture.Create<Uri>().ToString();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var mediaPlayerMock = new Mock<IMediaElementWrapper>();

            var sut = _fixture.Create<WordViewModel>();
            await sut.PlayNewSoundAsync(mediaPlayerMock.Object, soundUrl);

            mediaPlayerMock.Verify(x => x.Play(), Times.Exactly(2));
        }

        [TestMethod]
        public async Task PlayNewSoundAsync_OnAndroid_CallsPlayOnce()
        {
            string soundUrl = _fixture.Create<Uri>().ToString();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var mediaPlayerMock = new Mock<IMediaElementWrapper>();

            var sut = _fixture.Create<WordViewModel>();
            await sut.PlayNewSoundAsync(mediaPlayerMock.Object, soundUrl);

            mediaPlayerMock.Verify(x => x.Play(), Times.Once());
        }

        [TestMethod]
        public async Task PlayNewSoundAsync_OnMacCatalyst_CallsPlayOnce()
        {
            string soundUrl = _fixture.Create<Uri>().ToString();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.MacCatalyst);

            var mediaPlayerMock = new Mock<IMediaElementWrapper>();

            var sut = _fixture.Create<WordViewModel>();
            await sut.PlayNewSoundAsync(mediaPlayerMock.Object, soundUrl);

            mediaPlayerMock.Verify(x => x.Play(), Times.Once());
        }

        #endregion

        #region Tests for PlaySameSoundAgainAsync

        [TestMethod]
        public async Task PlaySameSoundAgainAsync_OnWindows_CallsPlay()
        {
            string soundUrl = _fixture.Create<Uri>().ToString();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var mediaPlayerMock = new Mock<IMediaElementWrapper>();

            var sut = _fixture.Create<WordViewModel>();
            await sut.PlaySameSoundAgainAsync(mediaPlayerMock.Object, soundUrl);

            mediaPlayerMock.Verify(x => x.Play(), Times.Once());
        }

        [TestMethod]
        public async Task PlaySameSoundAgainAsync_OnAndroid_CallsSeekTo()
        {
            string soundUrl = _fixture.Create<Uri>().ToString();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            var mediaPlayerMock = new Mock<IMediaElementWrapper>();

            var sut = _fixture.Create<WordViewModel>();
            await sut.PlaySameSoundAgainAsync(mediaPlayerMock.Object, soundUrl);

            mediaPlayerMock.Verify(x => x.SeekTo(TimeSpan.Zero, It.IsAny<CancellationToken>()), Times.Once());
            mediaPlayerMock.Verify(x => x.Play(), Times.Never());
        }

        [TestMethod]
        public async Task PlaySameSoundAgainAsync_OnMacCatalyst_CallsSeekToAndPlay()
        {
            string soundUrl = _fixture.Create<Uri>().ToString();

            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.MacCatalyst);

            var mediaPlayerMock = new Mock<IMediaElementWrapper>();

            var sut = _fixture.Create<WordViewModel>();
            await sut.PlaySameSoundAgainAsync(mediaPlayerMock.Object, soundUrl);

            mediaPlayerMock.Verify(x => x.SeekTo(TimeSpan.Zero, It.IsAny<CancellationToken>()), Times.Once());
            mediaPlayerMock.Verify(x => x.Play(), Times.Once());
        }

        #endregion

        #region Tests for AddNoteWithAnkiConnectAsync

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenAnkiDeckNameIsEmpty_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiDeckName = string.Empty;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>()), Times.Never);
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenAnkiModelNameIsEmpty_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiModelName = string.Empty;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>()), Times.Never);
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenFrontIsEmpty_ShowsAlert()
        {
            // Arrange
            const string front = "";
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please select at least example.",
                "OK"));
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenBackIsEmpty_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            const string back = "";

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please select at least example.",
                "OK"));
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenSuccessful_CallsAddNoteAsyncWithCorrectParameters()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeechAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndingsAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamplesAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(examples);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(
                It.Is<Models.AnkiNote>(note =>
                    note.DeckName == _appSettings.AnkiDeckName &&
                    note.ModelName == _appSettings.AnkiModelName &&
                    note.Front == front &&
                    note.Back == back &&
                    note.PartOfSpeech == partOfSpeech &&
                    note.Forms == endings &&
                    note.Example == examples &&
                    note.Sound == null &&
                    note.Options != null &&
                    note.Options.AllowDuplicate == false &&
                    note.Options.DuplicateScope == "deck" &&
                    note.Options.DuplicateScopeOptions != null &&
                    note.Options.DuplicateScopeOptions.DeckName == _appSettings.AnkiDeckName &&
                    note.Options.DuplicateScopeOptions.CheckChildren == false),
                It.IsAny<CancellationToken>()), Times.Once);

            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenSoundFileIsSaved_CallsAddNoteAsyncWithSoundFileName()
        {
            // Arrange
            string word = _fixture.Create<string>();
            string soundUrl = _fixture.Create<Uri>().ToString();
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();
            string soundFileName = $"[sound:{word}.mp3]";

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeechAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndingsAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamplesAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(examples);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileSoundFileName(word)).Returns(soundFileName);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.Word = word;
            sut.SoundUrl = soundUrl;
            sut.IsBusy = false;
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            saveSoundFileServiceMock.Verify(x => x.SaveSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>()), Times.Once);
            copySelectedToClipboardServiceMock.Verify(x => x.CompileSoundFileName(word), Times.Once);

            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(
                It.Is<Models.AnkiNote>(note =>
                    note.DeckName == _appSettings.AnkiDeckName &&
                    note.ModelName == _appSettings.AnkiModelName &&
                    note.Front == front &&
                    note.Back == back &&
                    note.PartOfSpeech == partOfSpeech &&
                    note.Forms == endings &&
                    note.Example == examples &&
                    note.Sound == soundFileName &&
                    note.Options != null &&
                    note.Options.AllowDuplicate == false &&
                    note.Options.DuplicateScope == "deck" &&
                    note.Options.DuplicateScopeOptions != null &&
                    note.Options.DuplicateScopeOptions.DeckName == _appSettings.AnkiDeckName &&
                    note.Options.DuplicateScopeOptions.CheckChildren == false),
                It.IsAny<CancellationToken>()), Times.Once);

            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenSoundFileFailsToSave_CallsAddNoteAsyncWithoutSoundFileName()
        {
            // Arrange
            string word = _fixture.Create<string>();
            string soundUrl = _fixture.Create<Uri>().ToString();
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeechAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndingsAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamplesAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(examples);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.Word = word;
            sut.SoundUrl = soundUrl;
            sut.IsBusy = false;
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            saveSoundFileServiceMock.Verify(x => x.SaveSoundFileAsync(soundUrl, word, It.IsAny<CancellationToken>()), Times.Once);
            copySelectedToClipboardServiceMock.Verify(x => x.CompileSoundFileName(It.IsAny<string>()), Times.Never);

            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(
                It.Is<Models.AnkiNote>(note =>
                    note.DeckName == _appSettings.AnkiDeckName &&
                    note.ModelName == _appSettings.AnkiModelName &&
                    note.Front == front &&
                    note.Back == back &&
                    note.PartOfSpeech == partOfSpeech &&
                    note.Forms == endings &&
                    note.Example == examples &&
                    note.Sound == null &&
                    note.Options != null &&
                    note.Options.AllowDuplicate == false &&
                    note.Options.DuplicateScope == "deck" &&
                    note.Options.DuplicateScopeOptions != null &&
                    note.Options.DuplicateScopeOptions.DeckName == _appSettings.AnkiDeckName &&
                    note.Options.DuplicateScopeOptions.CheckChildren == false),
                It.IsAny<CancellationToken>()), Times.Once);

            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenAnkiConnectNotRunningExceptionThrown_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AnkiConnectNotRunningException("Connection refused"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "AnkiConnect is not running",
                It.Is<string>(msg => msg.Contains("Please verify that AnkiConnect is installed") && msg.Contains("Connection refused")),
                "OK"));
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenGeneralExceptionThrown_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFrontAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBackAsync(It.IsAny<ObservableCollection<DefinitionViewModel>>())).ReturnsAsync(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("General error from unit test"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync();

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Error occurred while trying to add note with AnkiConnect: General error from unit test",
                "OK"));
        }
        #endregion
    }
}
