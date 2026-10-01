using ListingSearch.Core.Listings;
using ListingSearch.Infrastructure.Listings;

namespace ListingSearch.UnitTests.Infrastructure;

public class GeneratedListingSourceTests
{
    private static readonly DateOnly AnchorDate = new(2026, 9, 30);

    // The sample file's cities and zips; generated listings stay within them.
    private static readonly Dictionary<string, string[]> SampleZips = new()
    {
        ["Springfield"] = ["22150", "22151"],
        ["Fairfax"] = ["22030"],
        ["Reston"] = ["20190"],
        ["Vienna"] = ["22180"],
        ["Manassas"] = ["20110"],
        ["Chantilly"] = ["20151"]
    };

    [Fact]
    public async Task SameSeed_GivesTheSameListings()
    {
        var first = await Generate(500, seed: 7);
        var second = await Generate(500, seed: 7);

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task DifferentSeed_GivesDifferentListings()
    {
        var first = await Generate(500, seed: 7);
        var second = await Generate(500, seed: 8);

        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    public async Task GivesTheConfiguredCount(int count)
    {
        Assert.Equal(count, (await Generate(count, seed: 1)).Count);
    }

    [Fact]
    public async Task Listings_AreRealistic_AndInTheSampleCities()
    {
        var listings = await Generate(5000, seed: 3);

        Assert.Equal(listings.Count, listings.Select(l => l.Key).Distinct().Count());
        Assert.All(listings, listing =>
        {
            Assert.Equal("GEN", listing.Source);
            Assert.Contains(listing.Zip, SampleZips[listing.City]);
            Assert.Equal("VA", listing.State);
            Assert.InRange(listing.Price, 250_000m, 900_000m);
            Assert.Equal(0, listing.Price % 500);
            Assert.InRange(listing.Bedrooms, 1, 5);
            Assert.InRange(listing.Bathrooms, 1, listing.Bedrooms + 0.5);
            Assert.Equal(0, listing.Bathrooms % 0.5);
            Assert.InRange(listing.Sqft, 500, 2600);
            Assert.InRange(listing.ListedDate, AnchorDate.AddDays(-120), AnchorDate);
            Assert.False(string.IsNullOrWhiteSpace(listing.Address));
            Assert.EndsWith(".", listing.Description);
        });

        // Mostly active, some pending, a few sold.
        var active = listings.Count(l => l.Status == ListingStatus.Active) / (double)listings.Count;
        Assert.InRange(active, 0.8, 0.95);
        Assert.Contains(listings, l => l.Status == ListingStatus.Pending);
        Assert.Contains(listings, l => l.Status == ListingStatus.Sold);

        // Every sample city is used, and prices vary with bedrooms.
        Assert.Equal(SampleZips.Keys.Order(), listings.Select(l => l.City).Distinct().Order());
        Assert.True(AveragePrice(listings, 4) > AveragePrice(listings, 2));
    }

    private static decimal AveragePrice(IEnumerable<Listing> listings, int bedrooms) =>
        listings.Where(l => l.Bedrooms == bedrooms).Average(l => l.Price);

    private static Task<IReadOnlyList<Listing>> Generate(int count, int seed) =>
        new GeneratedListingSource(count, seed, AnchorDate).GetListingsAsync();
}
