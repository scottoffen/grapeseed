using System.Text;

namespace Grapeseed;

public static class MultipartBoundary
{
    public const int MAX_BOUNDARY_LENGTH = 70;

    public const int MIN_BOUNDARY_LENGTH = 30;

    private static readonly char[] _multipartChars = "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static string Generate(string firstPart = "----=NextPart_")
    {
        if (firstPart.Length >= MIN_BOUNDARY_LENGTH) return firstPart;

        var sb = new StringBuilder(firstPart);
        var init_size = firstPart.Length;
        var end_size = Random.Shared.Next(MIN_BOUNDARY_LENGTH, MAX_BOUNDARY_LENGTH);

        for (var i = init_size; i <= end_size; i++)
        {
            sb.Append(_multipartChars[Random.Shared.Next(_multipartChars.Length)]);
        }

        return sb.ToString();
    }
}
