using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Filters;

public class KeywordFilterTests
{
    private readonly KeywordFilter _filter = new();

    // Description of sample listing A6.
    private readonly SearchableListing _splitLevel =
        TestListings.Searchable(description: "Split-level home, large driveway, pets allowed.");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NoKeyword_Matches(string? keyword)
    {
        Assert.True(Matches(_splitLevel, new SearchQuery { Keyword = keyword }));
    }

    [Fact]
    public void TermAtStartOfWord_MatchesIgnoringCase()
    {
        var listing = TestListings.Searchable(description: "Top floor condo. Pets allowed.");

        Assert.True(Matches(listing, new SearchQuery { Keyword = "PET" }));
    }

    [Fact]
    public void TermInsideWord_DoesNotMatch()
    {
        var listing = TestListings.Searchable(description: "New carpet throughout.");

        Assert.False(Matches(listing, new SearchQuery { Keyword = "pet" }));
    }

    [Fact]
    public void TermMatchesPartOfHyphenatedWord()
    {
        Assert.True(Matches(_splitLevel, new SearchQuery { Keyword = "split" }));
    }

    [Fact]
    public void TermNotInDescription_DoesNotMatch()
    {
        Assert.False(Matches(_splitLevel, new SearchQuery { Keyword = "garage" }));
    }

    [Fact]
    public void EveryTermPresent_Matches()
    {
        Assert.True(Matches(_splitLevel, new SearchQuery { Keyword = "pets  split" }));
    }

    [Fact]
    public void OneTermMissing_DoesNotMatch()
    {
        Assert.False(Matches(_splitLevel, new SearchQuery { Keyword = "pets pool" }));
    }

    // Known limit, recorded in docs/decisions.md: matching is by word, not meaning.
    [Fact]
    public void KnownLimit_PetsAlsoMatchesNoPets()
    {
        var listing = TestListings.Searchable(description: "Cozy starter home, no pets.");

        Assert.True(Matches(listing, new SearchQuery { Keyword = "pets" }));
    }

    private bool Matches(SearchableListing listing, SearchQuery query) =>
        _filter.Matches(listing, new SearchCriteria(query));
}
