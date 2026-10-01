using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.Core.Search.Scoring;
using ListingSearch.Infrastructure.Listings;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search;

// Over 3,000 generated listings, every page the search returns is that page of a full sort, with the exact total.
public class ListingSearchServiceAtScaleTests
{
    private static readonly PagingLimits Limits = new(DefaultPageSize: 10, MaxPageSize: 50);

    private static readonly ListingCatalog Catalog = new(
        new GeneratedListingSource(3000, seed: 11, new DateOnly(2026, 9, 30)).GetListingsAsync().GetAwaiter().GetResult());

    private static readonly IListingFilter[] Filters =
        [new PriceRangeFilter(), new MinBedroomsFilter(), new CityFilter(), new KeywordFilter()];

    private readonly ListingScorer _scorer = TestScoring.Scorer();

    public static TheoryData<SearchQuery> Queries =>
    [
        new SearchQuery(),
        new SearchQuery { TargetBudget = 450_000m },
        new SearchQuery { City = "Reston", Keyword = "pool", TargetBudget = 600_000m },
        new SearchQuery { MinBedrooms = 3, MaxPrice = 600_000m, TargetBudget = 500_000m }
    ];

    [Theory]
    [MemberData(nameof(Queries))]
    public void EveryPage_MatchesAFullSort(SearchQuery filters)
    {
        var service = new ListingSearchService(Catalog, new SearchQueryValidator(Limits), Filters, _scorer, Limits);
        var fullSort = FullSort(filters);
        Assert.NotEmpty(fullSort);

        foreach (var pageSize in new[] { 1, 7, 50 })
        {
            var lastPage = Pager.TotalPages(fullSort.Count, pageSize);
            foreach (var page in new[] { 1, 2, lastPage }.Where(p => p <= lastPage).Distinct())
            {
                var result = service.Search(filters with { Page = page, PageSize = pageSize }).Result!;
                var expected = fullSort.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                Assert.Equal(fullSort.Count, result.TotalCount);
                Assert.Equal(expected.Select(s => s.Listing.Key), result.Items.Select(s => s.Listing.Key));
                Assert.Equal(expected.Select(s => s.Score), result.Items.Select(s => s.Score));
            }
        }
    }

    // The reference: score every match and sort them all.
    private List<ScoredListing> FullSort(SearchQuery filters)
    {
        var criteria = new SearchCriteria(filters);
        var matches = Catalog.Listings.Where(l => Filters.All(f => f.Matches(l, criteria))).Select(l => l.Listing);
        return _scorer.Score(matches, criteria).Order(RankingComparer.Instance).ToList();
    }
}
