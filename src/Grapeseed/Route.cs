using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Grapevine;

public class Route : RouteBase, IRoute
{
    protected Func<IHttpContext, Task> RouteAction;

    public Route(Func<IHttpContext, Task> action, HttpMethod method, string routePattern, bool enabled = true, string? name = null, string? description = null)
        : this(action, method, new RouteTemplate(routePattern), enabled, name, description) { }

    public Route(Func<IHttpContext, Task> action, HttpMethod method, Regex routePattern, bool enabled = true, string? name = null, string? description = null)
        : this(action, method, new RouteTemplate(routePattern), enabled, name, description) { }

    public Route(Func<IHttpContext, Task> action, HttpMethod method, IRouteTemplate routeTemplate, bool enabled = true, string? name = null, string? description = null)
        : base(method, routeTemplate, enabled, name, description)
    {
        RouteAction = action;
        Name = string.IsNullOrWhiteSpace(name)
            ? action.Method.Name
            : name;
    }

    public override async Task InvokeAsync(IHttpContext context)
    {
        if (!Enabled) return;
        context.Request.PathParameters = RouteTemplate.ParseEndpoint(context.Request.Endpoint);
        await RouteAction(context).ConfigureAwait(false);
    }
}

public class Route<T> : RouteBase, IRoute where T : notnull
{
    protected Func<T, IHttpContext, Task> RouteAction;

    public Route(MethodInfo methodInfo, HttpMethod method, string routePattern, bool enabled = true, string? name = null, string? description = null)
        : this(methodInfo, method, new RouteTemplate(routePattern), enabled, name, description) { }

    public Route(MethodInfo methodInfo, HttpMethod method, Regex routePattern, bool enabled = true, string? name = null, string? description = null)
        : this(methodInfo, method, new RouteTemplate(routePattern), enabled, name, description) { }

    public Route(MethodInfo methodInfo, HttpMethod method, IRouteTemplate routeTemplate, bool enabled = true, string? name = null, string? description = null) : base(method, routeTemplate, enabled, name, description)
    {
        RouteAction = (Func<T, IHttpContext, Task>)Delegate.CreateDelegate(typeof(Func<T, IHttpContext, Task>), null, methodInfo);
        if (string.IsNullOrWhiteSpace(Name)) Name = methodInfo.Name;
    }

    public override async Task InvokeAsync(IHttpContext context)
    {
        if (!Enabled) return;
        context.Request.PathParameters = RouteTemplate.ParseEndpoint(context.Request.Endpoint);
        await RouteAction(context.Services.GetRequiredService<T>(), context).ConfigureAwait(false);
    }
}
