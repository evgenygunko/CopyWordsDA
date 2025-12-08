using AutoFixture;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class HistoryPageViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public void Init_Should_FillPreviousWords()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadHistory()).Returns(["haj", "fisk"]);

            var sut = _fixture.Create<HistoryPageViewModel>();
            sut.Init();

            sut.PreviousWords.Should().HaveCount(2);
        }

        [TestMethod]
        public void Init_WhenPreviousWordViewModel_CallsInstantTranslationService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            settingsServiceMock.Setup(x => x.LoadHistory()).Returns(["haj"]);

            var instantTranslationServiceMock = _fixture.Freeze<Mock<IInstantTranslationService>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = _fixture.Create<HistoryPageViewModel>();
            sut.Init();

            PreviousWordViewModel previousWordViewModel = sut.PreviousWords[0];
            previousWordViewModel.SelectPreviousWord();

            instantTranslationServiceMock.Verify(x => x.SetText("haj"));
            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(st => st.Location.ToString() == "..")));
        }

        [TestMethod]
        public async Task ClearHistoryAsync_WhenConfirmed_CallsSettingsService()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync("Clear History", "Are you sure you want to clear all history?", "Clear", "Cancel"))
                .ReturnsAsync(true);

            var sut = _fixture.Create<HistoryPageViewModel>();
            sut.PreviousWords.Add(new PreviousWordViewModel("haj"));
            sut.PreviousWords.Should().HaveCount(1);

            await sut.ClearHistoryAsync();

            sut.PreviousWords.Should().HaveCount(0);
            settingsServiceMock.Verify(x => x.ClearHistory());
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Clear History", "Are you sure you want to clear all history?", "Clear", "Cancel"), Times.Once);
        }

        [TestMethod]
        public async Task ClearHistoryAsync_WhenCanceled_DoesNotClearHistory()
        {
            var settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();
            dialogServiceMock
                .Setup(x => x.DisplayAlertAsync("Clear History", "Are you sure you want to clear all history?", "Clear", "Cancel"))
                .ReturnsAsync(false);

            var sut = _fixture.Create<HistoryPageViewModel>();
            sut.PreviousWords.Add(new PreviousWordViewModel("haj"));
            sut.PreviousWords.Should().HaveCount(1);

            await sut.ClearHistoryAsync();

            sut.PreviousWords.Should().HaveCount(1);
            settingsServiceMock.Verify(x => x.ClearHistory(), Times.Never);
            dialogServiceMock.Verify(x => x.DisplayAlertAsync("Clear History", "Are you sure you want to clear all history?", "Clear", "Cancel"), Times.Once);
        }

        [TestMethod]
        public async Task CancelAsync_Should_CallShellService()
        {
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = _fixture.Create<HistoryPageViewModel>();
            await sut.CancelAsync();

            shellServiceMock.Verify(x => x.GoToAsync(It.Is<ShellNavigationState>(st => st.Location.ToString() == "..")));
        }
    }
}