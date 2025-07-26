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
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for LookUpWordAsync

        [TestMethod]
        public async Task LookUpWordAsync_WhenSuccess_ReturnsWordModel()
        {
            var wordModel = _fixture.Create<WordModel>();
            var json = JsonConvert.SerializeObject(wordModel);
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, json);

            var options = new Options(SourceLanguage.Danish, "http://fake-url");

            var sut = new TranslationsService(httpClient);
            var result = await sut.LookUpWordAsync("testword", options);

            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenWordIsNullOrEmpty_ThrowsArgumentException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient);
            var options = new Options(SourceLanguage.Danish, "http://fake-url");

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => sut.LookUpWordAsync(null!, options));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => sut.LookUpWordAsync("", options));
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenOptionsIsNull_ThrowsArgumentNullException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => sut.LookUpWordAsync("testword", null!));
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenApiUrlIsNullOrEmpty_ThrowsArgumentException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var sut = new TranslationsService(httpClient);
            var optionsNull = new Options(SourceLanguage.Danish, null!);
            var optionsEmpty = new Options(SourceLanguage.Danish, "");

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => sut.LookUpWordAsync("testword", optionsNull));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => sut.LookUpWordAsync("testword", optionsEmpty));
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            var errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);
            var sut = new TranslationsService(httpClient);
            var options = new Options(SourceLanguage.Danish, "http://fake-url");

            var act = async () => await sut.LookUpWordAsync("testword", options);
            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task LookUpWordAsync_WhenOtherErrors_ThrowsServerErrorException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");
            var sut = new TranslationsService(httpClient);
            var options = new Options(SourceLanguage.Danish, "http://fake-url");

            var act = async () => await sut.LookUpWordAsync("testword", options);
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
            var sut = new TranslationsService(httpClient);

            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<TranslationInput>());

            result.Should().NotBeNull();
            result!.Word.Should().Be(wordModel.Word);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenNotFound_ReturnsNull()
        {
            var errorMsg = "not found";
            var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, errorMsg);
            var sut = new TranslationsService(httpClient);

            var result = await sut.TranslateAsync("http://fake-url", _fixture.Create<TranslationInput>());

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task TranslateAsync_WhenBadRequest_ThrowsInvalidInputException()
        {
            var errorMsg = "Bad input error message";
            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorMsg);
            var sut = new TranslationsService(httpClient);

            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<TranslationInput>());
            await act.Should().ThrowAsync<InvalidInputException>()
                .WithMessage(errorMsg);
        }

        [TestMethod]
        public async Task TranslateAsync_WhenOtherErrors_ThrowsServerErrorException()
        {
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "Server error");
            var sut = new TranslationsService(httpClient);

            var act = async () => await sut.TranslateAsync("http://fake-url", _fixture.Create<TranslationInput>());
            await act.Should().ThrowAsync<ServerErrorException>()
                .WithMessage("The server returned the error 'InternalServerError'.");
        }

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

        #endregion
    }
}
