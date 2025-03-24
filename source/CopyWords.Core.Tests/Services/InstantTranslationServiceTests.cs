using CopyWords.Core.Services;
using FluentAssertions;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class InstantTranslationServiceTests
    {
        [TestMethod]
        public void GetTextAndClear_Should_ReturnCurrentValueAndThenClear()
        {
            var sut = new InstantTranslationService();
            sut.SetText("abcdef");

            sut.GetTextAndClear().Should().Be("abcdef");
            sut.GetTextAndClear().Should().BeNull();
        }
    }
}
