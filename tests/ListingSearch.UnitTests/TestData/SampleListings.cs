using ListingSearch.Core.Listings;

namespace ListingSearch.UnitTests.TestData;

// Listings from data/sample_listings.json used in the worked example in docs/decisions.md.
internal static class SampleListings
{
    public static readonly Listing A1 = TestListings.Create(
        id: "A1", price: 450000m, bedrooms: 2, city: "Springfield", listedDate: new DateOnly(2026, 8, 29),
        description: "Bright top-floor condo near shops and transit. Pet friendly.");

    public static readonly Listing A3 = TestListings.Create(
        id: "A3", price: 399000m, bedrooms: 2, city: "Fairfax", listedDate: new DateOnly(2026, 9, 1),
        description: "Cozy starter home, no pets.");

    public static readonly Listing A5 = TestListings.Create(
        id: "A5", price: 470000m, bedrooms: 3, city: "Vienna", listedDate: new DateOnly(2026, 9, 4),
        description: "Quiet cul-de-sac, walkable to Metro. No pets.");

    public static readonly Listing A6 = TestListings.Create(
        id: "A6", price: 415000m, bedrooms: 3, city: "Manassas", listedDate: new DateOnly(2026, 8, 10),
        description: "Split-level home, large driveway, pets allowed.");
}
