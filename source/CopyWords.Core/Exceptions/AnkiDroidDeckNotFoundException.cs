namespace CopyWords.Core.Exceptions
{
    public class AnkiDroidDeckNotFoundException : Exception
    {
        public AnkiDroidDeckNotFoundException()
        {
        }

        public AnkiDroidDeckNotFoundException(string message)
            : base(message)
        {
        }

        public AnkiDroidDeckNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
