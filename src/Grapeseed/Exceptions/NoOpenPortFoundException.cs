namespace Grapevine.Exceptions;

public class NoOpenPortFoundException : GrapevineException
{
    /// <summary>
    /// Error message when no open port is found in the specified range.
    /// </summary>
    /// <value></value>
    public const string NoOpenPortFoundMsg = "No local open ports found in range {0} - {1}";

    public NoOpenPortFoundException(int startIndex, int endIndex) : base(string.Format(NoOpenPortFoundMsg, startIndex, endIndex)) { }
}
