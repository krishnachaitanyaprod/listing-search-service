using ListingSearch.Core.Search;
using ListingSearch.Infrastructure.Listings;
using Microsoft.Extensions.Logging.Abstractions;

namespace ListingSearch.UnitTests.TestData;

internal static class SampleCatalog
{
    // The 12 listings in data/sample_listings.json, loaded once through the real JSON source.
    public static ListingCatalog Instance { get; } = new(
        new JsonFileListingSource(RepoPaths.SampleListingsFile, NullLogger<JsonFileListingSource>.Instance)
            .GetListingsAsync().GetAwaiter().GetResult());
}
