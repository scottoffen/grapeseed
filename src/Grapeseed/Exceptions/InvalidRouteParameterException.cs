namespace Grapevine.Exceptions;

public class InvalidRouteParameterException : GrapevineException
{
    public InvalidRouteParameterException() : base("Invalid route parameter") { }

    public InvalidRouteParameterException(string message) : base($"Invalid route parameter: {message}") { }
}
