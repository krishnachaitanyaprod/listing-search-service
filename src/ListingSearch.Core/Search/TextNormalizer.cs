using System.Text.RegularExpressions;

namespace ListingSearch.Core.Search;

// One set of text rules, applied the same way to listing data and to the query.
public static partial class TextNormalizer
{
    // Trims, collapses inner whitespace to one space, and lowercases.
    public static string Normalize(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : Whitespace().Replace(text.Trim(), " ").ToLowerInvariant();

    // Lowercase words, split on anything that isn't a letter or digit: "Split-level" -> "split", "level".
    public static string[] SplitWords(string? text) =>
        string.IsNullOrEmpty(text)
            ? []
            : Word().Matches(text.ToLowerInvariant()).Select(match => match.Value).ToArray();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"[\p{L}\p{Nd}]+")]
    private static partial Regex Word();
}
