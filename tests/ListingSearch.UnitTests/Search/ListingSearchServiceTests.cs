using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search;

// Runs over the 12 listings in data/sample_listings.json with "today" pinned to 2026-09-30.
public class ListingSearchServiceTests
{
    private static readonly PagingLimits Limits = new(DefaultPageSize: 10, MaxPageSize: 50);

    private readonly ListingSearchService _service = new(
        SampleCatalog.Instance,
        new SearchQueryValidator(Limits),
        [new PriceRangeFilter(), new MinBedroomsFilter(), new CityFilter(), new KeywordFilter()],
        TestScoring.Scorer(),
        Limits);

    [Fact]
    public void TargetBudget450000_RanksTheSampleAsInDecisionsDoc()
    {
        var result = Success(new SearchQuery { TargetBudget = 450000m, PageSize = 50 });

        // The full ranking was worked out separately from the code. A1, A5, A6 and A3 keep
        // the order and scores in docs/decisions.md; the other listings fall around them.
        Assert.Equal(
            new[]
            {
                "MLS_A:A1", "MLS_B:B7", "MLS_B:B11", "MLS_A:A5", "MLS_A:A6", "MLS_A:A3",
                "MLS_B:B9", "MLS_A:A4", "MLS_A:A2", "MLS_B:B8", "MLS_B:B10", "MLS_A:A7"
            },
            Keys(result));
        Assert.Equal(84.3, ScoreOf(result, "MLS_A:A1"), precision: 1);
        Assert.Equal(61.6, ScoreOf(result, "MLS_A:A5"), precision: 1);
        Assert.Equal(57.5, ScoreOf(result, "MLS_A:A6"), precision: 1);
        Assert.Equal(53.6, ScoreOf(result, "MLS_A:A3"), precision: 1);
    }

    [Fact]
    public void TwelveListings_WithPageSizeFive_GivePagesOfFiveFiveAndTwo()
    {
        var pages = Enumerable.Range(1, 3)
            .Select(page => Success(new SearchQuery { Page = page, PageSize = 5 }))
            .ToList();

        Assert.Equal(new[] { 5, 5, 2 }, pages.Select(p => p.Items.Count));
        Assert.All(pages, p =>
        {
            Assert.Equal(12, p.TotalCount);
            Assert.Equal(3, p.TotalPages);
            Assert.Equal(5, p.PageSize);
        });

        // Together the three pages hold every listing once, in rank order.
        var onePage = Success(new SearchQuery { PageSize = 50 });
        Assert.Equal(Keys(onePage), pages.SelectMany(Keys));
    }

    [Fact]
    public void PagePastTheLastPage_IsAnErrorOnPage()
    {
        AssertOnlyErrorOn(_service.Search(new SearchQuery { Page = 4, PageSize = 5 }), "page");
    }

    [Fact]
    public void PageSizeFifty_GivesOnePage()
    {
        var result = Success(new SearchQuery { PageSize = 50 });

        Assert.Equal(12, result.Items.Count);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void NoPageSize_UsesTheDefaultOfTen()
    {
        var result = Success(new SearchQuery());

        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void EveryRegisteredFilter_IsApplied()
    {
        var result = Success(new SearchQuery { City = "springfield", MinBedrooms = 2, MaxPrice = 500000m, Keyword = "pet" });

        // No targetBudget, so recency alone ranks them: A1 was listed two days after B7.
        Assert.Equal(new[] { "MLS_A:A1", "MLS_B:B7" }, Keys(result));
    }

    [Fact]
    public void KnownCityThatTheOtherFiltersExclude_ReturnsAnEmptyPage_NotAnError()
    {
        var result = Success(new SearchQuery { City = "Reston", MaxPrice = 400000m });

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public void UnknownCity_IsAnErrorOnCity()
    {
        AssertOnlyErrorOn(_service.Search(new SearchQuery { City = "Springfeld" }), "city");
    }

    [Fact]
    public void KnownCity_IsFoundWhateverItsCaseAndSpacing()
    {
        Assert.Equal(4, Success(new SearchQuery { City = "  SPRINGFIELD " }).TotalCount);
    }

    [Fact]
    public void InvalidInput_ReturnsErrors_NotResults()
    {
        var outcome = _service.Search(new SearchQuery { MinPrice = 500000m, MaxPrice = 400000m, PageSize = 0 });

        Assert.False(outcome.Succeeded);
        Assert.Null(outcome.Result);
        Assert.Equal(new[] { "minPrice", "pageSize" }, outcome.Errors.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void UnknownCity_AndAnInvalidPageSize_AreReportedTogether()
    {
        var outcome = _service.Search(new SearchQuery { City = "Springfeld", PageSize = 0 });

        Assert.False(outcome.Succeeded);
        Assert.Equal(new[] { "city", "pageSize" }, outcome.Errors.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void CityOverTheLengthLimit_IsNotAlsoLookedUp()
    {
        var outcome = _service.Search(new SearchQuery { City = new string('x', SearchQueryValidator.MaxTextLength + 1) });

        AssertOnlyErrorOn(outcome, "city");
        Assert.Contains("100 characters", Assert.Single(outcome.Errors["city"]));
    }

    private PagedResult<ScoredListing> Success(SearchQuery query)
    {
        var outcome = _service.Search(query);

        Assert.True(outcome.Succeeded, "Expected results but got errors: " +
            string.Join("; ", outcome.Errors.Select(e => $"{e.Key}: {string.Join(" ", e.Value)}")));
        return outcome.Result!;
    }

    private static string[] Keys(PagedResult<ScoredListing> result) =>
        result.Items.Select(s => s.Listing.Key).ToArray();

    private static double ScoreOf(PagedResult<ScoredListing> result, string key) =>
        Assert.Single(result.Items, s => s.Listing.Key == key).Score;

    private static void AssertOnlyErrorOn(SearchOutcome outcome, string field)
    {
        Assert.False(outcome.Succeeded);
        Assert.Null(outcome.Result);
        var error = Assert.Single(outcome.Errors);
        Assert.Equal(field, error.Key);
        Assert.NotEmpty(error.Value);
    }
}
