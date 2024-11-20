namespace Grapeseed;

public static class GlobalResponseHeaderExtensions
{
    /// <summary>
    /// Add a new header to the list of global response headers
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Add(this IList<GlobalResponseHeaders> headers, string key, string value)
    {
        headers.Add(new GlobalResponseHeaders(key, value));
    }
}
