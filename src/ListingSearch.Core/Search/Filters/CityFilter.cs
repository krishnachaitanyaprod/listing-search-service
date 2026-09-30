namespace ListingSearch.Core.Search.Filters;

// Exact match; both sides are already normalised. An empty city means no filter.
public sealed class CityFilter : IListingFilter
{
    public bool Matches(SearchableListing listing, SearchCriteria criteria) =>
        criteria.NormalizedCity.Length == 0 || listing.NormalizedCity == criteria.NormalizedCity;
}
