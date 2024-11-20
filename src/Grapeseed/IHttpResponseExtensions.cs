using System.Text;

namespace Grapeseed;

public static class IHttpResponseExtensions
{
    /// <summary>
    /// Sends a response to the client with the specified content and filename
    /// </summary>
    /// <param name="response"></param>
    /// <param name="content"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, Stream content, string filename)
    {
        if (!string.IsNullOrWhiteSpace(filename))
            response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");

        await response.SendResponseAsync(content);
    }

    /// <summary>
    /// Sends a response to the client with the specified content
    /// </summary>
    /// <param name="response"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, Stream content)
    {
        if (!response.Headers.AllKeys.ToArray().Contains("Expires"))
            response.AddHeader("Expires",
                DateTime.Now.Add(response.ContentExpiresDuration).ToString("R"));

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer);
        await response.SendResponseAsync(buffer.ToArray());
    }

    /// <summary>
    /// Sends an empty response to the client
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response)
    {
        await response.SendResponseAsync([]);
    }

    /// <summary>
    /// Sends an empty response to the client with the specified status code
    /// </summary>
    /// <param name="response"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, HttpStatusCode statusCode)
    {
        response.StatusCode = (int)statusCode;
        await response.SendResponseAsync([]);
    }

    /// <summary>
    /// Sends a response to the client with the specified content
    /// </summary>
    /// <param name="response"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, string content)
    {
        var encoding = response.ContentEncoding ?? Encoding.UTF8;
        await response.SendResponseAsync(encoding.GetBytes(content));
    }

    /// <summary>
    /// Sends a response to the client with the specified content and status code
    /// </summary>
    /// <param name="response"></param>
    /// <param name="statusCode"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, HttpStatusCode statusCode, string content)
    {
        response.StatusCode = (int)statusCode;
        var encoding = response.ContentEncoding ?? Encoding.UTF8;
        await response.SendResponseAsync(encoding.GetBytes(content));
    }

    /// <summary>
    /// Sends a response to the client with the specified content and status code
    /// </summary>
    /// <param name="response"></param>
    /// <param name="statusCode"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, HttpStatusCode statusCode, Stream content)
    {
        response.StatusCode = (int)statusCode;
        await response.SendResponseAsync(content);
    }

    /// <summary>
    /// Sends a response to the client with the specified content and status code
    /// </summary>
    /// <param name="response"></param>
    /// <param name="statusCode"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task SendResponseAsync(this IHttpResponse response, HttpStatusCode statusCode, byte[] content)
    {
        response.StatusCode = (int)statusCode;
        await response.SendResponseAsync(content);
    }
}
