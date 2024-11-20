namespace Grapevine;

public static class IHttpRequestExtensions
{
    /// <summary>
    /// Get the boundary value from the request's content type.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetMultipartBoundary(this IHttpRequest request)
    {
        if (string.IsNullOrEmpty(request.ContentType))
        {
            return string.Empty;
        }

        if (!request.ContentType.StartsWith(ContentType.MultipartFormData.Value))
        {
            return string.Empty;
        }

        var boundary = request.ContentType.Split(';')
            .Select(part => part.Trim())
            .FirstOrDefault(part => part.StartsWith("boundary="))?
            .Replace("boundary=", "");
        return boundary ?? string.Empty;
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

