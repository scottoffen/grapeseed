namespace Grapevine
{
    /// <summary>
    /// Static methods to parse <see href="https://developer.mozilla.org/en-US/docs/Glossary/Quality_values">quality values</see>.
    /// </summary>
    public class QualityValues
    {
        /// <summary>
        /// Parse <see href="https://developer.mozilla.org/en-US/docs/Glossary/Quality_values">quality values</see> from the provided http header value.
        /// </summary>
        /// <param name="header"></param>
        /// <returns>An enumeration of keys sorted by their <see href="https://developer.mozilla.org/en-US/docs/Glossary/Quality_values">quality value and specificity</see>.</returns>
        public static IEnumerable<string> Parse(string header)
        {
            var factors = new SortedDictionary<decimal, List<string>>();
            var values = new List<string>();

            foreach (var factor in header.Trim().Split(',').Select(x => x.Split(";q=")))
            {
                var key = string.IsNullOrWhiteSpace(factor[1])
                    ? Convert.ToDecimal(1)
                    : Convert.ToDecimal(factor[1].Trim());

                if (!factors.ContainsKey(key)) factors.Add(key, new List<string>());
                factors[key].Add(factor[0].Trim());
            }

            foreach (var factor in factors.Reverse())
            {
                values.AddRange(factor.Value.OrderBy(v => v.Count(c => c == '*')));
            }

            return values;
        }
    }
}
