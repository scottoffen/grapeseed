using System.Text;
using System.Reflection;

namespace Grapevine;

public struct ContentType
{
    #region Static Implementations

    public static readonly ContentType Binary = new("application/octet-stream");

    public static readonly ContentType Css = new("text/css", false, "UTF-8");

    public static readonly ContentType FormUrlEncoded = new("application/x-www-form-urlencoded");

    public static readonly ContentType Gif = new("image/gif");

    public static readonly ContentType Html = new("text/html", false, "UTF-8");

    public static readonly ContentType Icon = new("image/x-icon");

    public static readonly ContentType JavaScript = new("application/javascript", false, "UTF-8");

    public static readonly ContentType Json = new("application/json", false, "UTF-8");

    public static readonly ContentType Jpg = new("image/jpeg");

    public static readonly ContentType Mp3 = new("audio/mpeg");

    public static readonly ContentType Mp4 = new("video/mp4");

    public static readonly ContentType MultipartFormData = new("multipart/form-data");

    public static readonly ContentType Pdf = new("application/pdf");

    public static readonly ContentType Png = new("image/png");

    public static readonly ContentType Svg = new("image/svg+xml", false, "UTF-8");

    public static readonly ContentType Text = new("text/plain", false, "UTF-8");

    public static readonly ContentType Xml = new("application/xml", false, "UTF-8");

    public static readonly ContentType Zip = new("application/zip");

    #endregion

    private static readonly Dictionary<string, ContentType> _contentTypes = [];
    private static readonly Dictionary<string, ContentType> _extensions = [];

    /// <summary>
    /// The static constructor caches the content types and extensions collections.
    /// </summary>
    static ContentType()
    {
        foreach (var field in typeof(ContentType).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field == null || field.FieldType != typeof(ContentType)) continue;

            var value = field.GetValue(null);
            if (value == null) continue;

            var contentType = (ContentType)value;

            _contentTypes.Add(contentType, contentType);
            _extensions.Add(field.Name.ToLower(), contentType);
        }
    }

    /// <summary>
    /// Returns the multipart boundary value
    /// </summary>
    public string Boundary { get; private set; } = string.Empty;

    /// <summary>
    /// Returns the charset of the content type
    /// </summary>
    public string Charset { get; }

    /// <summary>
    /// Returns a boolean value that indicates whether the content type is binary
    /// </summary>
    public bool IsBinary { get; }

    /// <summary>
    /// Returns the content type mime-type value
    /// </summary>
    public string Value { get; }

    private string _string = string.Empty;

    public ContentType(string value, bool isBinary = true, string charset = "")
    {
        Value = value;
        IsBinary = isBinary;
        Charset = charset;
    }

    /// <summary>
    /// Resets the multipart boundary value
    /// </summary>
    public void ResetBoundary()
    {
        if (Value.StartsWith("multipart"))
        {
            _string = string.Empty;
            Boundary = MultipartBoundary.Generate();
        }
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(_string) || Value.StartsWith("multipart"))
        {
            ResetBoundary();

            var builder = new StringBuilder(Value);

            if (!string.IsNullOrEmpty(Boundary))
            {
                builder.Append($"; boundary={Boundary}");
            }

            if (!string.IsNullOrEmpty(Charset))
            {
                builder.Append($"; charset={Charset}");
            }

            _string = builder.ToString();
        }

        return _string;
    }

    public static implicit operator string(ContentType value) => value.ToString();

    public static implicit operator ContentType(string value) => Find(value);

    public static ContentType Find(string value)
    {
        Add(value);
        return _contentTypes[value];
    }

    public static ContentType FindKey(string key)
    {
        var k = key.ToLower();
        return _extensions.ContainsKey(k)
            ? _extensions[k]
            : Binary;
    }

    public static void Add(string value, bool isBinary = true, string charset = "")
    {
        if (_contentTypes.ContainsKey(value)) return;

        var key = (value.Contains(';') && string.IsNullOrWhiteSpace(charset))
            ? value
            : string.IsNullOrWhiteSpace(charset)
                ? value
                : $"{value}; charset={charset}";

        if (_contentTypes.ContainsKey(key)) return;

        if (value.Contains(';') && string.IsNullOrWhiteSpace(charset))
        {
            var parts = value.Split(';');
            value = parts[0];
            charset = parts[1].Replace("charset=", "").Trim();
        }

        var contentType = new ContentType(value, isBinary, charset);
        _contentTypes.Add(contentType, contentType);
    }

    public static void Add(string key, ContentType contentType)
    {
        _contentTypes.Add(contentType, contentType);
        _extensions.Add(key, contentType);
    }

    public static ContentType MultipartContent(Multipart multipart = default, string boundary = "")
    {
        if (string.IsNullOrWhiteSpace(boundary))
            boundary = MultipartBoundary.Generate();

        return new ContentType($"multipart/{multipart.ToString().ToLower()}", false, "")
        {
            Boundary = boundary[..70].TrimEnd()
        };
    }
}
