using ListingSearch.Core.Listings;
using ListingSearch.Core.Search;

namespace ListingSearch.UnitTests.TestData;

// Builds listings for tests. Only the fields a test cares about need to be passed.
internal static class TestListings
{
    public static Listing Create(
        string id = "T1",
        string source = "MLS_A",
        decimal price = 450000m,
        int bedrooms = 3,
        string city = "Springfield",
        string description = "",
        DateOnly? listedDate = null,
        ListingStatus status = ListingStatus.Active) =>
        new()
        {
            Id = id,
            Source = source,
            Address = "1 Test St",
            City = city,
            State = "VA",
            Zip = "22150",
            Price = price,
            Bedrooms = bedrooms,
            Bathrooms = 2.0,
            Sqft = 1500,
            Latitude = 38.8,
            Longitude = -77.2,
            ListedDate = listedDate ?? new DateOnly(2026, 9, 1),
            Status = status,
            Description = description
        };

    public static SearchableListing Searchable(
        decimal price = 450000m,
        int bedrooms = 3,
        string city = "Springfield",
        string description = "") =>
        new(Create(price: price, bedrooms: bedrooms, city: city, description: description));
}
