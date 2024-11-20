using Microsoft.Extensions.DependencyInjection;

namespace Grapeseed;

public static class IRouterExtensions
{
    private static readonly IDictionary<string, IServiceProvider> _providers = new Dictionary<string, IServiceProvider>();

    /// <summary>
    /// Adds all the routes specified to the routing table
    /// </summary>
    /// <param name="router"></param>
    /// <param name="routes"></param>
    /// <returns></returns>
    public static IRouter Register(this IRouter router, IEnumerable<IRoute> routes)
    {
        foreach (var route in routes.ToList())
        {
            router.Register(route);
        }

        return router;
    }

    /// <summary>
    /// Gets the service provider for the router
    /// </summary>
    /// <param name="router"></param>
    /// <returns></returns>
    public static IServiceProvider GetServiceProvider(this IRouter router)
    {
        if (!_providers.ContainsKey(router.Id)) _providers.Add(router.Id, router.Services.BuildServiceProvider());
        return _providers[router.Id];
    }
}
