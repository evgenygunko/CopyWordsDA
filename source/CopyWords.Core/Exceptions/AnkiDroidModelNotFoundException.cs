namespace CopyWords.Core.Exceptions
{
    public class AnkiDroidModelNotFoundException : Exception
    {
        public AnkiDroidModelNotFoundException()
        {
        }

        public AnkiDroidModelNotFoundException(string message)
            : base(message)
        {
        }

        public AnkiDroidModelNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
