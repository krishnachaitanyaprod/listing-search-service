namespace ListingSearch.Core.Search;

// The query prepared once per search with the same TextNormalizer rules used on the data at load time,
// so filters compare like with like and don't redo the work for every listing.
public sealed class SearchCriteria(SearchQuery query)
{
    public decimal? MinPrice { get; } = query.MinPrice;
    public decimal? MaxPrice { get; } = query.MaxPrice;
    public int? MinBedrooms { get; } = query.MinBedrooms;
    public decimal? TargetBudget { get; } = query.TargetBudget;

    // Empty means no city filter.
    public string NormalizedCity { get; } = TextNormalizer.Normalize(query.City);

    // Empty means no keyword filter.
    public IReadOnlyList<string> KeywordTerms { get; } = TextNormalizer.SplitWords(query.Keyword);
}
