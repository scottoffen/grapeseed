using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Grapevine;

/// <summary>
/// The default logger factory.
/// </summary>
public static class DefaultLogger
{
    /// <summary>
    /// Get or set the logger factory.
    /// </summary>
    public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

    /// <summary>
    /// Returns a logger instance for the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ILogger<T> GetInstance<T>() => LoggerFactory.CreateLogger<T>();
}
