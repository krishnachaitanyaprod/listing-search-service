using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search.Scoring;

// Score = 100 × Σ(weight × value) / Σ(weight), over the factors that apply (docs/decisions.md).
// Knows no factor by name, so a new factor is one IScoreFactor class plus its weight setting.
public sealed class ListingScorer(IEnumerable<IScoreFactor> factors, TimeProvider timeProvider)
{
    private readonly IScoreFactor[] _factors = factors.ToArray();

    public IReadOnlyList<ScoredListing> Score(IEnumerable<Listing> listings, SearchCriteria criteria)
    {
        // Read once, so every listing in this search is scored against the same date.
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        return listings.Select(listing => ScoreOne(listing, criteria, today)).ToList();
    }

    private ScoredListing ScoreOne(Listing listing, SearchCriteria criteria, DateOnly today)
    {
        var values = new Dictionary<string, double?>(_factors.Length);
        double weightedSum = 0;
        double totalWeight = 0;

        foreach (var factor in _factors)
        {
            var value = factor.Evaluate(listing, criteria, today);
            values[factor.Name] = value;

            if (value is null)
                continue;

            weightedSum += factor.Weight * value.Value;
            totalWeight += factor.Weight;
        }

        // If the weights that apply add up to 0 there is nothing to rank on: every score is 0
        // and the tie-break sets the order.
        var score = totalWeight > 0 ? 100 * weightedSum / totalWeight : 0;

        return new ScoredListing(listing, score, values);
    }
}
