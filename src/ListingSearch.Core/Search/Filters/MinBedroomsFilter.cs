namespace ListingSearch.Core.Search.Filters;

// Inclusive: minBedrooms 3 keeps listings with 3 bedrooms.
public sealed class MinBedroomsFilter : IListingFilter
{
    public bool Matches(SearchableListing listing, SearchCriteria criteria) =>
        criteria.MinBedrooms is null || listing.Listing.Bedrooms >= criteria.MinBedrooms;
}
