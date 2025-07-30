using AutoFixture;
using CopyWords.Core.Models;
using CopyWords.Core.Services;
using CopyWords.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace CopyWords.Core.Tests.ViewModels
{
    [TestClass]
    public class HeadwordViewModelTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for Constructor

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Constructor_Should_UpdateProperties(bool showCopyButtons)
        {
            var headword = new Headword("Grillspyd", "Kebabs", "Шашлыки");

            var sut = new HeadwordViewModel(headword, SourceLanguage.Danish, showCopyButtons);

            sut.Original.Should().Be("Grillspyd");

            // we also make first letter lower case
            sut.English.Should().Be("kebabs");
            sut.Russian.Should().Be("шашлыки");

            sut.ShowCopyButtons.Should().Be(showCopyButtons);

            if (showCopyButtons)
            {
                sut.BorderPadding.Should().BeEquivalentTo(new Thickness(0));
            }
            else
            {
                sut.BorderPadding.Should().BeEquivalentTo(new Thickness(5, 3, 5, 5));
            }
        }

        [TestMethod]
        public void Update_WhenSpanishDictionarySelected_SetsShowEnglishTranslationToFalse()
        {
            var headword = new Headword("Casa", "House", "Дом");

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Spanish.ToString();

            var ssettingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            ssettingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new HeadwordViewModel(headword, SourceLanguage.Spanish, true);

            sut.ShowEnglishTranslation.Should().BeFalse();
        }

        [TestMethod]
        public void Update_WhenDanishDictionarySelected_SetsShowEnglishTranslationToTrue()
        {
            var headword = new Headword("Grillspyd", "Kebabs", "Шашлыки");

            AppSettings appSettings = _fixture.Create<AppSettings>();
            appSettings.SelectedParser = SourceLanguage.Danish.ToString();

            var ssettingsServiceMock = _fixture.Freeze<Mock<ISettingsService>>();
            ssettingsServiceMock.Setup(x => x.LoadSettings()).Returns(appSettings);

            var sut = new HeadwordViewModel(headword, SourceLanguage.Danish, true);

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
