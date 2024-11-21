namespace Grapevine;

/// <summary>
/// Represents a global response header
/// </summary>
public struct GlobalResponseHeaders
{
    /// <summary>
    /// Get or set the name of the header
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Get or set the default value of the header
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Get or set a boolean value that represents whether or not to suppress the header
    /// </summary>
    public bool Suppress { get; set; }

    public GlobalResponseHeaders(string name, string defaultValue, bool suppress = false)
    {
        Name = name;
        Value = defaultValue;
        Suppress = suppress;
    }
}
