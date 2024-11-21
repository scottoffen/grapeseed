using System.Collections.Concurrent;

namespace Grapevine;

public class Locals : ConcurrentDictionary<object, object?> { }

public static class LocalExtensions
{
    public static object? Get(this Locals l, object key)
    {
        _ = l.TryGetValue(key, out object? value);
        return value;
    }

    public static T? GetAs<T>(this Locals l, object key)
    {
        return (T?)l.Get(key);
    }

    public static T? GetOrAddAs<T>(this Locals l, object key, object value)
    {
        return (T?)l.GetOrAdd(key, value);
    }
}
