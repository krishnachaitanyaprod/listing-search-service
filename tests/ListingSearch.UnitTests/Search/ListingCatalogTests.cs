using ListingSearch.Core.Listings;
using ListingSearch.Core.Search;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search;

public class ListingCatalogTests
{
    [Fact]
    public void KnownCities_AreNormalisedAndDistinct()
    {
        var catalog = new ListingCatalog(
        [
            TestListings.Create(id: "1", city: "Springfield"),
            TestListings.Create(id: "2", city: "  SPRINGFIELD "),
            TestListings.Create(id: "3", city: "Falls  Church")
        ]);

        Assert.Equal(new[] { "falls church", "springfield" }, catalog.KnownCities.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Listings_HoldOneSearchableListingPerListing_InOrder()
    {
        Listing[] listings = [TestListings.Create(id: "1"), TestListings.Create(id: "2")];

        var catalog = new ListingCatalog(listings);

        Assert.Equal(listings, catalog.Listings.Select(s => s.Listing));
    }
}
