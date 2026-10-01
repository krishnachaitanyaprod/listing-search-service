using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Scoring;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Search.Scoring;

public class TopRankedTests
{
    // 200 listings with only three scores and two prices, so most of them tie on both and the key decides.
    private static readonly ScoredListing[] Scored = Enumerable.Range(1, 200)
        .Select(i => new ScoredListing(
            TestListings.Create(id: $"T{i:D3}", price: i % 2 == 0 ? 400_000m : 450_000m),
            Score: new[] { 50.0, 60.0, 70.0 }[i % 3],
            Factors: new Dictionary<string, double?>()))
        .OrderBy(_ => Random.Shared.Next())
        .ToArray();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(67)]
    [InlineData(199)]
    [InlineData(200)]
    [InlineData(500)]
    public void KeepsExactlyTheFirstCountOfAFullSort(int count)
    {
        var fullSort = Scored.Order(RankingComparer.Instance).Take(count).Select(s => s.Listing.Key);

        var top = TopRanked.Select(Scored, count);

        Assert.Equal(fullSort, top.Select(s => s.Listing.Key));
    }
}
