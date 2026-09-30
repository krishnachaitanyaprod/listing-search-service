using ListingSearch.Core.Listings;
using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Scoring;
using ListingSearch.UnitTests.TestData;
using Xunit.Abstractions;

namespace ListingSearch.UnitTests.Search.Scoring;

// "Today" is pinned to 2026-09-30, the date of the worked example in docs/decisions.md.
public class ListingScorerTests(ITestOutputHelper output)
{
    [Fact]
    public void WorkedExample_MatchesDecisionsDoc()
    {
        var ranked = Rank(
            TestScoring.Scorer(),
            [SampleListings.A3, SampleListings.A6, SampleListings.A5, SampleListings.A1],
            new SearchQuery { TargetBudget = 450000m });

        foreach (var s in ranked)
        {
            output.WriteLine(
                $"{s.Listing.Key}: score {s.Score:F4}, budgetFit {s.Factors.GetValueOrDefault(BudgetFitFactor.FactorName):F4}, " +
                $"recency {s.Factors.GetValueOrDefault(RecencyFactor.FactorName):F4}");
        }

        Assert.Equal(new[] { "MLS_A:A1", "MLS_A:A5", "MLS_A:A6", "MLS_A:A3" }, ranked.Select(s => s.Listing.Key));
        AssertScored(ranked[0], score: 84.3, budgetFit: 1.00, recency: 0.48);
        AssertScored(ranked[1], score: 61.6, budgetFit: 0.64, recency: 0.55);
        AssertScored(ranked[2], score: 57.5, budgetFit: 0.69, recency: 0.31);
        AssertScored(ranked[3], score: 53.6, budgetFit: 0.55, recency: 0.51);
    }

    [Fact]
    public void NoTargetBudget_BudgetFitIsNull_AndScoreIsHundredTimesRecency()
    {
        var scored = Assert.Single(TestScoring.Scorer().Score([SampleListings.A1], new SearchCriteria(new SearchQuery())));

        Assert.Null(Assert.Contains(BudgetFitFactor.FactorName, scored.Factors));
        var recency = Assert.Contains(RecencyFactor.FactorName, scored.Factors);
        Assert.NotNull(recency);
        Assert.Equal(100 * recency.Value, scored.Score, precision: 10);
    }

    [Theory]
    [InlineData(0.0, 0.0, 450000)] // every weight is 0
    [InlineData(0.7, 0.0, null)]   // only recency applies, and its weight is 0
    public void WeightsOfApplicableFactorsAddingUpToZero_ScoreEveryListingZero_AndTieBreakSetsOrder(
        double budgetWeight, double recencyWeight, int? targetBudget)
    {
        var options = TestScoring.Defaults with { BudgetWeight = budgetWeight, RecencyWeight = recencyWeight };

        var ranked = Rank(
            TestScoring.Scorer(options),
            [SampleListings.A5, SampleListings.A1, SampleListings.A6, SampleListings.A3],
            new SearchQuery { TargetBudget = targetBudget });

        Assert.All(ranked, s => Assert.Equal(0.0, s.Score));
        // Lower price first: A3 399,000, A6 415,000, A1 450,000, A5 470,000.
        Assert.Equal(new[] { "MLS_A:A3", "MLS_A:A6", "MLS_A:A1", "MLS_A:A5" }, ranked.Select(s => s.Listing.Key));
    }

    [Fact]
    public void ChangingTheWeights_ChangesTheOrderOfA3AndA6()
    {
        var query = new SearchQuery { TargetBudget = 450000m };
        var recencyLed = TestScoring.Defaults with { BudgetWeight = 0.3, RecencyWeight = 0.7 };

        var withDefaults = Rank(TestScoring.Scorer(), [SampleListings.A3, SampleListings.A6], query);
        var withRecencyLed = Rank(TestScoring.Scorer(recencyLed), [SampleListings.A6, SampleListings.A3], query);

        // A6 is closer to budget; A3 is three weeks newer.
        Assert.Equal(new[] { "MLS_A:A6", "MLS_A:A3" }, withDefaults.Select(s => s.Listing.Key));
        Assert.Equal(new[] { "MLS_A:A3", "MLS_A:A6" }, withRecencyLed.Select(s => s.Listing.Key));
    }

    [Fact]
    public void ShuffledInput_AlwaysComesOutInTheSameOrder()
    {
        // Two listings identical except for their key, so only the final tie-break separates them.
        var twinB = TestListings.Create(id: "T1", source: "MLS_B", price: 500000m, listedDate: new DateOnly(2026, 9, 15));
        var twinA = twinB with { Source = "MLS_A" };
        Listing[] listings = [SampleListings.A1, SampleListings.A3, SampleListings.A5, SampleListings.A6, twinB, twinA];
        string[] expected = ["MLS_A:A1", "MLS_A:A5", "MLS_A:A6", "MLS_A:A3", "MLS_A:T1", "MLS_B:T1"];
        var query = new SearchQuery { TargetBudget = 450000m };
        var random = new Random(42);

        for (var i = 0; i < 20; i++)
        {
            var shuffled = listings.OrderBy(_ => random.Next()).ToArray();

            Assert.Equal(expected, Rank(TestScoring.Scorer(), shuffled, query).Select(s => s.Listing.Key));
        }
    }

    [Fact]
    public void EveryListingInOneSearch_IsScoredAgainstTheSameDate()
    {
        // Listed 30 days before 2026-09-30, so recency is 0.5 on that date.
        var listedDate = new DateOnly(2026, 8, 31);
        Listing[] listings =
        [
            TestListings.Create(id: "X1", listedDate: listedDate),
            TestListings.Create(id: "X2", listedDate: listedDate),
            TestListings.Create(id: "X3", listedDate: listedDate)
        ];
        var scorer = TestScoring.Scorer(clock: new ClockThatMovesOnADayEachRead(TestScoring.Today));

        var scored = scorer.Score(listings, new SearchCriteria(new SearchQuery()));

        Assert.All(scored, s => Assert.Equal(0.5, Assert.Contains(RecencyFactor.FactorName, s.Factors)));
    }

    private static List<ScoredListing> Rank(ListingScorer scorer, Listing[] listings, SearchQuery query) =>
        scorer.Score(listings, new SearchCriteria(query)).Order(RankingComparer.Instance).ToList();

    private static void AssertScored(ScoredListing scored, double score, double budgetFit, double recency)
    {
        Assert.Equal(score, scored.Score, precision: 1);
        Assert.Equal(budgetFit, Assert.Contains(BudgetFitFactor.FactorName, scored.Factors)!.Value, precision: 2);
        Assert.Equal(recency, Assert.Contains(RecencyFactor.FactorName, scored.Factors)!.Value, precision: 2);
    }

    // Returns a later date every time it's read, so reading it per listing would give different recency values.
    private sealed class ClockThatMovesOnADayEachRead(DateOnly start) : TimeProvider
    {
        private DateTimeOffset _next = new(start.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow()
        {
            var now = _next;
            _next = _next.AddDays(1);
            return now;
        }
    }
}
