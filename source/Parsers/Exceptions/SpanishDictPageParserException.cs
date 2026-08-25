namespace CopyWords.Parsers.Exceptions
{
    public class SpanishDictPageParserException : Exception
    {
        public SpanishDictPageParserException()
        {
        }

        public SpanishDictPageParserException(string message)
            : base(message)
        {
        }

        public SpanishDictPageParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
