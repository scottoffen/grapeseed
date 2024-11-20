using System.Reflection;

namespace Grapeseed;

public class HttpMethod : System.Net.Http.HttpMethod
{
    public static readonly HttpMethod Any = new("Any");

    private static readonly Dictionary<string, HttpMethod> _httpMethods = [];

    static HttpMethod()
    {
        var ct = typeof(HttpMethod);
        var methods = typeof(HttpMethod).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetValue(null))
            .Where(f => f != null && f.GetType() == ct)
            .Cast<HttpMethod>()
            .ToList();

        foreach (var method in methods) _httpMethods.Add(method.ToString().ToLower(), method);
    }

    public HttpMethod(string method) : base(method) { }

    public bool Equivalent(HttpMethod other)
    {
        if (this.Equals(Any)) return true;
        if (other.Equals(Any)) return true;
        return this.Equals(other);
    }

    public static implicit operator HttpMethod(string value)
    {
        return Find(value);
    }

    public static implicit operator string(HttpMethod value)
    {
        return value.ToString();
    }

    public static HttpMethod Find(string value)
    {
        var key = Add(value);
        return _httpMethods[key];
    }

    public static string Add(string value)
    {
        var key = value.ToLower();
        if (!_httpMethods.ContainsKey(key)) _httpMethods.Add(key, new HttpMethod(value));
        return key;
    }
}
