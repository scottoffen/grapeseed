using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Grapeseed;

public static class IRouteScannerExtensions
{
    /// <summary>
    /// Scans the assembly containing the specified type and returns a list of discovered routes
    /// </summary>
    /// <param name="scanner"></param>
    /// <param name="services"></param>
    /// <param name="basePath"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IList<IRoute> ScanAssemblyContainingType<T>(this IRouteScanner scanner, IServiceCollection? services = null, string? basePath = null)
    {
        var assembly = Assembly.GetAssembly(typeof(T));
        if (assembly is null) return [];

        return scanner.Scan(assembly, basePath);
    }

    /// <summary>
    /// Scans the type T and returns a list of discovered routes
    /// </summary>
    /// <param name="scanner"></param>
    /// <param name="basePath"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IList<IRoute> Scan<T>(this IRouteScanner scanner, string? basePath = null)
    {
        return scanner.Scan(typeof(T), basePath);
    }

    /// <summary>
    /// Add an assembly to be ignored when scanning all assemblies for routes
    /// </summary>
    /// <param name="scanner"></param>
    /// <param name="assembly"></param>
    public static void AddIgnoredAssembly(this IRouteScanner scanner, Assembly assembly)
    {
        var name = assembly.GetName().Name;
        if (name is not null) scanner.IgnoredAssemblies.Add(name);
    }

    /// <summary>
    /// Add a list of assemblies to be ignored when scanning all assemblies for routes
    /// </summary>
    /// <param name="scanner"></param>
    /// <param name="assemblies"></param>
    public static void AddIgnoredAssemblies(this IRouteScanner scanner, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            scanner.AddIgnoredAssembly(assembly);
        }
    }

    /// <summary>
    /// Add an assembly to be ignored when scanning all assemblies for routes
    /// </summary>
    /// <param name="scanner"></param>
    /// <param name="assemblyName"></param>
    public static void AddIgnoredAssembly(this IRouteScanner scanner, string assemblyName)
    {
        scanner.IgnoredAssemblies.Add(assemblyName);
    }

    /// <summary>
    /// Add a list of assemblies to be ignored when scanning all assemblies for routes
    /// </summary>
    /// <param name="scanner"></param>
    /// <param name="assemblyNames"></param>
    public static void AddIgnoredAssemblies(this IRouteScanner scanner, string[] assemblyNames)
    {
        foreach (var assemblyName in assemblyNames) scanner.IgnoredAssemblies.Add(assemblyName);
    }

    /// <summary>
    /// Add the assembly containing the specified type to be ignored when scanning all assemblies for routes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="scanner"></param>
    public static void IgnoreAssemblyContainingType<T>(this IRouteScanner scanner)
    {
        var assembly = Assembly.GetAssembly(typeof(T));
        var name = assembly?.GetName().Name;
        if (name is not null)
            scanner.IgnoredAssemblies.Add(name);
    }
}
