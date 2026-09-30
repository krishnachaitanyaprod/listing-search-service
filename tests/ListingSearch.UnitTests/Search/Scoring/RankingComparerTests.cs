using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Scoring;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Scoring;

public class RankingComparerTests
{
    [Fact]
    public void HigherScore_ComesFirst()
    {
        var ranked = Rank(Scored("A", score: 60), Scored("B", score: 70));

        Assert.Equal(new[] { "MLS_A:B", "MLS_A:A" }, ranked);
    }

    [Fact]
    public void ScoresThatRoundTheSame_AreOrderedByTheUnroundedValue()
    {
        // Both show as 84.3; the cheaper one would win if rounded scores were compared.
        var ranked = Rank(Scored("A", score: 84.31, price: 400000m), Scored("B", score: 84.34, price: 500000m));

        Assert.Equal(new[] { "MLS_A:B", "MLS_A:A" }, ranked);
    }

    [Fact]
    public void EqualScores_PutTheLowerPriceFirst()
    {
        var ranked = Rank(Scored("A", score: 50, price: 500000m), Scored("B", score: 50, price: 400000m));

        Assert.Equal(new[] { "MLS_A:B", "MLS_A:A" }, ranked);
    }

    [Fact]
    public void EqualScoreAndPrice_FallBackToKeyInOrdinalOrder()
    {
        // Ordinal puts "B1" before "a1" (uppercase sorts first); a culture-aware sort would not.
        var ranked = Rank(Scored("a1", score: 50), Scored("B1", score: 50));

        Assert.Equal(new[] { "MLS_A:B1", "MLS_A:a1" }, ranked);
    }

    private static string[] Rank(params ScoredListing[] scored) =>
        scored.Order(RankingComparer.Instance).Select(s => s.Listing.Key).ToArray();

    private static ScoredListing Scored(string id, double score, decimal price = 450000m) =>
        new(TestListings.Create(id: id, price: price), score, new Dictionary<string, double?>());
}
