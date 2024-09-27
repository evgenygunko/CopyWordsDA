namespace CopyWords.Core.Exceptions
{
    public class PrepareWordForCopyingException : Exception
    {
        public PrepareWordForCopyingException()
        {
        }

        public PrepareWordForCopyingException(string message)
            : base(message)
        {
        }

        public PrepareWordForCopyingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
