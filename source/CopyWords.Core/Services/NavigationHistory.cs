namespace CopyWords.Core.Services
{
    public record struct NavigationEntry(string Word, string Dictionary);

    public interface INavigationHistory
    {
        int Count { get; }

        bool CanNavigateBack { get; }

        void Push(string word, string dictionary);

        NavigationEntry Pop();

        void Clear();
    }

    public class NavigationHistory : INavigationHistory
    {
        private readonly Stack<NavigationEntry> _navigationHistoryStack = new Stack<NavigationEntry>();

        public int Count => _navigationHistoryStack.Count;

        public bool CanNavigateBack => _navigationHistoryStack.Count > 1;

        public void Push(string word, string dictionary)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            // Don't add the same word twice in a row
            if (_navigationHistoryStack.Count == 0 || _navigationHistoryStack.Peek().Word != word)
            {
                _navigationHistoryStack.Push(new NavigationEntry(word, dictionary));
            }
        }

        public NavigationEntry Pop() => _navigationHistoryStack.Pop();

        public void Clear()
        {
            _navigationHistoryStack.Clear();
        }
    }
}
