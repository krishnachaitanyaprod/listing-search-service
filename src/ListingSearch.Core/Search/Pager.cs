namespace ListingSearch.Core.Search;

// Pages are numbered from 1 (docs/decisions.md).
public static class Pager
{
    // Total results / page size, rounded up.
    public static int TotalPages(int totalCount, int pageSize) => (totalCount + pageSize - 1) / pageSize;

    // Page 1 always exists, even with no results; any other page must be within the total.
    public static bool PageExists(int page, int totalCount, int pageSize) =>
        page == 1 || page <= TotalPages(totalCount, pageSize);

    // ranked holds the items in order at least up to the end of this page; totalCount is every match.
    public static PagedResult<T> Page<T>(IReadOnlyList<T> ranked, int page, int pageSize, int totalCount) =>
        new(
            ranked.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            page,
            pageSize,
            totalCount,
            TotalPages(totalCount, pageSize));
}
