namespace ListingSearch.Core.Search.Filters;

// Every term must match the start of some description word, so "pet" matches "Pets" but not "carpet".
// No terms means no filter.
public sealed class KeywordFilter : IListingFilter
{
    public bool Matches(SearchableListing listing, SearchCriteria criteria) =>
        criteria.KeywordTerms.All(term =>
            listing.DescriptionWords.Any(word => word.StartsWith(term, StringComparison.Ordinal)));
}
