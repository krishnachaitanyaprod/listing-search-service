namespace ListingSearch.Core.Search.Filters;

// Both ends are inclusive.
public sealed class PriceRangeFilter : IListingFilter
{
    public bool Matches(SearchableListing listing, SearchCriteria criteria)
    {
        var price = listing.Listing.Price;

        return (criteria.MinPrice is null || price >= criteria.MinPrice)
            && (criteria.MaxPrice is null || price <= criteria.MaxPrice);
    }
}
