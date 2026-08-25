using System.Net;

namespace CopyWords.Parsers.Exceptions
{
    public class ServerErrorException : Exception
    {
        public HttpStatusCode? StatusCode { get; }

        public string? RequestUrl { get; }

        public ServerErrorException()
        {
        }

        public ServerErrorException(string message)
            : base(message)
        {
        }

        public ServerErrorException(string message, HttpStatusCode statusCode, string requestUrl)
            : base(message)
        {
            StatusCode = statusCode;
            RequestUrl = requestUrl;
        }

        public ServerErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
