using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Filters;

public class PriceRangeFilterTests
{
    private readonly PriceRangeFilter _filter = new();
    private readonly SearchableListing _listing = TestListings.Searchable(price: 450000m);

    [Fact]
    public void NoPriceRange_Matches()
    {
        Assert.True(Matches(new SearchQuery()));
    }

    [Fact]
    public void PriceInsideRange_Matches()
    {
        Assert.True(Matches(new SearchQuery { MinPrice = 400000m, MaxPrice = 500000m }));
    }

    [Fact]
    public void PriceEqualToMinPrice_Matches()
    {
        Assert.True(Matches(new SearchQuery { MinPrice = 450000m }));
    }

    [Fact]
    public void PriceEqualToMaxPrice_Matches()
    {
        Assert.True(Matches(new SearchQuery { MaxPrice = 450000m }));
    }

    [Fact]
    public void PriceBelowMinPrice_DoesNotMatch()
    {
        Assert.False(Matches(new SearchQuery { MinPrice = 450001m }));
    }

    [Fact]
    public void PriceAboveMaxPrice_DoesNotMatch()
    {
        Assert.False(Matches(new SearchQuery { MaxPrice = 449999m }));
    }

    private bool Matches(SearchQuery query) => _filter.Matches(_listing, new SearchCriteria(query));
}
