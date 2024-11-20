using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Grapeseed;

public abstract class RouteScannerBase : IRouteScanner
{
    public string BasePath { get; set; } = null!;

    public IServiceCollection Services { get; set; } = new ServiceCollection();

    /// <summary>
    /// Returns an enumeration of assemblies not on the IgnoredAssemblies list
    /// </summary>
    /// <value></value>
    public IEnumerable<Assembly> Assemblies
    {
        get
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (name is null) continue;

                if (name == "Grapevine" || name == "Grapeseed" || name.StartsWith([.. IgnoredAssemblies])) continue;

                yield return assembly;
            }
        }
    }

    /// <summary>
    /// List of assembly names to be ignored when scanning all assemblies
    /// </summary>
    /// <value></value>
    public IList<string> IgnoredAssemblies { get; } =
        [
            "vshost",
            "xunit",
            "Shouldly",
            "System",
            "Microsoft",
            "netstandard",
            "TestPlatform",
        ];

    public abstract IList<IRoute> Scan(string? basePath = null);
    public abstract IList<IRoute> Scan(Assembly assembly, string? basePath = null);
    public abstract IList<IRoute> Scan(Type type, string? basePath = null);
    public abstract IList<IRoute> Scan(MethodInfo methodInfo, string? basePath = null);
}
