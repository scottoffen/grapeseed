using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Grapevine;

/// <summary>
/// Provides methods for launching the default web browser.
/// </summary>
public static class BrowserLauncher
{
    /// <summary>
    /// Opens the specified URL in the default web browser.
    /// </summary>
    /// <param name="url"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        // Ensure the URL starts with a valid scheme (http or https)
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }

        OpenUrl(new Uri(url));
    }

    /// <summary>
    /// Opens the specified URL in the default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public static void OpenUrl(Uri url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url.ToString(),
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url.ToString());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url.ToString());
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system");
            }

            Console.WriteLine($"Opening {url} in the default browser...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while opening the browser: {ex.Message}");
            throw; // Optionally rethrow the exception for higher-level handling
        }
    }
}
