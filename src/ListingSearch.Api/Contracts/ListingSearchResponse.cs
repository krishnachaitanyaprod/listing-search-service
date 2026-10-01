using ListingSearch.Core.Search;

namespace ListingSearch.Api.Contracts;

// The response contract, kept apart from the Core types so Core can change without changing the API.
public sealed record ListingSearchResponse(
    IReadOnlyList<ListingSearchItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static ListingSearchResponse From(PagedResult<ScoredListing> result) =>
        new(
            result.Items.Select(ListingSearchItem.From).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);
}

public sealed record ListingSearchItem(
    string Key,
    string Source,
    string Id,
    string Address,
    string City,
    string State,
    string Zip,
    decimal Price,
    int Bedrooms,
    double Bathrooms,
    int Sqft,
    DateOnly ListedDate,
    string Status,
    string Description,
    double Score,
    IReadOnlyDictionary<string, double?> Factors)
{
    // Rounded for display only; ranking used the unrounded score.
    public static ListingSearchItem From(ScoredListing scored)
    {
        var listing = scored.Listing;
        return new(
            listing.Key,
            listing.Source,
            listing.Id,
            listing.Address,
            listing.City,
            listing.State,
            listing.Zip,
            listing.Price,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Sqft,
            listing.ListedDate,
            listing.Status.ToString().ToLowerInvariant(),
            listing.Description,
            Math.Round(scored.Score, 1, MidpointRounding.AwayFromZero),
            // Null stays null: that factor didn't apply to this search.
            scored.Factors.ToDictionary(
                factor => factor.Key,
                factor => factor.Value is { } value ? Math.Round(value, 2, MidpointRounding.AwayFromZero) : (double?)null));
    }
}
