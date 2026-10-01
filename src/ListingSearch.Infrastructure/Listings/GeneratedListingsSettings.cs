namespace ListingSearch.Infrastructure.Listings;

// The "GeneratedListings" section of appsettings. Checked at startup.
public sealed class GeneratedListingsSettings
{
    public const string Section = "GeneratedListings";

    // 0 means only the listings file is searched.
    public int Count { get; init; }

    public int Seed { get; init; } = 42;

    // Generated listing dates count back from this fixed date, so the same settings give the same listings.
    public DateOnly AnchorDate { get; init; } = new(2026, 9, 30);
}
