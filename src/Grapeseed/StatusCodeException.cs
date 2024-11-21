namespace Grapevine;

public class StatusCodeException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public StatusCodeException(HttpStatusCode statusCode) : base()
    {
        StatusCode = statusCode;
    }

    public StatusCodeException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
