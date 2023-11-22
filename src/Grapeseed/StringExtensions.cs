namespace Grapevine;

public static class StringExtensions
{
    public static string SanitizePath(this string path)
    {
        var basePath = path?.Trim().TrimEnd('/').TrimStart('/').Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(basePath) ? basePath : $"/{basePath}";
    }
}
