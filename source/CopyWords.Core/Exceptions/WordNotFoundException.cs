namespace CopyWords.Core.Exceptions
{
    public class WordNotFoundException : Exception
    {
        public WordNotFoundException()
        {
            SearchedWord = string.Empty;
        }

        public WordNotFoundException(string searchedWord)
            : base($"Could not find a translation for '{searchedWord}'")
        {
            SearchedWord = searchedWord;
        }

        public WordNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
            SearchedWord = string.Empty;
        }

        public string SearchedWord { get; }
    }
}
