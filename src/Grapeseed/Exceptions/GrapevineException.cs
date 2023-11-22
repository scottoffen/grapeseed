namespace Grapevine.Exceptions;

public abstract class GrapevineException : Exception
{
    protected GrapevineException(string message) : base(message) { }
}
