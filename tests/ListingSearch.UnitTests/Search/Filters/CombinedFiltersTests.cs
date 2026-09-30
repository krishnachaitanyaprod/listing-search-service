using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Filters;

public class CombinedFiltersTests
{
    private static readonly IListingFilter[] AllFilters =
        [new PriceRangeFilter(), new MinBedroomsFilter(), new CityFilter(), new KeywordFilter()];

    // A subset of data/sample_listings.json.
    private static readonly SearchableListing[] Listings =
    [
        Sample("MLS_A", "A1", 450000m, 2, "Springfield", "Bright top-floor condo near shops and transit. Pet friendly."),
        Sample("MLS_B", "B7", 452000m, 2, "Springfield", "Top floor condo, walk to shopping. Pets allowed."),
        Sample("MLS_A", "A2", 525000m, 3, "Springfield", "Updated kitchen, fenced yard, close to schools."),
        Sample("MLS_A", "A3", 399000m, 2, "Fairfax", "Cozy starter home, no pets."),
        Sample("MLS_A", "A4", 610000m, 4, "Reston", "Spacious family home near Reston Town Center. Pets welcome."),
        Sample("MLS_B", "B10", 585000m, 3, "Reston", "Townhome with 2-car garage, community pool."),
        Sample("MLS_A", "A6", 415000m, 3, "Manassas", "Split-level home, large driveway, pets allowed.")
    ];

    [Fact]
    public void AllFiltersTogether_KeepOnlyListingsMatchingEveryFilter()
    {
        var query = new SearchQuery
        {
            MinPrice = 400000m,
            MaxPrice = 500000m,
            MinBedrooms = 2,
            City = "springfield",
            Keyword = "pet"
        };

        Assert.Equal(new[] { "MLS_A:A1", "MLS_B:B7" }, Search(query));
    }

    [Fact]
    public void QueryMatchingNothing_ReturnsNoListings()
    {
        Assert.Empty(Search(new SearchQuery { City = "Reston", MaxPrice = 400000m }));
    }

    private static string[] Search(SearchQuery query)
    {
        var criteria = new SearchCriteria(query);

        return Listings
            .Where(listing => AllFilters.All(filter => filter.Matches(listing, criteria)))
            .Select(listing => listing.Listing.Key)
            .ToArray();
    }

    private static SearchableListing Sample(
        string source, string id, decimal price, int bedrooms, string city, string description) =>
        new(TestListings.Create(
            id: id, source: source, price: price, bedrooms: bedrooms, city: city, description: description));
}
