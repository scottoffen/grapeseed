using System.Text.RegularExpressions;

namespace Grapevine;

public interface IRoute : IRouteProperties
{
    /// <summary>
    /// Returns the route template.
    /// </summary>
    IRouteTemplate RouteTemplate { get; }

    /// <summary>
    /// Returns the route handler.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task InvokeAsync(IHttpContext context);

    /// <summary>
    /// Returns true if the route matches the request.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    bool IsMatch(IHttpContext context);

    /// <summary>
    /// Adds a header constraint to the route.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    IRoute WithHeader(string header, Regex pattern);
}
