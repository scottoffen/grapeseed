using System.Text.RegularExpressions;

namespace Grapeseed;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HeaderAttribute : Attribute
{
    public string Key { get; set; }
    public Regex Value { get; set; }

    public HeaderAttribute(string key, string value)
    {
        Key = key;
        Value = new Regex(value);
    }
}
