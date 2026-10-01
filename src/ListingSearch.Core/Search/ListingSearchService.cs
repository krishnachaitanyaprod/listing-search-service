using ListingSearch.Core.Search.Filters;
using ListingSearch.Core.Search.Scoring;

namespace ListingSearch.Core.Search;

// One search: validate, check the city, filter, score, rank, page. Bad input comes back as field errors,
// never as an exception.
public sealed class ListingSearchService(
    ListingCatalog catalog,
    SearchQueryValidator validator,
    IEnumerable<IListingFilter> filters,
    ListingScorer scorer,
    PagingLimits limits)
{
    private readonly IListingFilter[] _filters = filters.ToArray();

    public SearchOutcome Search(SearchQuery query)
    {
        var errors = validator.Validate(query).ToDictionary();

        // Built once; the city check below uses its normalised city, so the query is normalised only here.
        var criteria = new SearchCriteria(query);

        // Checked even when other fields are invalid, so every field error comes back at once.
        // A city that already failed validation (too long) isn't looked up.
        if (!errors.ContainsKey("city")
            && criteria.NormalizedCity.Length > 0
            && !catalog.KnownCities.Contains(criteria.NormalizedCity))
            errors["city"] = [$"No listings are in city '{query.City!.Trim()}'."];

        if (errors.Count > 0)
            return SearchOutcome.Invalid(errors);

        var matches = catalog.Listings
            .Where(listing => _filters.All(filter => filter.Matches(listing, criteria)))
            .Select(listing => listing.Listing)
            .ToList();

        var pageSize = query.PageSize ?? limits.DefaultPageSize;
        if (!Pager.PageExists(query.Page, matches.Count, pageSize))
            return SearchOutcome.Invalid(
                "page", $"page {query.Page} is past the last page ({Pager.TotalPages(matches.Count, pageSize)}).");

        // Every match is scored, because any of them could be the best. Only those up to the end of this page
        // are put in order; the total still counts every match.
        var needed = (int)Math.Min((long)query.Page * pageSize, matches.Count);
        var best = TopRanked.Select(scorer.Score(matches, criteria), needed);

        return SearchOutcome.Success(Pager.Page(best, query.Page, pageSize, matches.Count));
    }
}
