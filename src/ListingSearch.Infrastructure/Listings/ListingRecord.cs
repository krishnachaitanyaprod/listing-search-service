using ListingSearch.Core.Listings;

namespace ListingSearch.Infrastructure.Listings;

// One record as it appears in the file. Everything is nullable so a missing field is reported
// as a reason to skip the record instead of failing the whole load.
internal sealed class ListingRecord
{
    public string? Id { get; init; }
    public string? Source { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Zip { get; init; }
    public decimal? Price { get; init; }
    public int? Bedrooms { get; init; }
    public double? Bathrooms { get; init; }
    public int? Sqft { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public DateOnly? ListedDate { get; init; }
    public ListingStatus? Status { get; init; }
    public string? Description { get; init; }

    // Returns the listing, or null and the reason the record can't be used.
    public (Listing? Listing, string? Reason) ToListing()
    {
        var missing = MissingFields();
        if (missing.Count > 0)
            return (null, $"missing {string.Join(", ", missing)}");
        if (Price <= 0)
            return (null, "price must be greater than 0");
        if (Bedrooms < 0)
            return (null, "bedrooms must be 0 or more");
        if (!Enum.IsDefined(Status!.Value))
            return (null, "unknown status");

        // Every field was checked for presence above.
        return (new Listing
        {
            Id = Id!,
            Source = Source!,
            Address = Address!,
            City = City!,
            State = State!,
            Zip = Zip!,
            Price = Price!.Value,
            Bedrooms = Bedrooms!.Value,
            Bathrooms = Bathrooms!.Value,
            Sqft = Sqft!.Value,
            Latitude = Latitude!.Value,
            Longitude = Longitude!.Value,
            ListedDate = ListedDate!.Value,
            Status = Status.Value,
            Description = Description!
        }, null);
    }

    private List<string> MissingFields()
    {
        var missing = new List<string>();

        // Id, source and city must also be non-blank: they make up the key and the city filter.
        if (string.IsNullOrWhiteSpace(Id)) missing.Add("id");
        if (string.IsNullOrWhiteSpace(Source)) missing.Add("source");
        if (Address is null) missing.Add("address");
        if (string.IsNullOrWhiteSpace(City)) missing.Add("city");
        if (State is null) missing.Add("state");
        if (Zip is null) missing.Add("zip");
        if (Price is null) missing.Add("price");
        if (Bedrooms is null) missing.Add("bedrooms");
        if (Bathrooms is null) missing.Add("bathrooms");
        if (Sqft is null) missing.Add("sqft");
        if (Latitude is null) missing.Add("latitude");
        if (Longitude is null) missing.Add("longitude");
        if (ListedDate is null) missing.Add("listedDate");
        if (Status is null) missing.Add("status");
        if (Description is null) missing.Add("description");

        return missing;
    }
}
