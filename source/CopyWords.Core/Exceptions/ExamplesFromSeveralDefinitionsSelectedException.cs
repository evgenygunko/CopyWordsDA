namespace CopyWords.Core.Exceptions
{
    public class ExamplesFromSeveralDefinitionsSelectedException : Exception
    {
        public ExamplesFromSeveralDefinitionsSelectedException()
        {
        }

        public ExamplesFromSeveralDefinitionsSelectedException(string message)
            : base(message)
        {
        }

        public ExamplesFromSeveralDefinitionsSelectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
