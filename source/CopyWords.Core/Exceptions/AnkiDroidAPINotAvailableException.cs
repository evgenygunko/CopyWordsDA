namespace CopyWords.Core.Exceptions
{
    public class AnkiDroidAPINotAvailableException : Exception
    {
        public AnkiDroidAPINotAvailableException()
        {
        }

        public AnkiDroidAPINotAvailableException(string message)
            : base(message)
        {
        }

        public AnkiDroidAPINotAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
