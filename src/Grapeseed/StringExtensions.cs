namespace Grapevine;

public static class StringExtensions
{
    public static string SanitizePath(this string path)
    {
        var basepath = path?.Trim().TrimEnd('/').TrimStart('/').Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(basepath) ? basepath : $"/{basepath}";
    }
}
