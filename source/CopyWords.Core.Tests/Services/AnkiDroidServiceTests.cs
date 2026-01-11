using AutoFixture;
using CopyWords.Core.Services;
using FluentAssertions;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class AnkiDroidServiceTests
    {
        private Fixture _fixture = default!;

        [TestInitialize]
        public void SetUp()
        {
            _fixture = FixtureFactory.CreateFixture();
        }

        #region Tests for GetDeckNamesAsync

        [TestMethod]
        public async Task GetDeckNamesAsync_WhenCalled_ReturnsDeckNames()
        {
            // Arrange
            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            var result = await sut.GetDeckNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(["Deck 1", "Deck 2", "Deck 3"]);
        }

        #endregion

        #region Tests for GetModelNamesAsync

        [TestMethod]
        public async Task GetModelNamesAsync_WhenCalled_ReturnsModelNames()
        {
            // Arrange
            var sut = _fixture.Create<AnkiDroidService>();

            // Act
            var result = await sut.GetModelNamesAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(["Model 1", "Model 2", "Model 3", "Model 4"]);
        }

        #endregion
    }
}
