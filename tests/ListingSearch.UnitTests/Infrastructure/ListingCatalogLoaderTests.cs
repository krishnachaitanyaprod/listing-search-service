using ListingSearch.Core.Listings;
using ListingSearch.Infrastructure.Listings;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Infrastructure;

public class ListingCatalogLoaderTests
{
    [Fact]
    public async Task Catalog_HoldsTheListingsOfEverySource()
    {
        var loader = new ListingCatalogLoader([
            new FixedSource(TestListings.Create(id: "A1", source: "MLS_A")),
            new FixedSource(TestListings.Create(id: "G1", source: "GEN"), TestListings.Create(id: "G2", source: "GEN"))
        ]);

        await loader.StartAsync(CancellationToken.None);

        Assert.Equal(
            new[] { "GEN:G1", "GEN:G2", "MLS_A:A1" },
            loader.Catalog.Listings.Select(l => l.Listing.Key).Order(StringComparer.Ordinal));
    }

    private sealed class FixedSource(params Listing[] listings) : IListingSource
    {
        public Task<IReadOnlyList<Listing>> GetListingsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Listing>>(listings);
    }
}
