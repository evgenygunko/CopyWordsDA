// Ignore Spelling: Snackbar Snackbars

using AutoFixture;
using CommunityToolkit.Maui.Core;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class ConnectivityServiceTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Fixture _fixture;
        private Mock<IDeviceInfo> _deviceInfoMock;
        private Mock<ISnackbar> _snackbarNoConnectionMock;
        private Mock<ISnackbar> _snackbarConnectionRestoredMock;
        private Mock<ISnackbarService> _snackbarServiceMock;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _deviceInfoMock = _fixture.Freeze<Mock<IDeviceInfo>>();
            _deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.Android);

            _snackbarNoConnectionMock = new Mock<ISnackbar>();
            _snackbarNoConnectionMock.Setup(x => x.Show(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _snackbarNoConnectionMock.Setup(x => x.Dismiss(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _snackbarConnectionRestoredMock = new Mock<ISnackbar>();
            _snackbarConnectionRestoredMock.Setup(x => x.Show(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _snackbarConnectionRestoredMock.Setup(x => x.Dismiss(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _snackbarServiceMock = _fixture.Freeze<Mock<ISnackbarService>>();
            _snackbarServiceMock.Setup(x => x.Make(
                    "No internet connection",
                    null,
                    "Dismiss",
                    TimeSpan.FromMinutes(3),
                    It.IsAny<SnackbarOptions>()))
                .Returns(_snackbarNoConnectionMock.Object)
                .Verifiable();

            _snackbarServiceMock.Setup(x => x.Make(
                    "Connection restored",
                    null,
                    "Dismiss",
                    TimeSpan.FromSeconds(3),
                    It.IsAny<SnackbarOptions>()))
                .Returns(_snackbarConnectionRestoredMock.Object)
                .Verifiable();
        }

        #region Tests for UpdateConnectivitySnackbarAsync

        [TestMethod]
        public async Task UpdateConnectivitySnackbar_WhenPlatformIsNotAndroid_DoesNotShowSnackbar()
        {
            // Arrange
            _deviceInfoMock.Setup(x => x.Platform).Returns(DevicePlatform.WinUI);

            var sut = new ConnectivityService(_deviceInfoMock.Object, _snackbarServiceMock.Object);

            // Act
            await sut.UpdateConnectivitySnackbarAsync(true, CancellationToken.None);

            // Assert
            _snackbarServiceMock.Verify(x => x.Make(
                It.IsAny<string>(),
                It.IsAny<Action?>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<SnackbarOptions>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateConnectivitySnackbar_WhenPlatformIsAndroidAndNoConnection_ShowsNoConnectionSnackbar()
        {
            // Arrange
            var sut = new ConnectivityService(_deviceInfoMock.Object, _snackbarServiceMock.Object);

            // Act
            await sut.UpdateConnectivitySnackbarAsync(false, CancellationToken.None);

            // Assert
            _snackbarServiceMock.Verify();
            _snackbarConnectionRestoredMock.Verify(x => x.Dismiss(It.IsAny<CancellationToken>()), Times.Once);
            _snackbarNoConnectionMock.Verify(x => x.Show(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConnectivitySnackbar_WhenPlatformIsAndroidAndHasConnection_ShowsConnectionRestoredSnackbar()
        {
            // Arrange
            var sut = new ConnectivityService(_deviceInfoMock.Object, _snackbarServiceMock.Object);

            // Act
            await sut.UpdateConnectivitySnackbarAsync(true, CancellationToken.None);

            // Assert
            _snackbarServiceMock.Verify();
            _snackbarNoConnectionMock.Verify(x => x.Dismiss(It.IsAny<CancellationToken>()), Times.Once);
            _snackbarConnectionRestoredMock.Verify(x => x.Show(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConnectivitySnackbar_CreatesSnackbarsOnlyOnce()
        {
            // Arrange
            var sut = new ConnectivityService(_deviceInfoMock.Object, _snackbarServiceMock.Object);

            // Act
            await sut.UpdateConnectivitySnackbarAsync(false, CancellationToken.None);
            await sut.UpdateConnectivitySnackbarAsync(true, CancellationToken.None);
            await sut.UpdateConnectivitySnackbarAsync(false, CancellationToken.None);

            // Assert - Snackbars should be created only once (on first call)
            _snackbarServiceMock.Verify(x => x.Make(
                "No internet connection",
                null,
                "Dismiss",
                TimeSpan.FromMinutes(3),
                It.IsAny<SnackbarOptions>()), Times.Once);

            _snackbarServiceMock.Verify(x => x.Make(
                "Connection restored",
                null,
                "Dismiss",
                TimeSpan.FromSeconds(3),
                It.IsAny<SnackbarOptions>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConnectivitySnackbar_WhenCancellationTokenIsCancelled_DoesNotCallSnackbarMethods()
        {
            // Arrange
            var sut = new ConnectivityService(_deviceInfoMock.Object, _snackbarServiceMock.Object);

            // First call to initialize the snackbars
            await sut.UpdateConnectivitySnackbarAsync(false, CancellationToken.None);

            // Reset mock invocations to start fresh
            _snackbarNoConnectionMock.Reset();
            _snackbarConnectionRestoredMock.Reset();

            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            await sut.UpdateConnectivitySnackbarAsync(true, cts.Token);

            // Assert - No snackbar methods should be called when token is cancelled
            _snackbarNoConnectionMock.Verify(x => x.Dismiss(It.IsAny<CancellationToken>()), Times.Never);
            _snackbarNoConnectionMock.Verify(x => x.Show(It.IsAny<CancellationToken>()), Times.Never);
            _snackbarConnectionRestoredMock.Verify(x => x.Dismiss(It.IsAny<CancellationToken>()), Times.Never);
            _snackbarConnectionRestoredMock.Verify(x => x.Show(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
