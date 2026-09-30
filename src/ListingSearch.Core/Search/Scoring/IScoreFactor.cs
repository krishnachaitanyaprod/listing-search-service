using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search.Scoring;

public interface IScoreFactor
{
    // The key for this factor's value on each ScoredListing, e.g. "budgetFit".
    string Name { get; }

    // This factor's weight, taken from ScoringOptions.
    double Weight { get; }

    // A value from 0 to 1, or null when the factor doesn't apply to this search.
    double? Evaluate(Listing listing, SearchCriteria criteria, DateOnly today);
}
