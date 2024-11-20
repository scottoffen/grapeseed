namespace Grapeseed;

public static class IRouteExtensions
{
    /// <summary>
    /// Disable the route
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    public static IRoute Disable(this IRoute route)
    {
        route.Enabled = false;
        return route;
    }

    /// <summary>
    /// Enable the route
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    public static IRoute Enable(this IRoute route)
    {
        route.Enabled = true;
        return route;
    }
}
