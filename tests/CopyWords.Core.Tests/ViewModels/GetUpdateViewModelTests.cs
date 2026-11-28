using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class GetUpdateViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region GetLatestReleaseAsync

        [TestMethod]
        public async Task GetLatestReleaseAsync_WhenServiceReturnsValidRelease_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var releaseInfo = new ReleaseInfo("2.0.0", "New features and bug fixes", "https://example.com/download");

            var updateServiceMock = _fixture.Freeze<Mock<IUpdateService>>();
            updateServiceMock
                .Setup(x => x.GetLatestReleaseVersionAsync())
                .ReturnsAsync(releaseInfo);

            var sut = new GetUpdateViewModel(
                updateServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IAppInfo>());

            // Act
            await sut.GetLatestReleaseAsync();

            // Assert
            sut.WhatIsNew.Should().Be("What is new in 2.0.0");
            sut.UpdateDescription.Should().Be("New features and bug fixes");
            sut.LatestVersion.Should().Be("2.0.0");
            sut.DownloadUrl.Should().Be("https://example.com/download");
            sut.ErrorMessage.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetLatestReleaseAsync_WhenDownloadUrlIsInvalid_SetsErrorMessage()
        {
            // Arrange
            var releaseInfo = new ReleaseInfo("2.0.0", "New features", "not-a-valid-url");

            var updateServiceMock = _fixture.Freeze<Mock<IUpdateService>>();
            updateServiceMock
                .Setup(x => x.GetLatestReleaseVersionAsync())
                .ReturnsAsync(releaseInfo);

            var sut = new GetUpdateViewModel(
                updateServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IAppInfo>());

            // Act
            await sut.GetLatestReleaseAsync();

            // Assert
            sut.DownloadUrl.Should().Be("not-a-valid-url");
            sut.ErrorMessage.Should().Be("The download URL is not valid.");
        }

        [TestMethod]
        public async Task GetLatestReleaseAsync_WhenServiceThrowsException_SetsErrorMessage()
        {
            // Arrange
            var updateServiceMock = _fixture.Freeze<Mock<IUpdateService>>();
            updateServiceMock
                .Setup(x => x.GetLatestReleaseVersionAsync())
                .ThrowsAsync(new InvalidOperationException("Network error"));

            var sut = new GetUpdateViewModel(
                updateServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IAppInfo>());

            // Act
            await sut.GetLatestReleaseAsync();

            // Assert
            sut.ErrorMessage.Should().Be("An error occurred while checking for updates. Error: Network error");
        }

        [TestMethod]
        public async Task GetLatestReleaseAsync_WhenDownloadUrlIsEmpty_SetsErrorMessage()
        {
            // Arrange
            var releaseInfo = new ReleaseInfo("2.0.0", "New features", string.Empty);

            var updateServiceMock = _fixture.Freeze<Mock<IUpdateService>>();
            updateServiceMock
                .Setup(x => x.GetLatestReleaseVersionAsync())
                .ReturnsAsync(releaseInfo);

            var sut = new GetUpdateViewModel(
                updateServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IAppInfo>());

            // Act
            await sut.GetLatestReleaseAsync();

            // Assert
            sut.ErrorMessage.Should().Be("The download URL is not valid.");
        }

        [TestMethod]
        public async Task GetLatestReleaseAsync_WhenDownloadUrlIsRelative_SetsErrorMessage()
        {
            // Arrange
            var releaseInfo = new ReleaseInfo("2.0.0", "New features", "/relative/path/download");

            var updateServiceMock = _fixture.Freeze<Mock<IUpdateService>>();
            updateServiceMock
                .Setup(x => x.GetLatestReleaseVersionAsync())
                .ReturnsAsync(releaseInfo);

            var sut = new GetUpdateViewModel(
                updateServiceMock.Object,
                Mock.Of<IShellService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<IAppInfo>());

            // Act
            await sut.GetLatestReleaseAsync();

            // Assert
            sut.ErrorMessage.Should().Be("The download URL is not valid.");
        }

        #endregion
    }
}
