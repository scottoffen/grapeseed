using System.Text.RegularExpressions;

namespace Grapevine;

public interface IRouteTemplate
{
    /// <summary>
    /// The pattern to match the route
    /// </summary>
    Regex Pattern { get; }

    /// <summary>
    /// The pattern keys to extract from the route
    /// </summary>
    List<string> PatternKeys { get; }

    /// <summary>
    /// Returns true if the endpoint matches the route
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    bool Matches(string endpoint);

    /// <summary>
    /// Parses the endpoint and returns the values
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    IDictionary<string, string> ParseEndpoint(string endpoint);
}
