namespace ListingSearch.Core.Listings;

public sealed record Listing
{
    public required string Id { get; init; }
    public required string Source { get; init; }
    public required string Address { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Zip { get; init; }
    public required decimal Price { get; init; }
    public required int Bedrooms { get; init; }
    public required double Bathrooms { get; init; }
    public required int Sqft { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required DateOnly ListedDate { get; init; }
    public required ListingStatus Status { get; init; }
    public required string Description { get; init; }

    // Ids are only unique within one source.
    public string Key => $"{Source}:{Id}";
}
