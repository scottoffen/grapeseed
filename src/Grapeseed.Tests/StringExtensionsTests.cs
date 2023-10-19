namespace Grapeseed.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("/some/path", "/some/path")]
    [InlineData(" /some/path", "/some/path")]
    [InlineData("/some/path ", "/some/path")]
    [InlineData(" /some/path ", "/some/path")]
    [InlineData("/some/path/", "/some/path")]
    [InlineData("some/path/", "/some/path")]
    [InlineData("some/path", "/some/path")]
    [InlineData(" / some/path / ", "/some/path")]
    public void SanitizePathTests(string value, string expected)
    {
        var actual = value.SanitizePath();
        actual.ShouldBe(expected);
    }
}
