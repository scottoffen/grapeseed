using System.Net;
using Microsoft.Extensions.Logging;

namespace Grapeseed;

public static class IRestServerExtensions
{
    /// <summary>
    /// Applies the global headers to the provided response header collection.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="headers"></param>
    public static void ApplyGlobalResponseHeaders(this IRestServer server, WebHeaderCollection headers)
    {
        foreach (var header in server.GlobalResponseHeaders.Where(g => !g.Suppress)) headers.Add(header.Name, header.Value);
    }

    /// <summary>
    /// Sets the default logger factory for the server.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static IRestServer SetDefaultLogger(this IRestServer server, ILoggerFactory factory)
    {
        DefaultLogger.LoggerFactory = factory;
        return server;
    }
}
