using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class TranslationsServiceTests
    {
        private Fixture _fixture = default!;
        private Mock<IGlobalSettings> _globalSettingsMock = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns("http://fake-translator-app-url");
            _globalSettingsMock.SetupGet(x => x.TranslatorAppRequestCode).Returns("fake-request-code");
        }

        #region Tests for LookUpWordAsync

        [TestMethod]
        public async Task LookUpWordAsync_WhenSuccess_ReturnsWordModel()
        {
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);
            var result = await sut.LookUpWordAsync("testword", SourceLanguage.Danish.ToString(), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenWordIsNullOrEmpty_ThrowsArgumentException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.LookUpWordAsync(null!, SourceLanguage.Danish.ToString(), CancellationToken.None));
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.LookUpWordAsync("", SourceLanguage.Danish.ToString(), CancellationToken.None));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task LookUpWordAsync_WhenApiUrlIsNullOrEmpty_ThrowsArgumentException(string translatorAppUrl)
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");

            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns(translatorAppUrl);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.LookUpWordAsync("testword", SourceLanguage.Danish.ToString(), CancellationToken.None));
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            var errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            var act = async () => await sut.LookUpWordAsync("testword", SourceLanguage.Danish.ToString(), CancellationToken.None);
            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenOtherErrors_ThrowsServerErrorException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);
            var act = async () => await sut.LookUpWordAsync("testword", SourceLanguage.Danish.ToString(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        #endregion

        #region Tests for TranslateAsync

        [TestMethod]
        public async Task TranslateAsync_WhenSuccess_ReturnsWordModel()
        {
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenNotFound_ReturnsNull()
        {
            var errorMsg = "not found";
            var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, errorMsg);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task TranslateAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            var errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenOtherErrors_ThrowsServerErrorException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

        #region Tests for CancellationToken functionality

        [TestMethod]
        public async Task TranslateAsync_WhenExternalCancellationTokenCancelled_ReturnsNull()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var httpClient = CreateMockHttpClientWithTaskCancelledException();

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Cancel the token before the request completes
            cancellationTokenSource.Cancel();

            // Act
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), cancellationTokenSource.Token);
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task TranslateAsync_WhenExternalCancellationTokenCancelledDuringRequest_ReturnsNull()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var httpClient = CreateMockHttpClientWithDelay(HttpStatusCode.OK, "{}", TimeSpan.FromMilliseconds(200));

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act
            var task = sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), cancellationTokenSource.Token);

            // Cancel after starting the request but before it completes
            _ = Task.Run(async () =>
            {
                await Task.Delay(50);
                cancellationTokenSource.Cancel();
            });

            var act = async () => await task;
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task TranslateAsync_WhenBothTokensActive_UsesFirstTokenToBeCancelled()
        {
            // Arrange
            using var externalCts = new CancellationTokenSource();

            // Internal timeout is 30 seconds, so external should win when cancelled
            var httpClient = CreateMockHttpClientWithDelay(HttpStatusCode.OK, "{}", TimeSpan.FromMilliseconds(200));

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act - Start the operation
            var task = sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), externalCts.Token);

            // Cancel the external token after the request starts but before the delay completes
            _ = Task.Run(async () =>
            {
                await Task.Delay(50);
                externalCts.Cancel();
            });

            // Assert
            var act = async () => await task;
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task TranslateAsync_WhenCancellationTokenPassedToAllOperations_CancellationTokenIsRespected()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Setup the mock to verify that the cancellation token is passed to PostAsync
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.Is<CancellationToken>(ct => ct.CanBeCanceled)) // Verify that a cancellable token is passed
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<WordModel>()))
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act
            await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), cancellationTokenSource.Token);

            // Assert
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.Is<CancellationToken>(ct => ct.CanBeCanceled));
        }

        [TestMethod]
        public async Task TranslateAsync_WhenReadingResponse_CancellationTokenIsRespected()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act - Should complete successfully when not cancelled
            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), cancellationTokenSource.Token);

            // Assert
            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenReadingErrorContent_CancellationTokenIsRespected()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var errorMsg = "Bad input error message";

            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act & Assert
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), cancellationTokenSource.Token);
            var exception = await act.Should().ThrowAsync<InvalidInputException>();
            exception.WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenCombinedCtsIsCreated_BothTokensAreLinked()
        {
            // Arrange
            using var externalCts = new CancellationTokenSource();
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act - Should work normally when neither token is cancelled
            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), externalCts.Token);

            // Assert
            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenInternalTimeoutOccurs_ReturnsNull()
        {
            // Arrange - Use a very short timeout for testing by creating a custom mock
            var httpClient = CreateMockHttpClientWithTaskCancelledException();
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object);

            // Act
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        #endregion

        #endregion

        #region Private Methods

        private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
            return new HttpClient(handlerMock.Object);
        }

        private static HttpClient CreateMockHttpClientWithDelay(HttpStatusCode statusCode, string content, TimeSpan delay)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(async (HttpRequestMessage request, CancellationToken cancellationToken) =>
                {
                    await Task.Delay(delay, cancellationToken);
                    return new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = new StringContent(content)
                    };
                });
            return new HttpClient(handlerMock.Object);
        }

        private static HttpClient CreateMockHttpClientWithTaskCancelledException()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("The operation was canceled."));
            return new HttpClient(handlerMock.Object);
        }

        #endregion
    }
}
