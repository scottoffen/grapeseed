using System.Text;
using System.Text.RegularExpressions;
using Grapevine.Exceptions;

namespace Grapevine;

/// <summary>
/// The route template determines whether or not the request matches the pattern for the route and parses out any route parameters from the path.
/// </summary>
public interface IRouteTemplate
{
    /// <summary>
    /// An enumeration of the parameter keys from the route pattern.
    /// </summary>
    /// <value></value>
    HashSet<string> Keys { get; }

    /// <summary>
    /// Returns a boolean value indicating whether the specified endpoint matches the route pattern.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    bool Matches(string endpoint);

    /// <summary>
    /// An integer representing the number of parameterized segments in the route pattern.
    /// <remarks>A lower number means that the route template has a greater specificity.</remarks>
    /// </summary>
    /// <value></value>
    int Parameters { get; }

    /// <summary>
    /// Returns a dictionary of values for each entry in Keys based on the regular expression Pattern.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    IDictionary<string, string> Parse(string endpoint);

    /// <summary>
    /// Regular expression pattern for the route, automatically generated from the route path.
    /// </summary>
    /// <value></value>
    Regex Pattern { get; }

    /// <summary>
    /// An integer representing the number segments in the route pattern
    /// </summary>
    /// <remarks>A higher number means that the route template has a greater specificity.</remarks>
    /// <value></value>
    int Segments { get; }
}

public partial class RouteTemplate : IRouteTemplate
{
    public HashSet<string> Keys { get; } = new();

    public int Parameters => Keys.Count;

    public Regex Pattern { get; protected set; } = Default;

    public int Segments { get; protected set; }

    public RouteTemplate(string pattern)
    {
        ConvertTemplateToRegex(pattern);
    }

    public bool Matches(string endpoint) => Pattern.IsMatch(endpoint);

    public IDictionary<string, string> Parse(string endpoint)
    {
        throw new NotImplementedException();
    }
}

public partial class RouteTemplate
{
    private static readonly string caseInsensitive = "(?i)";
    private static readonly string regexPrefix = $"{caseInsensitive}^";
    private static readonly string regexSuffix = "/?$";

    /// <summary>
    /// The default regular expression.
    /// </summary>
    /// <remarks>Will be used if the provided pattern is null or empty.</remarks>
    /// <returns></returns>
    public static readonly Regex Default = new(@"^.*$");

    /// <summary>
    /// Converts the parameterized pattern to a regular expression.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public void ConvertTemplateToRegex(string pattern)
    {
        // If no pattern is provided, the default pattern will be used.
        if (string.IsNullOrWhiteSpace(pattern)) return;

        // If provided pattern is a regular expression string,
        // use the pattern as a regular expression
        if (pattern.StartsWith("^"))
        {
            Pattern = new(pattern);
            return;
        }

        // Get the number of route segments
        var segments = pattern.SanitizePath().TrimStart('/').Split('/');
        Segments = segments.Length;

        // If the provided pattern contains no parameterized route
        // constraints, use a regular expression with no matching groups
        if (!pattern.Contains("{"))
        {
            Pattern = new($"{regexPrefix}{Regex.Escape(pattern)}{regexSuffix}");
            return;
        }

        // Otherwise, parse the pattern and create a regular expression
        // for each parameterized route constraint
        var builder = new StringBuilder(regexPrefix);

        segments.ToList().ForEach(segment =>
        {
            segment = segment.ToLower();
            builder.Append('/');

            // Segment is a parameterized route constraint
            if (segment.StartsWith('{') && segment.EndsWith('}'))
            {
                var routeParameter = segment.TrimStart('{').TrimEnd('}');
                if (routeParameter.Contains('{')) throw new InvalidRouteParameterException("A given route segment can only contain one route constraint");

                var parameterSegments = routeParameter.Split(':');

                var key = parameterSegments.First();
                if (Keys.Contains(key)) throw new DuplicateRouteConstraintKey(key);

                var constraint = RouteConstraints.Resolve(parameterSegments.Skip(1).ToList());

                Keys.Add(key);
                builder.Append(constraint);
            }
            // Segment is not a parameterized route constraint
            else
            {
                builder.Append(Regex.Escape(segment));
            }
        });

        builder.Append(regexSuffix);
        Pattern = new(builder.ToString());
    }
}
