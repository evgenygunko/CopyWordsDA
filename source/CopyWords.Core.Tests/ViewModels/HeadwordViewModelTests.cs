using AutoFixture;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class HeadwordViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for Update

        [TestMethod]
        public void Update_Should_UpdateProperties()
        {
            var headword = new Headword("Grillspyd", "Kebabs", "Шашлыки");

            var sut = _fixture.Create<HeadwordViewModel>();
            sut.Update(headword);

            sut.Original.Should().Be("Grillspyd");

            // we also make first letter lower case
            sut.English.Should().Be("kebabs");
            sut.Russian.Should().Be("шашлыки");
        }

        #endregion

        #region Tests for FirstLetterToLower

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void FirstLetterToLower_WhenInputIsNullOrEmpty_ReturnsAsIs(string input)
        {
            HeadwordViewModel.FirstLetterToLower(input).Should().Be(input);
        }

        [DataTestMethod]
        [DataRow("акула")]
        [DataRow("shark")]
        public void FirstLetterToLower_WhenFirstLetterIsLowerCase_ReturnsAsIs(string input)
        {
            HeadwordViewModel.FirstLetterToLower(input).Should().Be(input);
        }

        [TestMethod]
        public void FirstLetterToLower_WhenEnglishAndFirstLetterIsUpperCase_ChangesToLowerCase()
        {
            HeadwordViewModel.FirstLetterToLower("На полном ходу").Should().Be("на полном ходу");
        }

        [TestMethod]
        public void FirstLetterToLower_WhenRussianAndFirstLetterIsUpperCase_ChangesToLowerCase()
        {
            HeadwordViewModel.FirstLetterToLower("At full speed").Should().Be("at full speed");
        }

        #endregion
    }
}
