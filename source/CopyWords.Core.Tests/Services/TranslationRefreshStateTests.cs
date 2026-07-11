using CopyWords.Core.Services;
using FluentAssertions;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class TranslationRefreshStateTests
    {
        [TestMethod]
        public void ConsumeRefreshRequired_WhenNoValueWasSet_DefaultsToTrue()
        {
            var sut = new TranslationRefreshState();

            sut.ConsumeRefreshRequired().Should().BeTrue();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ConsumeRefreshRequired_WhenValueWasSet_ReturnsItOnce(bool refreshRequired)
        {
            var sut = new TranslationRefreshState();
            sut.SetRefreshRequired(refreshRequired);

            sut.ConsumeRefreshRequired().Should().Be(refreshRequired);
            sut.ConsumeRefreshRequired().Should().BeTrue();
        }
    }
}
