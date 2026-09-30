using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search.Scoring;

// Halves every RecencyHalfLifeDays: 0.5 ^ (ageDays / half-life). Always applies.
public sealed class RecencyFactor(ScoringOptions options) : IScoreFactor
{
    public const string FactorName = "recency";

    public string Name => FactorName;
    public double Weight => options.RecencyWeight;

    public double? Evaluate(Listing listing, SearchCriteria criteria, DateOnly today)
    {
        // Whole days; a listing dated in the future counts as 0 days old.
        var ageDays = Math.Max(0, today.DayNumber - listing.ListedDate.DayNumber);

        return Math.Pow(0.5, ageDays / options.RecencyHalfLifeDays);
    }
}
