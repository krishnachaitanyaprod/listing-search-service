using ListingSearch.Core.Listings;

namespace ListingSearch.Infrastructure.Listings;

// Seeded listings (source "GEN") for trying the search at scale. The same count, seed and anchor date always give
// the same listings, so a result can be reproduced. Cities, zips and price levels follow the sample file.
public sealed class GeneratedListingSource(int count, int seed, DateOnly anchorDate) : IListingSource
{
    public const string SourceName = "GEN";

    private const int MaxAgeDays = 120;

    private sealed record City(string Name, string[] Zips, double Latitude, double Longitude, double PriceLevel);

    private static readonly City[] Cities =
    [
        new("Springfield", ["22150", "22151"], 38.7893, -77.1873, 1.10),
        new("Fairfax", ["22030"], 38.8462, -77.3064, 1.00),
        new("Reston", ["20190"], 38.9586, -77.3570, 1.20),
        new("Vienna", ["22180"], 38.9012, -77.2653, 1.05),
        new("Manassas", ["20110"], 38.7509, -77.4753, 0.90),
        new("Chantilly", ["20151"], 38.8909, -77.4316, 1.00)
    ];

    private static readonly string[] Streets =
        ["Main", "Oak", "Pine", "Maple", "Cedar", "Elm", "Birch", "Willow", "Hickory", "Chestnut", "Poplar", "Laurel"];

    private static readonly string[] StreetTypes = ["St", "Ave", "Rd", "Dr", "Ln", "Ct", "Way", "Blvd"];

    private static readonly string[] Features =
    [
        "Updated kitchen", "Hardwood floors", "Fenced yard", "Close to Metro", "Walk to shops", "Quiet cul-de-sac",
        "Finished basement", "2-car garage", "Community pool", "Near schools", "Renovated bathrooms",
        "Open floor plan", "Corner lot", "Large deck"
    ];

    public Task<IReadOnlyList<Listing>> GetListingsAsync(CancellationToken cancellationToken = default)
    {
        var random = new Random(seed);
        var listings = new Listing[count];
        for (var i = 0; i < count; i++)
            listings[i] = Create(i + 1, random);

        return Task.FromResult<IReadOnlyList<Listing>>(listings);
    }

    private Listing Create(int number, Random random)
    {
        var city = Cities[random.Next(Cities.Length)];
        var bedrooms = PickBedrooms(random);

        // About 250k + 70k a bedroom, scaled by the city and ±10%, to the nearest 500.
        var price = (250_000 + 70_000 * bedrooms) * city.PriceLevel * (0.9 + 0.2 * random.NextDouble());

        return new Listing
        {
            Id = $"G{number:D6}",
            Source = SourceName,
            Address = $"{random.Next(1, 10_000)} {Pick(Streets, random)} {Pick(StreetTypes, random)}",
            City = city.Name,
            State = "VA",
            Zip = Pick(city.Zips, random),
            Price = Math.Round((decimal)price / 500) * 500,
            Bedrooms = bedrooms,
            Bathrooms = Math.Max(1, bedrooms - 1 + 0.5 * random.Next(3)),
            Sqft = (int)Math.Round((250 + 380 * bedrooms) * (0.88 + 0.24 * random.NextDouble()) / 10) * 10,
            Latitude = Math.Round(city.Latitude + (random.NextDouble() - 0.5) * 0.06, 4),
            Longitude = Math.Round(city.Longitude + (random.NextDouble() - 0.5) * 0.06, 4),
            ListedDate = anchorDate.AddDays(-random.Next(MaxAgeDays + 1)),
            Status = PickStatus(random),
            Description = Describe(random)
        };
    }

    // Mostly 2 to 4 bedrooms.
    private static int PickBedrooms(Random random) => random.Next(100) switch
    {
        < 10 => 1,
        < 35 => 2,
        < 70 => 3,
        < 92 => 4,
        _ => 5
    };

    private static ListingStatus PickStatus(Random random) => random.Next(100) switch
    {
        < 88 => ListingStatus.Active,
        < 98 => ListingStatus.Pending,
        _ => ListingStatus.Sold
    };

    // Two features and sometimes a pet policy, e.g. "Updated kitchen, close to metro. Pets allowed."
    private static string Describe(Random random)
    {
        var first = random.Next(Features.Length);
        var second = (first + 1 + random.Next(Features.Length - 1)) % Features.Length;
        var pets = random.Next(100) switch
        {
            < 30 => " Pets allowed.",
            < 45 => " No pets.",
            _ => ""
        };
        return $"{Features[first]}, {Features[second].ToLowerInvariant()}.{pets}";
    }

    private static T Pick<T>(T[] items, Random random) => items[random.Next(items.Length)];
}
