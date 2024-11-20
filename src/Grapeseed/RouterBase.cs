using Microsoft.Extensions.DependencyInjection;

namespace Grapeseed;

public abstract class RouterBase : IRouter
{
    /// <summary>
    /// Gets or sets the default routing error handler
    /// </summary>
    /// <returns></returns>
    public static HandleErrorAsync DefaultErrorHandler { get; set; } = async (context, exception) =>
    {
        string content = context.Response.StatusCode.ToString() ?? HttpStatusCode.InternalServerError.ToString();

        if (exception != null)
        {
            content = $"Internal Server Error: {exception.Message}";
        }
        else if (context.Response.StatusCode == HttpStatusCode.NotFound)
        {
            content = $"File Not Found: {context.Request.Endpoint}";
        }
        else if (context.Response.StatusCode == HttpStatusCode.NotImplemented)
        {
            content = $"Method Not Implemented: {context.Request.Name}";
        }

        await context.Response.SendResponseAsync(content);
    };

    /// <summary>
    /// Collection of global error handlers keyed by HTTP status code
    /// </summary>
    public static readonly Dictionary<HttpStatusCode, HandleErrorAsync> GlobalErrorHandlers = [];

    /// <summary>
    /// Collection of error handlers specific to this Router object
    /// </summary>
    public readonly Dictionary<HttpStatusCode, HandleErrorAsync> LocalErrorHandlers = [];

    public virtual string Id { get; } = Guid.NewGuid().ToString();

    public RouterOptions Options { get; } = new RouterOptions();

    public abstract IList<IRoute> RoutingTable { get; }

    public IServiceCollection Services { get; set; } = null!;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public virtual RequestRoutingEvent AfterRoutingAsync { get; set; } = [];
    public virtual RequestRoutingEvent BeforeRoutingAsync { get; set; } = [];

    public abstract IRouter Register(IRoute route);

    public abstract void RouteAsync(object? state);

    /// <summary>
    /// Asynchronously determines which error handler to invoke and invokes with the given context and exception
    /// </summary>
    /// <param name="context"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected internal async Task HandleErrorAsync(IHttpContext context, Exception? e = null)
    {
        if (context.WasRespondedTo) return;

        if (context.Response.StatusCode == HttpStatusCode.Ok)
            context.Response.StatusCode = HttpStatusCode.InternalServerError;

        if (!LocalErrorHandlers.ContainsKey(context.Response.StatusCode))
        {
            LocalErrorHandlers[context.Response.StatusCode] = GlobalErrorHandlers.ContainsKey(context.Response.StatusCode)
                ? GlobalErrorHandlers[context.Response.StatusCode]
                : DefaultErrorHandler;
        }

        var action = LocalErrorHandlers[context.Response.StatusCode];

        try
        {
            await action(context, Options.SendExceptionMessages ? e : null).ConfigureAwait(false);
        }
        catch { }
    }
}
