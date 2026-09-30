using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search;

// Factors holds each score factor's value by name ("budgetFit", "recency").
// A value is null when that factor doesn't apply, e.g. budgetFit when no targetBudget was given.
public sealed record ScoredListing(Listing Listing, double Score, IReadOnlyDictionary<string, double?> Factors);
