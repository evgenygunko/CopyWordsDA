namespace CopyWords.Core.Exceptions
{
    public class AnkiDroidFieldsNotFoundException : Exception
    {
        public AnkiDroidFieldsNotFoundException()
        {
        }

        public AnkiDroidFieldsNotFoundException(string message)
            : base(message)
        {
        }

        public AnkiDroidFieldsNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
