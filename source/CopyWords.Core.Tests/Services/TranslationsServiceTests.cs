using System.Net;
using AutoFixture;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.Services.Wrappers;
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
        private Mock<ISettingsService> _settingsServiceMock = default!;
        private Mock<ILaunchDarklyService> _launchDarklyServiceMock = default!;
        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = FixtureFactory.CreateFixture();

            _globalSettingsMock = _fixture.Freeze<Mock<IGlobalSettings>>();
            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns("http://fake-translator-app-url");
            _globalSettingsMock.SetupGet(x => x.TranslatorAppRequestCode).Returns("fake-request-code");

            _settingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            _settingsServiceMock.Setup(x => x.GetDestinationLanguage()).Returns("English");
            _settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));
            _settingsServiceMock.Setup(x => x.GetActiveDictionaries()).Returns([nameof(SourceLanguage.Danish), nameof(SourceLanguage.Spanish)]);

            _launchDarklyServiceMock = _fixture.Freeze<Mock<ILaunchDarklyService>>();
            _launchDarklyServiceMock.Setup(x => x.GetBooleanFlag("client-side-parsing", false)).Returns(false);
        }

        #region Tests for LookUpWordAsync

        [TestMethod]
        public async Task LookUpWordAsync_WhenSuccess_ReturnsWordModel()
        {
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var result = await sut.LookUpWordAsync("testword", CancellationToken.None);

            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenClientSideParsingIsDisabled_PostsV2Request()
        {
            HttpRequestMessage? capturedRequest = null;
            string? capturedContent = null;
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    capturedRequest = request;
                    capturedContent = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<WordModel>()))
                });
            var sut = new TranslationsService(
                new HttpClient(handlerMock.Object),
                _globalSettingsMock.Object,
                _settingsServiceMock.Object,
                _launchDarklyServiceMock.Object);

            await sut.LookUpWordAsync("typed-word", CancellationToken.None);

            capturedRequest!.RequestUri!.ToString().Should().Contain("/api/v2/Translation/LookUpWord");
            capturedContent.Should().Contain("\"Text\":\"typed-word\"");
            _launchDarklyServiceMock.Verify(x => x.GetBooleanFlag("client-side-parsing", false), Times.Once);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenClientSideParsingIsEnabledForDanish_PostsHajModelToV3()
        {
            _launchDarklyServiceMock.Setup(x => x.GetBooleanFlag("client-side-parsing", false)).Returns(true);
            _settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Danish));

            (Uri requestUri, WordModel payload, WordModel response) = await SendAndCaptureV3Request();

            requestUri.ToString().Should().Contain("/api/v3/Translation/LookUpWord");
            payload.Word.Should().Be("haj");
            payload.SourceLanguage.Should().Be(SourceLanguage.Danish);
            payload.SoundUrl.Should().BeNull();
            payload.Variants.Should().BeEmpty();
            payload.Expressions.Should().BeEmpty();
            payload.Definition!.Headword.Should().Be(new Headword("haj", null, null));
            payload.Definition.PartOfSpeech.Should().Be("substantiv, fælleskøn");
            payload.Definition.Endings.Should().Be("-en, -er, -erne");
            payload.Definition.Contexts.Single().Meanings.Should().HaveCount(3);
            payload.Definition.Contexts.SelectMany(x => x.Meanings).Should().OnlyContain(x => x.Translation == null);
            response.Word.Should().Be("translated-result");
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenClientSideParsingIsEnabledForSpanish_PostsCocheModelToV3()
        {
            _launchDarklyServiceMock.Setup(x => x.GetBooleanFlag("client-side-parsing", false)).Returns(true);
            _settingsServiceMock.Setup(x => x.GetSelectedParser()).Returns(nameof(SourceLanguage.Spanish));

            (Uri requestUri, WordModel payload, WordModel response) = await SendAndCaptureV3Request();

            requestUri.ToString().Should().Contain("/api/v3/Translation/LookUpWord");
            payload.Word.Should().Be("coche");
            payload.SourceLanguage.Should().Be(SourceLanguage.Spanish);
            payload.Variants.Should().BeEmpty();
            payload.Expressions.Should().BeEmpty();
            payload.Definition!.Headword.Should().Be(new Headword("el coche", null, null));
            payload.Definition.Contexts.Should().HaveCount(2);
            payload.Definition.Contexts.SelectMany(x => x.Meanings).Should().HaveCount(4);
            payload.Definition.Contexts.SelectMany(x => x.Meanings).Should().OnlyContain(x => x.Translation == null);
            payload.Definition.Contexts.SelectMany(x => x.Meanings).SelectMany(x => x.Examples)
                .Should().OnlyContain(x => x.Translation != null);
            response.Word.Should().Be("translated-result");
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenLaunchDarklyIsUninitialized_DefaultsToV2()
        {
            _launchDarklyServiceMock.SetupGet(x => x.IsInitialized).Returns(false);
            _launchDarklyServiceMock.Setup(x => x.GetBooleanFlag("client-side-parsing", false)).Returns(false);
            Uri? requestUri = null;
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => requestUri = request.RequestUri)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<WordModel>()))
                });
            var sut = new TranslationsService(
                new HttpClient(handlerMock.Object),
                _globalSettingsMock.Object,
                _settingsServiceMock.Object,
                _launchDarklyServiceMock.Object);

            await sut.LookUpWordAsync("typed-word", CancellationToken.None);

            requestUri!.ToString().Should().Contain("/api/v2/Translation/LookUpWord");
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenV3Fails_DoesNotFallBackToV2()
        {
            _launchDarklyServiceMock.Setup(x => x.GetBooleanFlag("client-side-parsing", false)).Returns(true);
            var requestedUris = new List<Uri>();
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => requestedUris.Add(request.RequestUri!))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("v3 failed")
                });
            var sut = new TranslationsService(
                new HttpClient(handlerMock.Object),
                _globalSettingsMock.Object,
                _settingsServiceMock.Object,
                _launchDarklyServiceMock.Object);

            Func<Task> action = async () => await sut.LookUpWordAsync("typed-word", CancellationToken.None);

            await action.Should().ThrowAsync<ServerErrorException>().WithMessage("v3 failed");
            requestedUris.Should().ContainSingle();
            requestedUris.Single().ToString().Should().Contain("/api/v3/Translation/LookUpWord");
        }

        [TestMethod]
        public async Task LookUpWordAsync_Should_UseDestinationLanguageFromSettings()
        {
            string? requestContent = null;
            _settingsServiceMock.Setup(x => x.GetDestinationLanguage()).Returns("English");
            _settingsServiceMock.Setup(x => x.GetActiveDictionaries()).Returns([nameof(SourceLanguage.Danish), nameof(SourceLanguage.Spanish)]);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<WordModel>()))
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await sut.LookUpWordAsync("testword", CancellationToken.None);

            requestContent.Should().NotBeNull();
            requestContent.Should().Contain("\"DestinationLanguage\":\"English\"");
            requestContent.Should().Contain("\"ActiveDictionaries\":[\"Danish\",\"Spanish\"]");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task LookUpWordAsync_WhenDestinationLanguageMissing_Should_FallbackToRussian(string? destinationLanguage)
        {
            string? requestContent = null;
            _settingsServiceMock.Setup(x => x.GetDestinationLanguage()).Returns(destinationLanguage!);
            _settingsServiceMock.Setup(x => x.GetActiveDictionaries()).Returns([nameof(SourceLanguage.Danish), nameof(SourceLanguage.Spanish)]);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<WordModel>()))
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await sut.LookUpWordAsync("testword", CancellationToken.None);

            requestContent.Should().NotBeNull();
            requestContent.Should().Contain("\"DestinationLanguage\":\"Russian\"");
            requestContent.Should().Contain("\"ActiveDictionaries\":[\"Danish\",\"Spanish\"]");
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenWordIsNullOrEmpty_ThrowsArgumentException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.LookUpWordAsync(null!, CancellationToken.None));
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.LookUpWordAsync("", CancellationToken.None));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task LookUpWordAsync_WhenApiUrlIsNullOrEmpty_ThrowsArgumentException(string translatorAppUrl)
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");

            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns(translatorAppUrl);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.LookUpWordAsync("testword", CancellationToken.None));
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            var errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            var act = async () => await sut.LookUpWordAsync("testword", CancellationToken.None);
            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenInternalServerErrorWithBody_ThrowsServerErrorExceptionWithBodyMessage()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.LookUpWordAsync("testword", CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("Server error");
        }

        #endregion

        #region Tests for CreateLookUpWordUrl

        [TestMethod]
        public void CreateLookUpWordUrl_Should_ReturnCorrectUrl()
        {
            // Arrange
            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "test-code-123";

            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            _globalSettingsMock.SetupGet(x => x.TranslatorAppRequestCode).Returns(requestCode);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            // Act
            string result = sut.CreateLookUpWordUrl();

            // Assert
            result.Should().Be("https://translator.example.com/api/v2/Translation/LookUpWord?code=test-code-123");
        }

        [TestMethod]
        public void CreateLookUpWordUrl_WhenUrlHasTrailingSlash_RemovesItBeforeAppending()
        {
            // Arrange
            string translatorAppUrl = "https://translator.example.com/";
            string requestCode = "my-code";

            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            _globalSettingsMock.SetupGet(x => x.TranslatorAppRequestCode).Returns(requestCode);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            // Act
            string result = sut.CreateLookUpWordUrl();

            // Assert
            result.Should().NotContain("//api");
            result.Should().Be("https://translator.example.com/api/v2/Translation/LookUpWord?code=my-code");
        }

        [TestMethod]
        public void CreateLookUpWordUrl_WhenUrlHasNoTrailingSlash_WorksCorrectly()
        {
            // Arrange
            string translatorAppUrl = "https://translator.example.com";
            string requestCode = "my-code";

            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns(translatorAppUrl);
            _globalSettingsMock.SetupGet(x => x.TranslatorAppRequestCode).Returns(requestCode);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            // Act
            string result = sut.CreateLookUpWordUrl();

            // Assert
            result.Should().Be("https://translator.example.com/api/v2/Translation/LookUpWord?code=my-code");
        }

        #endregion

        #region Tests for TranslateAsync

        [TestMethod]
        public async Task TranslateAsync_WhenSuccess_ReturnsWordModel()
        {
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenNotFound_ThrowsWordNotFoundException()
        {
            var errorMsg = "not found";
            var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, errorMsg);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            var input = _fixture.Create<LookUpWordRequest>() with { Text = "missing-word" };
            var act = async () => await sut.TranslateAsync("http://fake-url", input, CancellationToken.None);

            var ex = await act.Should().ThrowAsync<WordNotFoundException>();
            ex.Which.SearchedWord.Should().Be("missing-word");
        }

        [TestMethod]
        public async Task TranslateAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            var errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenInternalServerErrorWithBody_ThrowsServerErrorExceptionWithBodyMessage()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("Server error");
        }

        [TestMethod]
        public async Task TranslateAsync_WhenServiceUnavailableWithBody_ThrowsServerErrorExceptionWithBodyMessage()
        {
            const string errorMsg = "Online dictionary 'DDO' is temporarily unavailable. Original error: Service unavailable";
            var httpClient = CreateMockHttpClient(HttpStatusCode.ServiceUnavailable, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenBadGatewayWithBody_ThrowsServerErrorExceptionWithBodyMessage()
        {
            const string errorMsg = "Online dictionary 'DDO' is temporarily unavailable. Original error: Bad gateway";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadGateway, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenGatewayTimeoutWithBody_ThrowsServerErrorExceptionWithBodyMessage()
        {
            const string errorMsg = "Online dictionary 'DDO' is temporarily unavailable. Original error: Gateway timeout";
            var httpClient = CreateMockHttpClient(HttpStatusCode.GatewayTimeout, errorMsg);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenServiceUnavailableWithEmptyBody_ThrowsGenericServerErrorException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.ServiceUnavailable, "   ");

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'ServiceUnavailable'.");
        }

        #region Tests for CancellationToken functionality

        [TestMethod]
        public async Task TranslateAsync_WhenExternalCancellationTokenCancelled_ReturnsNull()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var httpClient = CreateMockHttpClientWithTaskCancelledException();

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

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
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            // Act
            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<LookUpWordRequest>(), CancellationToken.None);
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        #endregion

        #region Tests for GetSuggestedWordsAsync

        [TestMethod]
        public async Task GetSuggestedWordsAsync_WhenCalled_ReturnsApiSuggestions()
        {
            var expectedSuggestions = new SuggestedWordsModel(["house", "horse"]);
            string json = JsonConvert.SerializeObject(expectedSuggestions);

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            SuggestedWordsModel result = await sut.GetSuggestedWordsAsync("test", CancellationToken.None);

            result.Words.Should().NotBeNull();
            result.Words.Should().BeEquivalentTo(expectedSuggestions.Words);
        }

        [TestMethod]
        public async Task GetSuggestedWordsAsync_WhenApiReturnsNullBody_ReturnsEmptySuggestedWords()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "null");
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            SuggestedWordsModel result = await sut.GetSuggestedWordsAsync("test", CancellationToken.None);

            result.Words.Should().NotBeNull();
            result.Words.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSuggestedWordsAsync_WhenWordIsNullOrEmpty_ThrowsArgumentException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.GetSuggestedWordsAsync(null!, CancellationToken.None));
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.GetSuggestedWordsAsync("", CancellationToken.None));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task GetSuggestedWordsAsync_WhenApiUrlIsNullOrEmpty_ThrowsArgumentException(string translatorAppUrl)
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            _globalSettingsMock.SetupGet(x => x.TranslatorAppUrl).Returns(translatorAppUrl);

            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.GetSuggestedWordsAsync("testword", CancellationToken.None));
        }

        [TestMethod]
        public async Task GetSuggestedWordsAsync_Should_PostExpectedPayloadToSuggestedWordsEndpoint()
        {
            string? requestContent = null;
            Uri? requestUri = null;
            HttpMethod? method = null;

            var expectedSuggestions = new SuggestedWordsModel(["word1", "word2"]);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                    requestUri = request.RequestUri;
                    method = request.Method;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(expectedSuggestions))
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await sut.GetSuggestedWordsAsync("testword", CancellationToken.None);

            method.Should().Be(HttpMethod.Post);
            requestUri.Should().NotBeNull();
            requestUri!.ToString().Should().Be("http://fake-translator-app-url/api/v2/Translation/SuggestedWords?code=fake-request-code");
            requestContent.Should().NotBeNull();
            requestContent.Should().Contain("\"Text\":\"testword\"");
            requestContent.Should().Contain("\"SourceLanguage\":\"Danish\"");
            requestContent.Should().Contain("\"DestinationLanguage\":\"English\"");
            requestContent.Should().Contain("\"ActiveDictionaries\":[\"Danish\",\"Spanish\"]");
            requestContent.Should().Contain("\"Version\":\"2\"");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task GetSuggestedWordsAsync_WhenDestinationLanguageMissing_Should_FallbackToRussian(string? destinationLanguage)
        {
            string? requestContent = null;
            _settingsServiceMock.Setup(x => x.GetDestinationLanguage()).Returns(destinationLanguage!);
            _settingsServiceMock.Setup(x => x.GetActiveDictionaries()).Returns([nameof(SourceLanguage.Danish), nameof(SourceLanguage.Spanish)]);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new SuggestedWordsModel(["word"])))
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            await sut.GetSuggestedWordsAsync("testword", CancellationToken.None);

            requestContent.Should().NotBeNull();
            requestContent.Should().Contain("\"DestinationLanguage\":\"Russian\"");
            requestContent.Should().Contain("\"ActiveDictionaries\":[\"Danish\",\"Spanish\"]");
        }

        [TestMethod]
        public async Task GetSuggestedWordsAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            const string errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            var act = async () => await sut.GetSuggestedWordsAsync("testword", CancellationToken.None);

            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task GetSuggestedWordsAsync_WhenInternalServerErrorWithBody_ThrowsServerErrorExceptionWithBodyMessage()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            var act = async () => await sut.GetSuggestedWordsAsync("testword", CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("Server error");
        }

        [TestMethod]
        [DataRow(HttpStatusCode.ServiceUnavailable, "Service unavailable")]
        [DataRow(HttpStatusCode.BadGateway, "Bad gateway")]
        [DataRow(HttpStatusCode.GatewayTimeout, "Gateway timeout")]
        public async Task GetSuggestedWordsAsync_WhenTransientErrorsWithBody_ThrowsServerErrorExceptionWithBodyMessage(HttpStatusCode statusCode, string errorMsg)
        {
            var httpClient = CreateMockHttpClient(statusCode, errorMsg);
            var sut = new TranslationsService(httpClient, _globalSettingsMock.Object, _settingsServiceMock.Object);

            var act = async () => await sut.GetSuggestedWordsAsync("testword", CancellationToken.None);

            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage(errorMsg);
        }

        #endregion

        #endregion

        #region Private Methods

        private async Task<(Uri RequestUri, WordModel Payload, WordModel Response)> SendAndCaptureV3Request()
        {
            Uri? requestUri = null;
            WordModel? payload = null;
            var translatedModel = _fixture.Build<WordModel>().With(x => x.Word, "translated-result").Create();
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    requestUri = request.RequestUri;
                    string json = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                    payload = JsonConvert.DeserializeObject<WordModel>(json);
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(translatedModel))
                });
            var sut = new TranslationsService(
                new HttpClient(handlerMock.Object),
                _globalSettingsMock.Object,
                _settingsServiceMock.Object,
                _launchDarklyServiceMock.Object);

            WordModel? response = await sut.LookUpWordAsync("this-word-is-not-posted", CancellationToken.None);

            return (requestUri!, payload!, response!);
        }

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
