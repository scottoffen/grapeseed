namespace Grapevine.Exceptions;

public abstract class GrapevineException : Exception
{
    public GrapevineException(string message) : base(message) { }
}
