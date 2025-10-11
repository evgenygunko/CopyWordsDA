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
            var emailServiceMock = _fixture.Freeze<Mock<IEmailService>>();
            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            // Act
            var sut = new LastCrashViewModel(preferencesMock.Object, shellServiceMock.Object, emailServiceMock.Object, clipboardServiceMock.Object, dialogServiceMock.Object);

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

            var sut = _fixture.Create<LastCrashViewModel>();

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

            var sut = _fixture.Create<LastCrashViewModel>();

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

            var sut = _fixture.Create<LastCrashViewModel>();

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

            var sut = _fixture.Create<LastCrashViewModel>();

            // Act
            await sut.CloseDialogAsync();

            // Assert
            preferencesMock.Verify(x => x.Set("LastCrashHandled", true, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task CloseDialogAsync_Should_CallPopModalAsync()
        {
            // Arrange
            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();

            var sut = _fixture.Create<LastCrashViewModel>();

            // Act
            await sut.CloseDialogAsync();

            // Assert
            shellServiceMock.Verify(x => x.PopModalAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CloseDialogAsync_Should_SetPreferenceBeforeClosingModal()
        {
            // Arrange
            var callOrder = new List<string>();

            var preferencesMock = _fixture.Freeze<Mock<IPreferences>>();
            preferencesMock.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Callback(() => callOrder.Add("SetPreference"));

            var shellServiceMock = _fixture.Freeze<Mock<IShellService>>();
            shellServiceMock.Setup(x => x.PopModalAsync())
                .Callback(() => callOrder.Add("PopModal"))
                .Returns(Task.CompletedTask);

            var sut = _fixture.Create<LastCrashViewModel>();

            // Act
            await sut.CloseDialogAsync();

            // Assert
            callOrder.Should().HaveCount(2);
            callOrder[0].Should().Be("SetPreference");
            callOrder[1].Should().Be("PopModal");
        }

        #endregion

        #region SendEmailAsync Command Tests

        [TestMethod]
        public async Task SendEmailAsync_Should_CallEmailServiceWithCrashDetails()
        {
            // Arrange
            var emailServiceMock = _fixture.Freeze<Mock<IEmailService>>();

            var sut = _fixture.Create<LastCrashViewModel>();

            // Set up test data
            sut.CrashTime = "2023-01-01 12:00:00";
            sut.ExceptionName = "System.ArgumentNullException";
            sut.ErrorMessage = "Value cannot be null. (Parameter 'value')";
            sut.StackTrace = "   at TestClass.TestMethod() in TestFile.cs:line 42";

            // Act
            await sut.SendEmailAsync();

            // Assert
            emailServiceMock.Verify(x => x.ComposeAsync(
                "CopyWords Application Crash Report",
                It.Is<string>(body =>
                    body.Contains("2023-01-01 12:00:00") &&
                    body.Contains("System.ArgumentNullException") &&
                    body.Contains("Value cannot be null. (Parameter 'value')") &&
                    body.Contains("   at TestClass.TestMethod() in TestFile.cs:line 42")),
                null), Times.Once);
        }

        [TestMethod]
        public async Task SendEmailAsync_Should_HandleEmailNotSupportedException()
        {
            // Arrange
            var emailServiceMock = _fixture.Freeze<Mock<IEmailService>>();
            emailServiceMock.Setup(x => x.ComposeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new NotSupportedException("Email is not supported on this device"));

            var sut = _fixture.Create<LastCrashViewModel>();

            // Act & Assert - Should not throw exception
            var act = async () => await sut.SendEmailAsync();
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region CopyErrorMessageAsync Command Tests

        [TestMethod]
        public async Task CopyErrorMessageAsync_Should_CopyErrorMessageToClipboard()
        {
            // Arrange
            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<LastCrashViewModel>();
            sut.ErrorMessage = "Test error message";

            // Act
            await sut.CopyErrorMessageAsync();

            // Assert
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync("Test error message"), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayToast("Error message copied to clipboard"), Times.Once);
        }

        #endregion

        #region CopyStackTraceAsync Command Tests

        [TestMethod]
        public async Task CopyStackTraceAsync_Should_CopyStackTraceToClipboard()
        {
            // Arrange
            var clipboardServiceMock = _fixture.Freeze<Mock<IClipboardService>>();
            var dialogServiceMock = _fixture.Freeze<Mock<IDialogService>>();

            var sut = _fixture.Create<LastCrashViewModel>();
            sut.StackTrace = "Test stack trace";

            // Act
            await sut.CopyStackTraceAsync();

            // Assert
            clipboardServiceMock.Verify(x => x.CopyTextToClipboardAsync("Test stack trace"), Times.Once);
            dialogServiceMock.Verify(x => x.DisplayToast("Stack trace copied to clipboard"), Times.Once);
        }

        #endregion
    }
}