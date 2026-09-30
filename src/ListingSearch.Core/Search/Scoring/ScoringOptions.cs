namespace ListingSearch.Core.Search.Scoring;

// A plain settings object. The Api fills it from appsettings, so Core has no configuration code.
public sealed record ScoringOptions(
    double BudgetWeight,
    double RecencyWeight,
    double BudgetTolerance,
    double OverBudgetPenalty,
    double RecencyHalfLifeDays);
