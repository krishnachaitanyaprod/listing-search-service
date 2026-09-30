using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Filters;

public class MinBedroomsFilterTests
{
    private readonly MinBedroomsFilter _filter = new();
    private readonly SearchableListing _listing = TestListings.Searchable(bedrooms: 3);

    [Fact]
    public void NoMinBedrooms_Matches()
    {
        Assert.True(Matches(new SearchQuery()));
    }

    [Fact]
    public void MoreBedroomsThanMin_Matches()
    {
        Assert.True(Matches(new SearchQuery { MinBedrooms = 2 }));
    }

    [Fact]
    public void BedroomsEqualToMin_Matches()
    {
        Assert.True(Matches(new SearchQuery { MinBedrooms = 3 }));
    }

    [Fact]
    public void FewerBedroomsThanMin_DoesNotMatch()
    {
        Assert.False(Matches(new SearchQuery { MinBedrooms = 4 }));
    }

    private bool Matches(SearchQuery query) => _filter.Matches(_listing, new SearchCriteria(query));
}
