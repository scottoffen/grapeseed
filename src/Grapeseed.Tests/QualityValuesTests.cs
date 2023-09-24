namespace Grapeseed.Tests
{
    public class QualityValuesTests
    {
        public class Parse
        {
            [Theory]
            [InlineData("text/html;q=0.8,text/*;q=0.7,*/*;q=0.6", "text/html", "text/*", "*/*")]
            [InlineData("text/html;q=0.8,text/*;q=0.6,*/*;q=0.7", "text/html", "*/*", "text/*")]
            [InlineData("text/html;q=0.7,text/*;q=0.8,*/*;q=0.6", "text/*", "text/html", "*/*")]
            [InlineData("text/html;q=0.6,text/*;q=0.8,*/*;q=0.7", "text/*", "*/*", "text/html")]
            [InlineData("text/html;q=0.7,text/*;q=0.6,*/*;q=0.8", "*/*", "text/html", "text/*")]
            [InlineData("text/html;q=0.6,text/*;q=0.7,*/*;q=0.8", "*/*", "text/*", "text/html")]
            public void SortsBasedOnPriority(string header, string first, string second, string third)
            {
                var result = QualityValues.Parse(header).ToList();

                result.ShouldNotBeNull();
                result.Count().ShouldBe(3);
                result[0].ShouldBe(first);
                result[1].ShouldBe(second);
                result[2].ShouldBe(third);
            }

            [Theory]
            [InlineData("text/html;q=0.8,text/*;q=0.8,*/*;q=0.8")]
            [InlineData("text/html;q=0.8,*/*;q=0.8,text/*;q=0.8")]
            [InlineData("*/*;q=0.8,text/html;q=0.8,text/*;q=0.8")]
            [InlineData("*/*;q=0.8,text/*;q=0.8,text/html;q=0.8")]
            [InlineData("text/*;q=0.8,text/html;q=0.8,*/*;q=0.8")]
            [InlineData("text/*;q=0.8,*/*;q=0.8,text/html;q=0.8")]
            public void WhenPriorityIsSame_SortsBasedOnSpecificity(string header)
            {
                var result = QualityValues.Parse(header).ToList();

                result.ShouldNotBeNull();
                result.Count().ShouldBe(3);
                result[0].ShouldBe("text/html");
                result[1].ShouldBe("text/*");
                result[2].ShouldBe("*/*");
            }
        }
    }
}
