namespace CopyWords.Core.Exceptions
{
    public class AnkiConnectNotRunningException : Exception
    {
        public AnkiConnectNotRunningException()
        {
        }

        public AnkiConnectNotRunningException(string message)
            : base(message)
        {
        }

        public AnkiConnectNotRunningException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
