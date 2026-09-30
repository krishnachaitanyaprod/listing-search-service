using ListingSearch.Core.Listings;

namespace ListingSearch.UnitTests.Listings;

public class ListingTests
{
    [Fact]
    public void Key_CombinesSourceAndId()
    {
        var listing = new Listing
        {
            Id = "A1",
            Source = "MLS_A",
            Address = "123 Main St, Apt 4B",
            City = "Springfield",
            State = "VA",
            Zip = "22150",
            Price = 450000m,
            Bedrooms = 2,
            Bathrooms = 1.5,
            Sqft = 980,
            Latitude = 38.7893,
            Longitude = -77.1873,
            ListedDate = new DateOnly(2026, 8, 29),
            Status = ListingStatus.Active,
            Description = "Bright top-floor condo near shops and transit. Pet friendly."
        };

        Assert.Equal("MLS_A:A1", listing.Key);
    }
}
