using ListingSearch.Core.Search.Scoring;

namespace ListingSearch.Api.Settings;

// The "Scoring" section of appsettings, checked at startup against the rules in docs/decisions.md.
// The defaults match that table, so a missing key means the documented default rather than 0.
public sealed class ScoringSettings
{
    public const string Section = "Scoring";

    public double BudgetWeight { get; init; } = 0.7;
    public double RecencyWeight { get; init; } = 0.3;
    public double BudgetTolerance { get; init; } = 0.25;
    public double OverBudgetPenalty { get; init; } = 2;
    public double RecencyHalfLifeDays { get; init; } = 30;

    public ScoringOptions ToOptions() =>
        new(BudgetWeight, RecencyWeight, BudgetTolerance, OverBudgetPenalty, RecencyHalfLifeDays);
}
