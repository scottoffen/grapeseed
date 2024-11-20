using System.Text;
using System.Text.RegularExpressions;

namespace Grapeseed;

public class RouteTemplate : IRouteTemplate
{
    private static readonly Regex Default = new Regex(@"^.*$");

    public Regex Pattern { get; set; } = new Regex(@"^.*$");

    public List<string> PatternKeys { get; set; } = new List<string>();

    public RouteTemplate() { }

    public RouteTemplate(string pattern)
    {
        Pattern = ConvertToRegex(pattern, out var patternKeys);
        PatternKeys = patternKeys;
    }

    public RouteTemplate(Regex pattern, List<string>? patternKeys = null)
    {
        Pattern = pattern;
        if (patternKeys != null) PatternKeys = patternKeys;
    }

    public bool Matches(string endpoint) => Pattern.IsMatch(endpoint);

    public IDictionary<string, string> ParseEndpoint(string endpoint)
    {
        var parsed = new Dictionary<string, string>();
        var idx = 0;

        var matches = Pattern.Matches(endpoint)[0].Groups;
        for (int i = 1; i < matches.Count; i++)
        {
            var key = (PatternKeys?.Count > 0 && PatternKeys?.Count > idx)
                ? PatternKeys[idx]
                : $"p{idx}";

            parsed.Add(key, matches[i].Value);
            idx++;
        }

        return parsed;
    }

    public static Regex ConvertToRegex(string pattern, out List<string> patternKeys)
    {
        patternKeys = new List<string>();

        if (string.IsNullOrEmpty(pattern)) return Default;
        if (pattern.StartsWith('^')) return new Regex(pattern);

        var builder = new StringBuilder("(?i)^");
        var sections = pattern.SanitizePath() // Ensures the string begins with '/'
            .TrimEnd('$')                     // Removes any trailing '$'
            .Split(['{', '}']);  // splits into sections

        for (var i = 0; i < sections.Length; i++)
        {
            if ((i % 2) == 0) // is even
            {
                // Even sections don't contain constraints
                builder.Append(sections[i]);
            }
            else
            {
                var constraints = sections[i].Split(':').ToList();
                patternKeys.Add(constraints[0]);
                constraints.RemoveAt(0);
                builder.Append(RouteConstraints.Resolve(constraints));
            }
        }

        builder.Append("$");
        return new Regex(builder.ToString());
    }
}
