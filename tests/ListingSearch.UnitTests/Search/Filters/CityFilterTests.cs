using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Filters;

public class CityFilterTests
{
    private readonly CityFilter _filter = new();
    private readonly SearchableListing _springfield = TestListings.Searchable(city: "Springfield");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NoCity_Matches(string? city)
    {
        Assert.True(Matches(_springfield, new SearchQuery { City = city }));
    }

    [Fact]
    public void SameCity_Matches()
    {
        Assert.True(Matches(_springfield, new SearchQuery { City = "Springfield" }));
    }

    [Fact]
    public void SameCityWithDifferentCaseAndExtraSpaces_Matches()
    {
        Assert.True(Matches(_springfield, new SearchQuery { City = "  SPRINGfield " }));
    }

    [Fact]
    public void MultiWordCity_MatchesWhenInnerSpacesDiffer()
    {
        var fallsChurch = TestListings.Searchable(city: "Falls  Church");

        Assert.True(Matches(fallsChurch, new SearchQuery { City = " falls church" }));
    }

    [Fact]
    public void DifferentCity_DoesNotMatch()
    {
        Assert.False(Matches(_springfield, new SearchQuery { City = "Fairfax" }));
    }

    [Fact]
    public void PartOfCityName_DoesNotMatch()
    {
        Assert.False(Matches(_springfield, new SearchQuery { City = "Spring" }));
    }

    private bool Matches(SearchableListing listing, SearchQuery query) =>
        _filter.Matches(listing, new SearchCriteria(query));
}
