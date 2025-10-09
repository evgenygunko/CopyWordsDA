using AutoFixture;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class LastCrashViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Constructor Tests

        [TestMethod]
        public void Constructor_Should_InitializeProperties()
        {
            // Arrange
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            // Act
            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Assert
            sut.ExceptionName.Should().Be(string.Empty);
            sut.ErrorMessage.Should().Be(string.Empty);
            sut.CrashTime.Should().Be(string.Empty);
            sut.StackTrace.Should().Be(string.Empty);
        }

        #endregion

        #region GetCrashDumpInfo Command Tests

        [TestMethod]
        public void GetCrashDumpInfo_Should_LoadCrashDataFromPreferences()
        {
            // Arrange
            var testCrashTime = 1672531200000L; // January 1, 2023, 00:00:00 UTC
            var testExceptionName = "System.ArgumentNullException";
            var testErrorMessage = "Value cannot be null. (Parameter 'value')";
            var testStackTrace = "   at TestClass.TestMethod() in TestFile.cs:line 42";

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("LastCrashTime", It.IsAny<long>(), It.IsAny<string>())).Returns(testCrashTime);
            preferencesMock.Setup(x => x.Get("LastCrashException", It.IsAny<string>(), It.IsAny<string>())).Returns(testExceptionName);
            preferencesMock.Setup(x => x.Get("LastCrashMessage", It.IsAny<string>(), It.IsAny<string>())).Returns(testErrorMessage);
            preferencesMock.Setup(x => x.Get("LastCrashStackTrace", It.IsAny<string>(), It.IsAny<string>())).Returns(testStackTrace);

            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Act
            sut.GetCrashDumpInfo();

            // Assert
            sut.ExceptionName.Should().Be(testExceptionName);
            sut.ErrorMessage.Should().Be(testErrorMessage);
            sut.StackTrace.Should().Be(testStackTrace);

            // Verify the crash time is converted correctly from Unix timestamp
            var expectedDateTime = DateTimeOffset.FromUnixTimeMilliseconds(testCrashTime).LocalDateTime;
            sut.CrashTime.Should().Be(expectedDateTime.ToString());

            // Verify preferences were called with correct keys
            preferencesMock.Verify(x => x.Get("LastCrashTime", It.IsAny<long>(), It.IsAny<string>()), Times.Once);
            preferencesMock.Verify(x => x.Get("LastCrashException", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            preferencesMock.Verify(x => x.Get("LastCrashMessage", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            preferencesMock.Verify(x => x.Get("LastCrashStackTrace", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void GetCrashDumpInfo_Should_HandleNullValues()
        {
            // Arrange
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("LastCrashTime", It.IsAny<long>(), It.IsAny<string>())).Returns(1672531200000L);
            preferencesMock.Setup(x => x.Get("LastCrashException", It.IsAny<string>(), It.IsAny<string>())).Returns((string)null!);
            preferencesMock.Setup(x => x.Get("LastCrashMessage", It.IsAny<string>(), It.IsAny<string>())).Returns((string)null!);
            preferencesMock.Setup(x => x.Get("LastCrashStackTrace", It.IsAny<string>(), It.IsAny<string>())).Returns((string)null!);

            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Act
            sut.GetCrashDumpInfo();

            // Assert
            // The properties should handle null values gracefully
            sut.ExceptionName.Should().BeNull();
            sut.ErrorMessage.Should().BeNull();
            sut.StackTrace.Should().BeNull();
        }

        [TestMethod]
        public void GetCrashDumpInfo_Should_FormatCrashTimeCorrectly()
        {
            // Arrange
            var testCrashTime = 1672531200000L; // January 1, 2023, 00:00:00 UTC
            var expectedDateTime = DateTimeOffset.FromUnixTimeMilliseconds(testCrashTime).LocalDateTime;

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Get("LastCrashTime", It.IsAny<long>(), It.IsAny<string>())).Returns(testCrashTime);

            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Act
            sut.GetCrashDumpInfo();

            // Assert
            sut.CrashTime.Should().Be(expectedDateTime.ToString());
        }

        #endregion

        #region CloseDialogAsync Command Tests

        [TestMethod]
        public async Task CloseDialogAsync_Should_SetLastCrashHandledPreference()
        {
            // Arrange
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Act
            await sut.CloseDialogAsync();

            // Assert
            preferencesMock.Verify(x => x.Set("LastCrashHandled", true, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task CloseDialogAsync_Should_CallPopModalAsync()
        {
            // Arrange
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Act
            await sut.CloseDialogAsync();

            // Assert
            shellServiceMock.Verify(x => x.PopModalAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CloseDialogAsync_Should_SetPreferenceBeforeClosingModal()
        {
            // Arrange
            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var callOrder = new List<string>();

            preferencesMock.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Callback(() => callOrder.Add("SetPreference"));

            shellServiceMock.Setup(x => x.PopModalAsync())
                .Callback(() => callOrder.Add("PopModal"))
                .Returns(Task.CompletedTask);

            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object);

            // Act
            await sut.CloseDialogAsync();

            // Assert
            callOrder.Should().HaveCount(2);
            callOrder[0].Should().Be("SetPreference");
            callOrder[1].Should().Be("PopModal");
        }

        #endregion
    }
}