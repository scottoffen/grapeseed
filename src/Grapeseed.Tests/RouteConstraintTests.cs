using System.Text.RegularExpressions;
using Grapevine.Exceptions;

namespace Grapeseed.Tests;

public class RouteConstraintTests
{
    public class AddResolver
    {
        private static readonly string _expected = "resolver successfully called";
        private static readonly RouteConstraintResolver _customResolver = (args) => { return _expected; };

        [Fact]
        public void WhenMissingKey_ThrowsException()
        {
            Should.Throw<ArgumentException>(() =>
            {
                RouteConstraints.AddResolver(string.Empty, _customResolver);
            });
        }

        [Fact]
        public void WhenUsingProtectedKey_ThrowsException()
        {
            Should.Throw<ArgumentException>(() =>
            {
                RouteConstraints.AddResolver("alpha", _customResolver);
            });
        }

        [Fact]
        public void AddsResolver()
        {
            RouteConstraints.AddResolver("custom", _customResolver);
            var actual = RouteConstraints.Resolve(new List<string> { "custom" });
            actual.ShouldBe(_expected);
        }
    }

    public class Resolve
    {

    }

    public class QuantifierResolver
    {
        [Theory]
        [InlineData("length(10)", "{10}")]
        [InlineData("length(1,10)", "{1,10}")]
        [InlineData("length(10,1)", "{1,10}")]
        [InlineData("max(10)", "{,10}")]
        [InlineData("maxlength(10)", "{,10}")]
        [InlineData("min(10)", "{10,}")]
        [InlineData("minlength(10)", "{10,}")]
        public void ResolvesQuantities(string value, string expected)
        {
            var actual = RouteConstraints.QuantifierResolver(value);
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("length")]
        [InlineData("max")]
        [InlineData("maxlength")]
        [InlineData("min")]
        [InlineData("minlength")]
        [InlineData("does not matter")]
        public void WhenLengthIsNotSpecified_ReturnsDefault(string value)
        {
            var expected = "+";
            var actual = RouteConstraints.QuantifierResolver(value);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void WhenConditionIsInvalid_ThrowsException()
        {
            Should.Throw<InvalidRouteParameterException>(() =>
            {
                var actual = RouteConstraints.QuantifierResolver("wrong(10)");
            });
        }

        [Theory]
        [InlineData("length(a)")]
        [InlineData("length(a,b)")]
        [InlineData("length(a,1)")]
        [InlineData("length(1,b)")]
        public void WhenLengthValuesAreNotIntegers_ThrowsException(string value)
        {
            Should.Throw<InvalidRouteParameterException>(() =>
            {
                var actual = RouteConstraints.QuantifierResolver(value);
            });
        }
    }
}
