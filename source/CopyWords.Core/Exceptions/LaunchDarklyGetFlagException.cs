namespace CopyWords.Core.Exceptions
{
    public class LaunchDarklyGetFlagException : Exception
    {
        public LaunchDarklyGetFlagException()
        {
        }

        public LaunchDarklyGetFlagException(string message)
            : base(message)
        {
        }

        public LaunchDarklyGetFlagException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
