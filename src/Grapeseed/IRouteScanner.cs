using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Grapeseed;

public interface IRouteScanner
{
    /// <summary>
    /// Gets or sets a value to be used as the base path for routes created by this IRouteScanner object
    /// </summary>
    /// <value></value>
    string BasePath { get; set; }

    /// <summary>
    /// List of assemblies to be ignored when scanning for routes
    /// </summary>
    /// <value></value>
    IList<string> IgnoredAssemblies { get; }

    /// <summary>
    /// Register implementations for types scanned that contained route methods
    /// </summary>
    /// <value></value>
    IServiceCollection Services { get; set; }

    /// <summary>
    /// Scans all assemblies and returns a list of discovered routes
    /// </summary>
    /// <param name="basePath"></param>
    /// <returns></returns>
    IList<IRoute> Scan(string? basePath = null);

    /// <summary>
    /// Scans the specified assembly and returns a list of discovered routes
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="basePath"></param>
    /// <returns></returns>
    IList<IRoute> Scan(Assembly assembly, string? basePath = null);

    /// <summary>
    /// Scans the specified type and returns a list of discovered routes
    /// </summary>
    /// <param name="type"></param>
    /// <param name="basePath"></param>
    /// <returns></returns>
    IList<IRoute> Scan(Type type, string? basePath = null);

    /// <summary>
    /// Scans the method info and returns a list of discovered routes
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="basePath"></param>
    /// <returns></returns>
    IList<IRoute> Scan(MethodInfo methodInfo, string? basePath = null);
}
