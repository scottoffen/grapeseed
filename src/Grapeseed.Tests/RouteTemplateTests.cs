using Grapevine.Exceptions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;

namespace Grapeseed.Tests;

public class RouteTemplateTests
{
    [Theory]
    [InlineData("/{segment}/?x", "(?i)^/([^/]+)/\\?x/?$")]
    [InlineData(@"/segment\x", @"(?i)^/segment\\x/?$")]
    [InlineData("/segment*x", "(?i)^/segment\\*x/?$")]
    [InlineData("/segment+x", "(?i)^/segment\\+x/?$")]
    [InlineData("/segment?x", "(?i)^/segment\\?x/?$")]
    [InlineData("/segment|x", "(?i)^/segment\\|x/?$")]
    [InlineData("/segment[x", "(?i)^/segment\\[x/?$")]
    [InlineData("/segment{x", "(?i)^/segment\\{x/?$")]
    [InlineData("/segment(x", "(?i)^/segment\\(x/?$")]
    [InlineData("/segment)x", "(?i)^/segment\\)x/?$")]
    [InlineData("/segment^x", "(?i)^/segment\\^x/?$")]
    [InlineData("/segment$x", "(?i)^/segment\\$x/?$")]
    [InlineData("/segment.x", "(?i)^/segment\\.x/?$")]
    [InlineData("/segment#x", "(?i)^/segment\\#x/?$")]
    [InlineData("/segment x", "(?i)^/segment\\ x/?$")]
    public void WhenSegmentContainsRegexCharacters_CharactersAreEscaped(string value, string expected)
    {
        var actual = new RouteTemplate(value);
        actual.Pattern.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("", @"^.*$")]
    [InlineData("^[a-z]/?$", "^[a-z]/?$")]
    [InlineData("/api/resource/id", "(?i)^/api/resource/id/?$")]
    [InlineData("/api/resource/{id}", "(?i)^/api/resource/([^/]+)/?$")]
    [InlineData("/api/resource/{id}/list", "(?i)^/api/resource/([^/]+)/list/?$")]
    [InlineData("/api/resource/{id}/{value}", "(?i)^/api/resource/([^/]+)/([^/]+)/?$")]
    [InlineData("/api/resource/{id:length(10)}", @"(?i)^/api/resource/([^/]{10})/?$")]
    [InlineData("/api/resource/{id:length(1,10)}", @"(?i)^/api/resource/([^/]{1,10})/?$")]
    [InlineData("/api/resource/{id:max(10)}", @"(?i)^/api/resource/([^/]{,10})/?$")]
    [InlineData("/api/resource/{id:maxlength(10)}", @"(?i)^/api/resource/([^/]{,10})/?$")]
    [InlineData("/api/resource/{id:min(10)}", @"(?i)^/api/resource/([^/]{10,})/?$")]
    [InlineData("/api/resource/{id:minlength(10)}", @"(?i)^/api/resource/([^/]{10,})/?$")]
    [InlineData("/api/resource/{id:numeric:length(10)}", @"(?i)^/api/resource/(\d{10})/?$")]
    [InlineData("/api/resource/{id:num:length(10)}", @"(?i)^/api/resource/(\d{10})/?$")]
    [InlineData("/api/resource/{id:num:length(1,10)}", @"(?i)^/api/resource/(\d{1,10})/?$")]
    [InlineData("/api/resource/{id:num:length(10,1)}", @"(?i)^/api/resource/(\d{1,10})/?$")]
    [InlineData("/api/resource/{id:num:max(10)}", @"(?i)^/api/resource/(\d{,10})/?$")]
    [InlineData("/api/resource/{id:num:maxlength(10)}", @"(?i)^/api/resource/(\d{,10})/?$")]
    [InlineData("/api/resource/{id:num:min(10)}", @"(?i)^/api/resource/(\d{10,})/?$")]
    [InlineData("/api/resource/{id:num:minlength(10)}", @"(?i)^/api/resource/(\d{10,})/?$")]
    public void ConvertsTemplateToRegexCorrectly(string pattern, string expected)
    {
        var actual = new RouteTemplate(pattern);
        actual.Pattern.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("/segment/{param}", 2, 1)]
    [InlineData("/{param}/segment", 2, 1)]
    [InlineData("/segment/{param1}/{param2}/segment", 4, 2)]
    [InlineData("/segment/{param1}/segment/{param2}", 4, 2)]
    [InlineData("{param1}/{param2}/segment/segment", 4, 2)]
    [InlineData("/{param1}/segment/{param2}/segment", 4, 2)]
    [InlineData("/{param1}", 1, 1)]
    [InlineData("/segment", 1, 0)]
    [InlineData("/there/are/no/parameters/here", 5, 0)]
    [InlineData("^regex", 0, 0)]
    public void IdentifiesSegmentsCorrectly(string pattern, int segments, int keys)
    {
        var actual = new RouteTemplate(pattern);
        actual.Segments.ShouldBe(segments);
        actual.Parameters.ShouldBe(keys);
    }

    [Fact]
    public void WhenDuplicateParametersKeysArePresent_ThrowsException()
    {
        Should.Throw<DuplicateRouteConstraintKey>(() =>
        {
            _ = new RouteTemplate("/segment/{param}/{param}");
        });
    }

    [Fact]
    public void WhenASegmentContainsMultipleConstraints_ThrowsException()
    {
        Should.Throw<InvalidRouteParameterException>(() =>
        {
            _ = new RouteTemplate("/segment/{param1}{param2}/");
        });
    }

    [Theory]
    [InlineData("/users/{id}", "/users/1234", true)]
    [InlineData("/users/{id}", "/users/name", true)]
    [InlineData("/users/{id:alpha}", "/users/1234", false)]
    [InlineData("/users/{id:num}", "/users/1234", true)]
    [InlineData("/users/{id:alpha}", "/users/name", true)]
    [InlineData("/users/{id:num}", "/users/name", false)]
    public void MatchesReturnCorrectValue(string pattern, string route, bool expected)
    {
        var template = new RouteTemplate(pattern);
        var actual = template.Matches(route);
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/segment/11aa22", true)]
    [InlineData("/segment/aa11bb", false)]
    public void UsingCustomResolver_MatchesReturnsCorrectValue(string route, bool expected)
    {
        RouteConstraints.AddResolver("custom", CustomResolver);
        var pattern = @"/segment/{id:custom}";

        var template = new RouteTemplate(pattern);
        template.Pattern.ToString().ShouldBe(@"(?i)^/segment/(\d{2}[a-z]{2}\d{2})/?$");

        var actual = template.Matches(route);
        actual.ShouldBe(expected);
    }

    private string CustomResolver(IEnumerable<string> args)
    {
        return @"(\d{2}[a-z]{2}\d{2})";
    }
}
