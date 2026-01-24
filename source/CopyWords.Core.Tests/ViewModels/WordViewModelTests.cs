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

        private Mock<Func<DefinitionViewModel, string>> _func = default!;

        [TestInitialize]
        public void Initialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            // Default to Danish parser
            _appSettings = _fixture.Create<AppSettings>();
            _appSettings.SelectedParser = SourceLanguage.Danish.ToString();

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(_appSettings);

            _func = new Mock<Func<DefinitionViewModel, string>>();
            _func.Setup(x => x.Invoke(It.IsAny<DefinitionViewModel>()))
                .Returns((DefinitionViewModel vm) => vm.HeadwordViewModel.Original!);
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
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
            sut.SetDefinition(definitionViewModel);

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
            sut.SetDefinition(definitionViewModel);

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

            _func.Setup(x => x.Invoke(It.IsAny<DefinitionViewModel>())).Throws(new ExamplesFromSeveralDefinitionsSelectedException("exception from unit tests"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.SetDefinition(definitionViewModel);

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

            _func.Setup(x => x.Invoke(It.IsAny<DefinitionViewModel>())).Throws(new Exception("exception from unit tests"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.SetDefinition(definitionViewModel);

            await sut.CompileAndCopyToClipboard("front", _func.Object);

            clipboardServiceMock.VerifyNoOtherCalls();
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Cannot copy front", "Error occurred while trying to copy front: exception from unit tests", "OK"));
        }

        #endregion

        #region Tests for CopyBackAsync

        [TestMethod]
        public async Task CopyBackAsync_Should_CallCompileBackAndSavesImages()
        {
            // Arrange
            string compiledBack = _fixture.Create<string>();
            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(compiledBack);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.SetDefinition(definitionViewModel);
            sut.CanCopyFront = true;

            // Act
            await sut.CopyBackAsync(CancellationToken.None);

            // Assert
            copySelectedToClipboardServiceMock.Verify(x => x.CompileBack(It.IsAny<DefinitionViewModel>()));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileImages(definitionViewModel), Times.Once);
            saveImageFileServiceMock.Verify(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Tests for ShareAsync

        [TestMethod]
        public async Task ShareAsync_Should_CallCopySelectedToClipboardService()
        {
            // Arrange
            string textToShareFront = _fixture.Create<string>();
            string textToShareBack = _fixture.Create<string>();

            var sharedTextRequests = new List<ShareTextRequest>();

            var shareMock = _fixture.Freeze<Mock<IShare>>();
            shareMock.Setup(x => x.RequestAsync(Capture.In(sharedTextRequests)));

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(textToShareFront);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileTranslations(It.IsAny<DefinitionViewModel>())).Returns(textToShareBack);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.ShowCopyButtons = false;

            // Act
            await sut.ShareAsync();

            // Assert
            shareMock.Verify(x => x.RequestAsync(It.IsAny<ShareTextRequest>()));

            sharedTextRequests[0].Subject.Should().Be(textToShareFront);
            sharedTextRequests[0].Text.Should().Be(textToShareBack);
            sharedTextRequests[0].Title.Should().Be("Share Translations");

            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileTranslations(It.IsAny<DefinitionViewModel>()));
        }

        [TestMethod]
        public async Task ShareAsync_WhenExceptionIsThrown_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

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

            sut.SetDefinition(definitionViewModel);
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

            sut.SetDefinition(definitionViewModel);
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

            sut.SetDefinition(definitionViewModel);
            sut.UpdateUI();

            sut.CanCopyEndings.Should().Be(expected);
        }

        [TestMethod]
        public void UpdateUI_WhenPlatformIsAndroid_SetsShowCopyButtonsToFalse()
        {
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.SetDefinition(definitionViewModel);
            sut.UpdateUI();

            sut.ShowCopyButtons.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void UpdateUI_WhenPlatformIsNotAndroid_SetsShowCopyButtonsFromSettings(bool showCopyButtonsSetting)
        {
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetShowCopyButtons()).Returns(showCopyButtonsSetting);

            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.SetDefinition(definitionViewModel);
            sut.UpdateUI();

            sut.ShowCopyButtons.Should().Be(showCopyButtonsSetting);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void UpdateUI_SetsShowAnkiButtonFromSettings(bool showAnkiButtonSetting)
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.GetShowAnkiButton()).Returns(showAnkiButtonSetting);

            DefinitionViewModel definitionViewModel = _fixture.Create<DefinitionViewModel>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.SetDefinition(definitionViewModel);
            sut.UpdateUI();

            sut.ShowAnkiButton.Should().Be(showAnkiButtonSetting);
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

        #region Tests for ClearExpressions and AddExpression

        [TestMethod]
        public void ClearExpressions_Should_ClearExpressionsCollection()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            var expression = new VariantViewModel(new Variant("test expression", "http://test.com"));
            sut.AddExpression(expression);
            sut.Expressions.Should().HaveCount(1);

            sut.ClearExpressions();

            sut.Expressions.Should().BeEmpty();
        }

        [TestMethod]
        public void AddExpression_Should_AddExpressionToCollection()
        {
            WordViewModel sut = _fixture.Create<WordViewModel>();
            var expression = new VariantViewModel(new Variant("test expression", "http://test.com"));

            sut.AddExpression(expression);

            sut.Expressions.Should().HaveCount(1);
            sut.Expressions[0].Word.Should().Be("test expression");
            sut.Expressions[0].Url.Should().Be("http://test.com");
        }

        #endregion

        #region Tests for AddNoteWithAnkiConnectAsync

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenAnkiDeckNameDanishIsEmptyAndParserIsDanish_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiDeckNameDanish = string.Empty;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()), Times.Never);
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenAnkiDeckNameSpanishIsEmptyAndParserIsSpanish_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiDeckNameSpanish = string.Empty;
            appSettings.SelectedParser = SourceLanguage.Spanish.ToString();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()), Times.Never);
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
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()), Times.Never);
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenFrontIsEmpty_ShowsAlert()
        {
            // Arrange
            const string front = "";
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please select at least one example.",
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
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please select at least one example.",
                "OK"));
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenSuccessful_CallsAddNoteAsyncWithCorrectParameters()
        {
            // Arrange
            string word = _fixture.Create<string>();
            string soundUrl = _fixture.Create<Uri>().ToString();
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();
            const long noteId = 1;

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeech(It.IsAny<DefinitionViewModel>())).Returns(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndings(It.IsAny<DefinitionViewModel>())).Returns(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamples(It.IsAny<DefinitionViewModel>())).Returns(examples);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(noteId);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileToAnkiFolderAsync(soundUrl, word, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.Word = word;
            sut.SoundUrl = soundUrl;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            ankiConnectServiceMock.Verify(x => x.AddNoteAsync(
                It.Is<Models.AnkiNote>(note =>
                    note.DeckName == _appSettings.AnkiDeckNameDanish &&
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
                    note.Options.DuplicateScopeOptions.DeckName == _appSettings.AnkiDeckNameDanish &&
                    note.Options.DuplicateScopeOptions.CheckChildren == false),
                It.IsAny<CancellationToken>()), Times.Once);

            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            copySelectedToClipboardServiceMock.Verify(x => x.CompileImages(It.IsAny<DefinitionViewModel>()), Times.Once);

            saveImageFileServiceMock.Verify(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()), Times.Once);
            saveSoundFileServiceMock.Verify(x => x.SaveSoundFileToAnkiFolderAsync(soundUrl, word, It.IsAny<CancellationToken>()), Times.Once);
            copySelectedToClipboardServiceMock.Verify(x => x.CompileSoundFileName(word), Times.Once);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenNoteAdded_SavesMedia()
        {
            // Arrange
            string word = _fixture.Create<string>();
            string soundUrl = _fixture.Create<Uri>().ToString();
            const long noteId = 1;

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns("front");
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns("back");

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(noteId);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileToAnkiFolderAsync(soundUrl, word, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.Word = word;
            sut.SoundUrl = soundUrl;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            copySelectedToClipboardServiceMock.Verify(x => x.CompileImages(It.IsAny<DefinitionViewModel>()), Times.Once);
            saveImageFileServiceMock.Verify(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()), Times.Once);
            saveSoundFileServiceMock.Verify(x => x.SaveSoundFileToAnkiFolderAsync(soundUrl, word, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenNoteIsNotAdded_DoesNotSaveMedia()
        {
            // Arrange
            string word = _fixture.Create<string>();
            string soundUrl = _fixture.Create<Uri>().ToString();
            const long noteId = 0;

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns("front");
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns("back");

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(noteId);

            var saveImageFileServiceMock = _fixture.Freeze<Mock<ISaveImageFileService>>();
            saveImageFileServiceMock
                .Setup(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var saveSoundFileServiceMock = _fixture.Freeze<Mock<ISaveSoundFileService>>();
            saveSoundFileServiceMock.Setup(x => x.SaveSoundFileToAnkiFolderAsync(soundUrl, word, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.Word = word;
            sut.SoundUrl = soundUrl;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            copySelectedToClipboardServiceMock.Verify(x => x.CompileImages(It.IsAny<DefinitionViewModel>()), Times.Never);
            saveImageFileServiceMock.Verify(x => x.SaveImagesAsync(It.IsAny<IEnumerable<ImageFile>>(), It.IsAny<CancellationToken>()), Times.Never);
            saveSoundFileServiceMock.Verify(x => x.SaveSoundFileToAnkiFolderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiConnectAsync_WhenAnkiConnectNotRunningExceptionThrown_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AnkiConnectNotRunningException("Connection refused"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

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
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var ankiConnectServiceMock = _fixture.Freeze<Mock<IAnkiConnectService>>();
            ankiConnectServiceMock.Setup(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("General error from unit test"));

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiConnectAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Error occurred while trying to add note with AnkiConnect: General error from unit test",
                "OK"));
        }
        #endregion

        #region Tests for AddNoteWithAnkiDroidServiceAsync

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenAnkiDeckNameDanishIsEmptyAndParserIsDanish_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiDeckNameDanish = string.Empty;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()), Times.Never);
            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenAnkiDeckNameSpanishIsEmptyAndParserIsSpanish_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiDeckNameSpanish = string.Empty;
            appSettings.SelectedParser = SourceLanguage.Spanish.ToString();

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()), Times.Never);
            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenAnkiModelNameIsEmpty_ShowsAlert()
        {
            // Arrange
            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.AnkiModelName = string.Empty;

            Mock<ISettingsService> settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);
            settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(SourceLanguage.Danish.ToString());

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Please configure Anki deck name and model name in the settings.",
                "OK"));
            copySelectedToClipboardServiceMock.Verify(x => x.CompileFront(It.IsAny<DefinitionViewModel>()), Times.Never);
            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenSuccessful_CallsAddNoteAsyncWithCorrectParameters()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();
            var images = new List<ImageFile>()
            {
                new ImageFile(FileName: "image1.jpg", ImageUrl: "http://example.com/image1.jpg"),
                new ImageFile(FileName: "image2.jpg", ImageUrl: "http://example.com/image2.jpg")
            };

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeech(It.IsAny<DefinitionViewModel>())).Returns(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndings(It.IsAny<DefinitionViewModel>())).Returns(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamples(It.IsAny<DefinitionViewModel>())).Returns(examples);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileImages(It.IsAny<DefinitionViewModel>())).Returns(images);

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.SoundUrl = _fixture.Create<Uri>().ToString();

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(
                It.Is<Models.AnkiNote>(note =>
                    note.DeckName == _appSettings.AnkiDeckNameDanish &&
                    note.ModelName == _appSettings.AnkiModelName &&
                    note.Front == front &&
                    note.Back == back &&
                    note.PartOfSpeech == partOfSpeech &&
                    note.Forms == endings &&
                    note.Example == examples &&
                    note.Sound == null &&
                    note.Picture!.Count() == 2 &&
                    note.Audio!.Count() == 1 &&
                    note.Options != null &&
                    note.Options.AllowDuplicate == false &&
                    note.Options.DuplicateScope == "deck" &&
                    note.Options.DuplicateScopeOptions != null &&
                    note.Options.DuplicateScopeOptions.DeckName == _appSettings.AnkiDeckNameDanish &&
                    note.Options.DuplicateScopeOptions.CheckChildren == false),
                It.IsAny<CancellationToken>()),
                Times.Once);

            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenNoteIdIsGreaterThanZero_ShowsToast()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();
            long noteId = 12345L;

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeech(It.IsAny<DefinitionViewModel>())).Returns(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndings(It.IsAny<DefinitionViewModel>())).Returns(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamples(It.IsAny<DefinitionViewModel>())).Returns(examples);

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            ankiDroidServiceMock
                .Setup(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(noteId);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.SoundUrl = null;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Once);

            dialogServiceMock.Verify(x => x.DisplayToast("The note has been added to Anki."), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenNoteIdIsZero_DoesNotShowToast()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();
            string partOfSpeech = _fixture.Create<string>();
            string endings = _fixture.Create<string>();
            string examples = _fixture.Create<string>();
            long noteId = 0L;

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);
            copySelectedToClipboardServiceMock.Setup(x => x.CompilePartOfSpeech(It.IsAny<DefinitionViewModel>())).Returns(partOfSpeech);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileEndings(It.IsAny<DefinitionViewModel>())).Returns(endings);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileExamples(It.IsAny<DefinitionViewModel>())).Returns(examples);

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            ankiDroidServiceMock
                .Setup(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(noteId);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.SoundUrl = null;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Once);

            dialogServiceMock.Verify(x => x.DisplayToast(It.IsAny<string>()), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenAnkiDroidApiIsNotAvailable_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(false);

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "AnkiDroid API is not accessible",
                "Please verify that AndkiDroid API is enabled: run AnkiDroid -> Settings -> Advanced -> Enable AndkiDroid API",
                "OK"));

            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task AddNoteWithAnkiDroidServiceAsync_WhenAnkiDroidServiceAddNoteAsyncThrowsException_ShowsAlert()
        {
            // Arrange
            string front = _fixture.Create<string>();
            string back = _fixture.Create<string>();

            var copySelectedToClipboardServiceMock = _fixture.Freeze<Mock<ICopySelectedToClipboardService>>();
            copySelectedToClipboardServiceMock.Setup(x => x.CompileFront(It.IsAny<DefinitionViewModel>())).Returns(front);
            copySelectedToClipboardServiceMock.Setup(x => x.CompileBack(It.IsAny<DefinitionViewModel>())).Returns(back);

            var ankiDroidServiceMock = _fixture.Freeze<Mock<IAnkiDroidService>>();
            ankiDroidServiceMock.Setup(x => x.IsAvailable()).Returns(true);
            ankiDroidServiceMock
                .Setup(x => x.AddNoteAsync(It.IsAny<Models.AnkiNote>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("exception from unit test"));

            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            WordViewModel sut = _fixture.Create<WordViewModel>();
            sut.CanCopyFront = true;
            sut.SoundUrl = null;

            // Act
            await sut.AddNoteWithAnkiDroidServiceAsync(CancellationToken.None);

            // Assert
            dialogServiceMock.Verify(x => x.DisplayAlertAsync(
                "Cannot add note",
                "Error occurred while trying to add note with  AndkiDroid API: exception from unit test",
                "OK"));

            ankiDroidServiceMock.Verify(x => x.AddNoteAsync(It.IsAny<AnkiNote>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Tests for ShowShareButton

        [TestMethod]
        public void ShowShareButton_WhenPlatformIsAndroid_ReturnsTrue()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowShareButton.Should().BeTrue();
        }

        [TestMethod]
        public void ShowShareButton_WhenPlatformIsWinUI_ReturnsFalse()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowShareButton.Should().BeFalse();
        }

        [TestMethod]
        public void ShowShareButton_WhenPlatformIsMacCatalyst_ReturnsFalse()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.MacCatalyst);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowShareButton.Should().BeFalse();
        }

        [TestMethod]
        public void ShowShareButton_WhenPlatformIsiOS_ReturnsFalse()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.iOS);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowShareButton.Should().BeFalse();
        }

        #endregion

        #region Tests for ShowSaveSoundButton

        [TestMethod]
        public void ShowSaveSoundButton_WhenPlatformIsWinUI_ReturnsTrue()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowSaveSoundButton.Should().BeTrue();
        }

        [TestMethod]
        public void ShowSaveSoundButton_WhenPlatformIsMacCatalyst_ReturnsTrue()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.MacCatalyst);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowSaveSoundButton.Should().BeTrue();
        }

        [TestMethod]
        public void ShowSaveSoundButton_WhenPlatformIsAndroid_ReturnsFalse()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowSaveSoundButton.Should().BeFalse();
        }

        [TestMethod]
        public void ShowSaveSoundButton_WhenPlatformIsiOS_ReturnsFalse()
        {
            // Arrange
            var deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.iOS);

            // Act
            WordViewModel sut = _fixture.Create<WordViewModel>();

            // Assert
            sut.ShowSaveSoundButton.Should().BeFalse();
        }

        #endregion
    }
}
