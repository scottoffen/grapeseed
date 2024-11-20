using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grapeseed;

/// <summary>
/// Delegate for <see cref="RouterBase.GlobalErrorHandlers"/> and <see cref="RouterBase.LocalErrorHandlers"/>
/// </summary>
/// <param name="context"></param>
/// <param name="e"></param>
/// <returns></returns>
public delegate Task HandleErrorAsync(IHttpContext context, Exception? e);

public class Router : RouterBase
{
    /// <summary>
    /// Gets the logger for this Router object.
    /// </summary>
    public ILogger<IRouter> Logger { get; }

    /// <summary>
    /// List of all registered routes.
    /// </summary>
    /// <returns></returns>
    protected internal readonly IList<IRoute> RegisteredRoutes = [];

    public override IList<IRoute> RoutingTable => RegisteredRoutes.ToList().AsReadOnly();

    public Router(ILogger<IRouter> logger)
    {
        Logger = logger ?? DefaultLogger.GetInstance<IRouter>();
    }

    public override IRouter Register(IRoute route)
    {
        if (RegisteredRoutes.All(r => !route.Equals(r))) RegisteredRoutes.Add(route);
        return this;
    }

    public override async void RouteAsync(object? state)
    {
        var context = state as IHttpContext;
        if (context == null) return;

        try
        {
            context.Response.ContentExpiresDuration = Options.ContentExpiresDuration;

            Logger.LogDebug($"{context.Id} : Routing {context.Request.Name}");

            var routesExecuted = await RouteAsync(context);
            if (routesExecuted == 0 || ((context.Response.StatusCode != HttpStatusCode.Ok || Options.RequireRouteResponse) && !context.WasRespondedTo))
            {
                if (context.Response.StatusCode == HttpStatusCode.Ok)
                    context.Response.StatusCode = HttpStatusCode.NotImplemented;
                await HandleErrorAsync(context);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"{context.Id}: An exception occurred while routing request {context.Request.Name}");
            await HandleErrorAsync(context, e);
        }
    }

    /// <summary>
    /// Routes the IHttpContext through matching routes
    /// </summary>
    /// <param name="context"></param>
    public virtual async Task<int> RouteAsync(IHttpContext context)
    {
        // 0. If no routes are found, there is nothing to do here
        var routing = RoutesFor(context);
        if (!routing.Any()) return 0;
        Logger.LogDebug($"{context.Id} : Matching routes discovered for {context.Request.Name}");

        // 1. Create a scoped container for dependency injection
        if (ServiceProvider == null) ServiceProvider = Services.BuildServiceProvider();
        context.Services = ServiceProvider.CreateScope().ServiceProvider;

        // 2. Invoke before routing handlers
        if (!context.CancellationToken.IsCancellationRequested)
        {
            Logger.LogTrace($"{context.Id} : Invoking before routing handlers for {context.Request.Name}");
            var beforeCount = (BeforeRoutingAsync != null) ? await BeforeRoutingAsync.Invoke(context) : 0;
            Logger.LogTrace($"{context.Id} : {beforeCount} Before routing handlers invoked for {context.Request.Name}");
        }

        // 3. Iterate over the routes until a response is sent
        var count = 0;
        foreach (var route in routing)
        {
            if (context.CancellationToken.IsCancellationRequested) break;
            if (context.Response.StatusCode != HttpStatusCode.Ok) break;
            if (context.WasRespondedTo && !Options.ContinueRoutingAfterResponseSent) break;
            Logger.LogDebug($"{context.Id} : Executing {route.Name} for {context.Request.Name}");
            await route.InvokeAsync(context);
            count++;
        }
        Logger.LogDebug($"{context.Id} : {count} of {routing.Count()} routes invoked");

        // 4. Invoke after routing handlers
        if (!context.CancellationToken.IsCancellationRequested)
        {
            Logger.LogTrace($"{context.Id} : Invoking after routing handlers for {context.Request.Name}");
            var afterCount = (AfterRoutingAsync != null) ? await AfterRoutingAsync.Invoke(context) : 0;
            Logger.LogTrace($"{context.Id} : {afterCount} After routing handlers invoked for {context.Request.Name}");
        }

        return count;
    }

    /// <summary>
    /// Gets an enumeration of registered routes that match the IHttpContext provided
    /// </summary>
    /// <param name="context"></param>
    /// <returns>IEnumerable&lt;IRoute&gt;</returns>
    public virtual IEnumerable<IRoute> RoutesFor(IHttpContext context)
    {
        foreach (var route in RegisteredRoutes.Where(r => r.IsMatch(context) && r.Enabled)) yield return route;
    }
}
