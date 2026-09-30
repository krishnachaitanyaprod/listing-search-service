using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Scoring;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Scoring;

// Default half-life: 30 days.
public class RecencyFactorTests
{
    private readonly RecencyFactor _factor = new(TestScoring.Defaults);

    [Fact]
    public void ListedToday_IsOne()
    {
        Assert.Equal(1.0, Recency(listedDate: TestScoring.Today));
    }

    [Fact]
    public void ThirtyDaysOld_IsHalf()
    {
        Assert.Equal(0.5, Recency(listedDate: TestScoring.Today.AddDays(-30)));
    }

    [Fact]
    public void FutureDate_CountsAsZeroDaysOld()
    {
        Assert.Equal(1.0, Recency(listedDate: TestScoring.Today.AddDays(5)));
    }

    private double? Recency(DateOnly listedDate) =>
        _factor.Evaluate(
            TestListings.Create(listedDate: listedDate),
            new SearchCriteria(new SearchQuery()),
            TestScoring.Today);
}
