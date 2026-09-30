using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search;

// BudgetFit is null when no targetBudget was given, because the budget factor doesn't apply.
public sealed record ScoredListing(Listing Listing, double Score, double? BudgetFit, double Recency);
