namespace CopyWords.Core.Exceptions
{
    public class AnkiNoteExistsException : Exception
    {
        public AnkiNoteExistsException()
        {
        }

        public AnkiNoteExistsException(string message)
            : base(message)
        {
        }

        public AnkiNoteExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
