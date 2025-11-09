namespace CopyWords.Core.Exceptions
{
    public class LaunchDarklyInitializationException : Exception
    {
        public LaunchDarklyInitializationException()
        {
        }

        public LaunchDarklyInitializationException(string message)
            : base(message)
        {
        }

        public LaunchDarklyInitializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
