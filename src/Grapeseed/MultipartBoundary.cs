using System.Text;

namespace Grapeseed;

public static class MultipartBoundary
{
    public const int MAX_BOUNDARY_LENGTH = 70;

    public const int MIN_BOUNDARY_LENGTH = 30;

    private static readonly char[] _multipartChars = "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

#if NET5_0
    private static readonly Random _random = new();

#endif
    public static string Generate(string firstPart = "----=NextPart_")
    {
        if (firstPart.Length >= MIN_BOUNDARY_LENGTH) return firstPart;

        var sb = new StringBuilder(firstPart);
        var init_size = firstPart.Length;
#if NET5_0
        var end_size = _random.Next(MIN_BOUNDARY_LENGTH, MAX_BOUNDARY_LENGTH);
#else
        var end_size = Random.Shared.Next(MIN_BOUNDARY_LENGTH, MAX_BOUNDARY_LENGTH);
#endif
        for (var i = init_size; i <= end_size; i++)
        {
#if NET5_0
            sb.Append(_multipartChars[_random.Next(_multipartChars.Length)]);
#else
            sb.Append(_multipartChars[Random.Shared.Next(_multipartChars.Length)]);
#endif
        }

        return sb.ToString();
    }
}
