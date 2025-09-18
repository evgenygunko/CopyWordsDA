using AutoFixture;
using CopyWords.Core.Services;
using FluentAssertions;

namespace CopyWords.Core.Tests.Services
{
    [TestClass]
    public class NavigationHistoryTests
    {
        private readonly Fixture _fixture = FixtureFactory.CreateFixture();

        #region Tests for Push

        [TestMethod]
        public void Push_WhenMultipleWords_ShouldAddAllToHistory()
        {
            var sut = _fixture.Create<NavigationHistory>();
            sut.Push("word1", "dict1");
            sut.Push("word2", "dict2");
            sut.Push("word3", "dict3");

            sut.Count.Should().Be(3);
        }

        [TestMethod]
        public void Push_WhenSameWordTwiceInRow_ShouldNotAddDuplicate()
        {
            const string word = "testword";
            const string dictionary = "testdict";

            var sut = _fixture.Create<NavigationHistory>();
            sut.Push(word, dictionary);
            sut.Push(word, dictionary);

            sut.Count.Should().Be(1);
        }

        [TestMethod]
        public void Push_WhenSameWordAfterOtherWord_ShouldAddBoth()
        {
            const string word1 = "testword1";
            const string word2 = "testword2";
            const string dictionary = "testdict";

            var sut = _fixture.Create<NavigationHistory>();
            sut.Push(word1, dictionary);
            sut.Push(word2, dictionary);
            sut.Push(word1, dictionary); // Should be added since it's not the same as the previous

            sut.Count.Should().Be(3);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Push_WhenWordIsNullOrEmpty_ShouldNotAddToHistory(string word)
        {
            var sut = _fixture.Create<NavigationHistory>();
            sut.Push(word, "dictionary");

            sut.Count.Should().Be(0);
        }

        #endregion

        #region Tests for Pop

        [TestMethod]
        public void Pop_Should_ReturnLastAddedItem()
        {
            const string word1 = "first";
            const string word2 = "second";
            const string dictionary = "testdict";

            var sut = _fixture.Create<NavigationHistory>();
            sut.Push(word1, dictionary);
            sut.Push(word2, dictionary);

            NavigationEntry result = sut.Pop();

            result.Word.Should().Be(word2);
            result.Dictionary.Should().Be(dictionary);
            sut.Count.Should().Be(1);
        }

        #endregion

        #region Tests for Clear

        [TestMethod]
        public void Clear_WhenItems_ShouldRemoveAllItems()
        {
            var sut = _fixture.Create<NavigationHistory>();
            sut.Push("word1", "dict1");
            sut.Push("word2", "dict2");
            sut.Push("word3", "dict3");

            sut.Clear();

            sut.Count.Should().Be(0);
        }

        #endregion

        #region Tests for CanNavigateBack

        [TestMethod]
        public void CanNavigateBack_WhenEmpty_ReturnsFalse()
        {
            var sut = _fixture.Create<NavigationHistory>();
            sut.CanNavigateBack.Should().BeFalse();
        }

        [TestMethod]
        public void CanNavigateBack_WhenOneItem_ReturnsFalse()
        {
            const string searchWord = "testword";

            var sut = _fixture.Create<NavigationHistory>();
            sut.Push(searchWord, "dictionary");

            sut.CanNavigateBack.Should().BeFalse();
        }

        [TestMethod]
        public void CanNavigateBack_WhenMultipleItems_ReturnsTrue()
        {
            const string word1 = "word1";
            const string word2 = "word2";

            var sut = _fixture.Create<NavigationHistory>();
            sut.Push(word1, "dictionary");
            sut.Push(word2, "dictionary");

            sut.CanNavigateBack.Should().BeTrue();
        }

        #endregion
    }
}