namespace CopyWords.Parsers.Exceptions
{
    public class PageParserException : Exception
    {
        public PageParserException()
        {
        }

        public PageParserException(string message)
            : base(message)
        {
        }

        public PageParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
