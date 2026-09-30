using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Scoring;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Scoring;

// Defaults: tolerance 25%, over-budget penalty 2. Target budget 450,000 throughout.
public class BudgetFitFactorTests
{
    private readonly BudgetFitFactor _factor = new(TestScoring.Defaults);

    [Fact]
    public void ExactlyOnBudget_IsOne()
    {
        Assert.Equal(1.0, Fit(price: 450000m));
    }

    [Fact]
    public void TwentyFivePercentUnder_IsZero()
    {
        Assert.Equal(0.0, Fit(price: 337500m));
    }

    [Fact]
    public void TwelveAndAHalfPercentOver_IsZero()
    {
        Assert.Equal(0.0, Fit(price: 506250m));
    }

    [Theory]
    [InlineData(100000)]
    [InlineData(5000000)]
    public void FarBeyondTolerance_StaysZero(int price)
    {
        Assert.Equal(0.0, Fit(price: price));
    }

    [Fact]
    public void NoTargetBudget_IsNull()
    {
        Assert.Null(_factor.Evaluate(TestListings.Create(), new SearchCriteria(new SearchQuery()), TestScoring.Today));
    }

    private double? Fit(decimal price) =>
        _factor.Evaluate(
            TestListings.Create(price: price),
            new SearchCriteria(new SearchQuery { TargetBudget = 450000m }),
            TestScoring.Today);
}
