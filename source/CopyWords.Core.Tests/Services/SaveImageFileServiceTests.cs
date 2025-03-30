using AutoFixture;
using CopyWords.Core.Services;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class SaveImageFileServiceTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        [TestMethod]
        public async Task SaveImageFileAsync_Should_CallCopyFileToAnkiFolderAsync()
        {
            string filePath = _fixture.Create<string>();

            var fileDownloaderServiceMock = _fixture.Freeze<Mock<IFileDownloaderService>>();
            fileDownloaderServiceMock.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(filePath)
                .Verifiable();
            fileDownloaderServiceMock.Setup(x => x.CopyFileToAnkiFolderAsync(It.IsAny<string>()))
                .ReturnsAsync(true)
                .Verifiable();

            var sut = _fixture.Create<SaveImageFileService>();
            sut.IsUnitTest = true;

            bool result = await sut.SaveImageFileAsync(It.IsAny<string>(), It.IsAny<string>());

            result.Should().BeTrue();
            fileDownloaderServiceMock.Verify();
        }
    }
}
