namespace Grapeseed;

/// <summary>
/// <para>Method attribute for defining a RestRoute</para>
/// <para>Targets: Method, AllowMultiple: true</para>
/// <para>&#160;</para>
/// <para>A method with the RestRoute attribute can have traffic routed to it by a RestServer if the request matches the assigned properties.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RestRouteAttribute : Attribute, IRouteProperties
{
    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public HttpMethod HttpMethod { get; set; } = HttpMethod.Any;

    public string Name { get; set; } = string.Empty;

    public string RouteTemplate { get; set; } = string.Empty;

    public RestRouteAttribute()
    {
        HttpMethod = HttpMethod.Any;
    }

    public RestRouteAttribute(string httpMethod)
    {
        HttpMethod = httpMethod;
    }

    public RestRouteAttribute(string httpMethod, string routeTemplate)
    {
        HttpMethod = httpMethod;
        RouteTemplate = routeTemplate;
    }
}
