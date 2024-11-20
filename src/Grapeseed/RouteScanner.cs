using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Grapevine;

public class RouteScanner : RouteScannerBase, IRouteScanner
{
    /// <summary>
    /// Gets the logger for this RouteScanner object
    /// </summary>
    /// <value></value>
    public ILogger<IRouteScanner> Logger { get; }

    public RouteScanner(ILogger<IRouteScanner> logger)
    {
        Logger = logger ?? DefaultLogger.GetInstance<IRouteScanner>();
    }

    public override IList<IRoute> Scan(string? basePath = null)
    {
        var basepath = (basePath ?? BasePath).SanitizePath();

        var routes = new List<IRoute>();
        Logger.LogTrace("Begin Global Route Scanning");

        foreach (var assembly in Assemblies)
        {
            try
            {
                routes.AddRange(Scan(assembly, basepath));
            }
            catch (ReflectionTypeLoadException ex)
            {
                var message = $"Exception occurred when scanning for routes";
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Logger.LogDebug(loaderEx, message);
                }
            }
        }

        Logger.LogTrace($"Global Route Scanning Complete: {routes.Count} total routes found");

        return routes;
    }

    public override IList<IRoute> Scan(Assembly assembly, string? basePath = null)
    {
        var basepath = (basePath ?? BasePath).SanitizePath();
        var routes = new List<IRoute>();

        var name = assembly.GetName().FullName;
        Logger.LogTrace($"Scanning assembly {name} for routes");

        try
        {
            foreach (var type in GetQualifiedTypes(assembly))
                routes.AddRange(Scan(type, basepath));
        }
        catch (ReflectionTypeLoadException ex)
        {
            var message = $"Exception occurred when scanning assembly {name} for routes";
            foreach (var loaderEx in ex.LoaderExceptions)
                Logger.LogDebug(loaderEx, message);
        }

        Logger.LogTrace($"Scan of assembly {name} complete: {routes.Count} total routes found");

        return routes;
    }

    public override IList<IRoute> Scan(Type type, string? basePath = null)
    {
        var routes = new List<IRoute>();
        Logger.LogTrace($"Scanning type {type.Name} for routes");

        var attribute = type.GetCustomAttributes(typeof(RestResourceAttribute))
            .Cast<RestResourceAttribute>().FirstOrDefault()
            ?? new RestResourceAttribute();

        var basepath = (basePath ?? BasePath).SanitizePath();
        basepath = (!string.IsNullOrWhiteSpace(basepath))
            ? string.Join("/", [basepath, attribute.BasePath.SanitizePath()])
            : attribute.BasePath.SanitizePath();

        foreach (var method in GetQualifiedMethods(type))
        {
            if (type.IsAbstract && !method.IsStatic) continue;
            routes.AddRange(Scan(method, basepath));
        }

        if (routes.Count > 0 && !type.IsAbstract)
        {
            var lifetime = type.IsDefined(typeof(ResourceLifetimeAttribute), false)
                ? type.GetCustomAttributes(typeof(ResourceLifetimeAttribute))
                    .Cast<ResourceLifetimeAttribute>()
                    .FirstOrDefault()?.ServiceLifetime
                : ServiceLifetime.Scoped;

            Services.TryAdd(new ServiceDescriptor(type, type, lifetime ?? ServiceLifetime.Scoped));
        }

        Logger.LogTrace($"Scan of type {type.Name} complete: {routes.Count} total routes found");
        return routes;
    }

    public override IList<IRoute> Scan(MethodInfo methodInfo, string? basePath = null)
    {
        var basepath = (basePath ?? BasePath).SanitizePath();

        var routes = new List<IRoute>();
        var type = methodInfo.ReflectedType;
        if (type is null) return routes;

        Logger.LogTrace($"Scanning method {methodInfo.Name} for routes");

        var headers = GetAttributes<HeaderAttribute>(methodInfo);

        foreach (var attribute in GetAttributes<RestRouteAttribute>(methodInfo))
        {
            // 1. Create the arguments for Route<T> constructor
            var args = attribute.GenerateRouteConstructorArguments(methodInfo, basepath);

            // 2. Create the Route instance
            var routeType = (!methodInfo.IsStatic)
                ? typeof(Route<>).MakeGenericType(type)
                : typeof(Route);

            var instance = Activator.CreateInstance(routeType, args);
            if (instance is not IRoute route) continue;

            // 3. Add any header conditions
            foreach (var header in headers) route.WithHeader(header.Key, header.Value);

            // 4. Add route to routing table
            routes.Add(route);

            Logger.LogTrace($"Generated route {route}");
        }

        Logger.LogTrace($"Scan of method {methodInfo.Name} complete: {routes.Count} total routes found");

        return routes;
    }

    /// <summary>
    /// Returns an enumeration of classes in the specified assembly that have the RestResource attribute, order alphabetically by name
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetQualifiedTypes(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes()
            .Where(t => t.IsClass && t.IsDefined(typeof(RestResourceAttribute), false))
            .OrderBy(t => t.Name)) yield return type;
    }

    /// <summary>
    /// Returns an enumeration of MethodInfo in the specified type that have the RestRoute attribute, order by appearance in the type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<MethodInfo> GetQualifiedMethods(Type type)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => !m.IsAbstract && m.IsDefined(typeof(RestRouteAttribute), true))) yield return method;
    }

    /// <summary>
    /// Returns an enumeration of attributes of the specified type
    /// </summary>
    /// <param name="method"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetAttributes<T>(MethodInfo method) where T : Attribute
    {
        foreach (var attribute in method.GetCustomAttributes(true)
            .Where(a => a is T)
            .Cast<T>()) yield return attribute;
    }
}
