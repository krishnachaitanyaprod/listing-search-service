namespace ListingSearch.Core.Search.Filters;

// A filter whose value isn't set in the criteria matches every listing.
public interface IListingFilter
{
    bool Matches(SearchableListing listing, SearchCriteria criteria);
}
