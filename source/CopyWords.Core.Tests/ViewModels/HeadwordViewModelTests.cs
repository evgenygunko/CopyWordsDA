using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using CopyWords.Parsers.Models;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class HeadwordViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for Constructor

        [TestMethod]
        public void Constructor_Should_UpdateProperties()
        {
            var headword = new Headword("Grillspyd", "Kebabs", "Шашлыки");

            var sut = new HeadwordViewModel(headword, SourceLanguage.Danish);

            sut.Original.Should().Be("Grillspyd");

            // we also make first letter lower case
            sut.English.Should().Be("kebabs");
            sut.Russian.Should().Be("шашлыки");
        }

        [TestMethod]
        public void Update_WhenSpanishDisctionarySelectes_SetsShowEnglishTranslationToFalse()
        {
            var headword = new Headword("Casa", "House", "Дом");

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Spanish.ToString();

            var ssettingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            ssettingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new HeadwordViewModel(headword, SourceLanguage.Spanish);

            sut.ShowEnglishTranslation.Should().BeFalse();
        }

        [TestMethod]
        public void Update_WhenDanishDisctionarySelectes_SetsShowEnglishTranslationToTrue()
        {
            var headword = new Headword("Grillspyd", "Kebabs", "Шашлыки");

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Danish.ToString();

            var ssettingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            ssettingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new HeadwordViewModel(headword, SourceLanguage.Danish);

            sut.ShowEnglishTranslation.Should().BeTrue();
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
