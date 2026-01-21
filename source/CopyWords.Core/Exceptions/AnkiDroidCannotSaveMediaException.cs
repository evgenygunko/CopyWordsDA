namespace CopyWords.Core.Exceptions
{
    public class AnkiDroidCannotSaveMediaException : Exception
    {
        public AnkiDroidCannotSaveMediaException()
        {
        }

        public AnkiDroidCannotSaveMediaException(string message)
            : base(message)
        {
        }

        public AnkiDroidCannotSaveMediaException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
