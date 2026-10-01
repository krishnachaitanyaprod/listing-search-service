namespace ListingSearch.Infrastructure.Listings;

// The "Listings" section of appsettings. Checked at startup.
public sealed class ListingsSettings
{
    public const string Section = "Listings";

    // Relative to the app's own folder, or absolute.
    public string FilePath { get; init; } = "";
}
