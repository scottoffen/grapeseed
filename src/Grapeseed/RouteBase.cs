using System.Text.RegularExpressions;

namespace Grapevine;

public abstract class RouteBase : IRoute
{
    public string Description { get; set; }

    public bool Enabled { get; set; }

    public readonly Dictionary<string, Regex> HeaderConditions = new Dictionary<string, Regex>();

    public HttpMethod HttpMethod { get; set; }

    public string Name { get; set; }

    public IRouteTemplate RouteTemplate { get; set; }

    protected RouteBase(HttpMethod httpMethod, IRouteTemplate routeTemplate, bool enabled, string? name, string? description)
    {
        HttpMethod = httpMethod;
        RouteTemplate = routeTemplate;
        Name = name ?? string.Empty;
        Description = description ?? string.Empty;
        Enabled = enabled;
    }

    public abstract Task InvokeAsync(IHttpContext context);

    public virtual bool IsMatch(IHttpContext context)
    {
        if (!Enabled || !context.Request.HttpMethod.Equivalent(HttpMethod) || !RouteTemplate.Matches(context.Request.Endpoint)) return false;

        foreach (var condition in HeaderConditions)
        {
            var value = context.Request.Headers.Get(condition.Key) ?? string.Empty;
            if (condition.Value.IsMatch(value)) continue;
            return false;
        }

        return true;
    }

    public virtual IRoute WithHeader(string header, Regex pattern)
    {
        HeaderConditions[header] = pattern;
        return this;
    }
}

