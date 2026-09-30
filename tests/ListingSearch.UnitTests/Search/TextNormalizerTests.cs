using ListingSearch.Core.Search;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search;

public class TextNormalizerTests
{
    [Fact]
    public void Normalize_TrimsCollapsesInnerWhitespaceAndLowercases()
    {
        Assert.Equal("falls church", TextNormalizer.Normalize("  Falls \t  CHURCH \n"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_NullOrBlank_ReturnsEmpty(string? text)
    {
        Assert.Equal("", TextNormalizer.Normalize(text));
    }

    [Fact]
    public void SplitWords_SplitsOnAnythingNotALetterOrDigit_AndLowercases()
    {
        Assert.Equal(
            new[] { "split", "level", "home", "2", "car", "garage" },
            TextNormalizer.SplitWords("Split-level home, 2-car garage."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" -, ")]
    public void SplitWords_NoLettersOrDigits_ReturnsNoWords(string? text)
    {
        Assert.Empty(TextNormalizer.SplitWords(text));
    }

    [Fact]
    public void SearchableListing_NormalisesCityAndSplitsDescription()
    {
        var listing = new SearchableListing(TestListings.Create(city: " Falls  Church", description: "Pet friendly."));

        Assert.Equal("falls church", listing.NormalizedCity);
        Assert.Equal(new[] { "pet", "friendly" }, listing.DescriptionWords);
    }

    [Fact]
    public void SearchCriteria_NormalisesCityAndSplitsKeyword()
    {
        var criteria = new SearchCriteria(new SearchQuery { City = " Falls  CHURCH ", Keyword = "Pets, split-level" });

        Assert.Equal("falls church", criteria.NormalizedCity);
        Assert.Equal(new[] { "pets", "split", "level" }, criteria.KeywordTerms);
    }
}
