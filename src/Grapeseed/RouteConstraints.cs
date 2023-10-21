using Grapevine.Exceptions;

namespace Grapevine;

public delegate string RouteConstraintResolver(IEnumerable<string> args);

public static class RouteConstraints
{
    private static readonly Dictionary<string, RouteConstraintResolver> _resolvers = new();
    private static readonly HashSet<string> _protectedKeys = new();

    /// <summary>
    /// The default pattern used when no resolver is specified or can be found for the constraint.
    /// </summary>
    /// <returns></returns>
    public static readonly string DefaultPattern = "([^/]+)";

    static RouteConstraints()
    {
        _resolvers.Add("alpha", AlphaResolver);
        _resolvers.Add("alphanum", AlphaNumericResolver);
        _resolvers.Add("alphanumeric", AlphaNumericResolver);
        _resolvers.Add("guid", GuidResolver);
        _resolvers.Add("length", LengthResolver);
        _resolvers.Add("max", LengthResolver);
        _resolvers.Add("maxlength", LengthResolver);
        _resolvers.Add("min", LengthResolver);
        _resolvers.Add("minlength", LengthResolver);
        _resolvers.Add("num", NumericResolver);
        _resolvers.Add("numeric", NumericResolver);

        _protectedKeys = _resolvers.Keys.ToHashSet();
    }

    /// <summary>
    /// Resolves the provided route constraint to regular expression partial.
    /// </summary>
    /// <param name="constraints"></param>
    /// <returns>A regular expression partial for the specified route constraint.</returns>
    public static string Resolve(IEnumerable<string> constraints)
    {
        if (!constraints.Any()) return DefaultPattern;

        var key = constraints.First().Contains('(')
            ? constraints.First()[..constraints.First().IndexOf('(')]
            : constraints.First();

        var resolver = _resolvers.ContainsKey(key)
            ? _resolvers[key]
            : DefaultResolver;

        return resolver.Invoke(constraints);
    }

    /// <summary>
    /// Adds a custom route constraint resolver for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="resolver"></param>
    public static void AddResolver(string key, RouteConstraintResolver resolver)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"Unable to add resolver: {nameof(key)} cannot be null, empty or whitespace");
        if (_protectedKeys.Contains(key)) throw new ArgumentException($"Unable to add resolver: {key} is a protected key name and cannot be overridden or replaced.");
        _resolvers[key] = resolver;
    }

    /// <summary>
    /// Returns a alpha character regular expression partial
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string AlphaResolver(IEnumerable<string> args)
    {
        var key = args.First();
        var modifiers = args.Skip(1).ToList();

        var quantifier = modifiers.Any()
            ? QuantifierResolver(modifiers.First())
            : QuantifierResolver(key.Replace("alpha", "length"));

        return $"([a-zA-Z]{quantifier})";
    }

    /// <summary>
    /// Returns an alphanumeric character regular expression partial
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string AlphaNumericResolver(IEnumerable<string> args)
    {
        var key = args.First().Contains("alphanumeric")
            ? args.First().Replace("alphanumeric", "length")
            : args.First().Replace("alphanum", "length");

        var modifiers = args.Skip(1).ToList();

        var quantifier = modifiers.Any()
            ? QuantifierResolver(modifiers.First())
            : QuantifierResolver(key);

        return @$"(\w{quantifier})";
    }

    /// <summary>
    /// Returns a string regular expression partial
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string DefaultResolver(IEnumerable<string> args)
    {
        return DefaultPattern;
    }

    /// <summary>
    /// Returns a guid regular expression partial
    /// </summary>
    /// <param name="args"></param>
    /// <remarks>Ignores any modifiers passed in.</remarks>
    private static string GuidResolver(IEnumerable<string> args)
    {
        return @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?";
    }

    /// <summary>
    /// Returns a string length regular expression partial
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string LengthResolver(IEnumerable<string> args)
    {
        var quantifier = QuantifierResolver(args.First());
        return DefaultPattern.Replace("+", quantifier);
    }

    /// <summary>
    /// Returns a numeric regular expression partial
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string NumericResolver(IEnumerable<string> args)
    {
        var key = args.First().Contains("numeric")
            ? args.First().Replace("numeric", "length")
            : args.First().Replace("num", "length");

        var modifiers = args.Skip(1).ToList();

        var quantifier = modifiers.Any()
            ? QuantifierResolver(modifiers.First())
            : QuantifierResolver(key);

        return $"(\\d{quantifier})";
    }

    /// <summary>
    /// Returns a regular expression partial for string length
    /// </summary>
    /// <remarks>
    /// This resolver can be called to resolve length expressions for length(n), length(n,m), min(n), minlength(n), max(n) and maxlength(n) from within other resolvers.
    /// </remarks>
    /// <param name="args"></param>
    /// <returns>Returns a quantifier expression based on the input in the form of {n} or {n,m}. Values for ranges will always be order ascending, regardless of how they are specified in the input.</returns>
    public static string QuantifierResolver(string args)
    {
        if (string.IsNullOrWhiteSpace(args)) return "+";

        try
        {
            var middle = args.IndexOf('(');
            if (middle < 0) return "+";

            var condition = args[..middle];
            var lengths = args[middle..]
                .TrimStart('(')
                .TrimEnd(')')
                .Split(',')
                .Select(s => int.Parse(s))
                .OrderBy(i => i)
                .ToList();

            switch (condition)
            {
                case "length":
                    return $"{{{string.Join(',', lengths)}}}";
                case "max":
                case "maxlength":
                    return $"{{,{lengths.First()}}}";
                case "min":
                case "minlength":
                    return $"{{{lengths.First()},}}";
                default:
                    throw new InvalidRouteParameterException();
            }
        }
        catch (FormatException)
        {
            throw new InvalidRouteParameterException($"length values must be numeric ({args})");
        }
    }
}
