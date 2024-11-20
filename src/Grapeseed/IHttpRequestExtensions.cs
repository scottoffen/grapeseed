namespace Grapeseed;

public static class IHttpRequestExtensions
{
    private static readonly int _startIndex = ContentType.MultipartFormData.ToString().Length;

    /// <summary>
    /// Get the boundary value from the request's content type.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetMultipartBoundary(this IHttpRequest request)
    {
        return (string.IsNullOrWhiteSpace(request.ContentType) || !request.ContentType.StartsWith(ContentType.MultipartFormData))
            ? string.Empty
            : request.ContentType.Substring(_startIndex);
    }

    /// <summary>
    /// Parse the form data from the request's input stream.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<IDictionary<string, string>> ParseFormUrlEncodedData(this IHttpRequest request)
    {
        var data = new Dictionary<string, string>();

        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            var payload = await reader.ReadToEndAsync();

            foreach (var kvp in payload.Split('&'))
            {
                var pair = kvp.Split('=');
                var key = pair[0];
                var value = pair[1];

                var decoded = string.Empty;
                while ((decoded = Uri.UnescapeDataString(value)) != value)
                {
                    value = decoded;
                }

                data.Add(key, value);
            }
        }

        return data;
    }
}

