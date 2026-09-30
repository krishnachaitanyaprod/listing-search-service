using ListingSearch.Core.Search.Scoring;
using Microsoft.Extensions.Time.Testing;

namespace ListingSearch.UnitTests.TestData;

internal static class TestScoring
{
    // The defaults in docs/decisions.md.
    public static readonly ScoringOptions Defaults = new(
        BudgetWeight: 0.7, RecencyWeight: 0.3, BudgetTolerance: 0.25, OverBudgetPenalty: 2, RecencyHalfLifeDays: 30);

    // The date the worked example in docs/decisions.md is calculated on.
    public static readonly DateOnly Today = new(2026, 9, 30);

    public static FakeTimeProvider ClockAt(DateOnly date) =>
        new(new DateTimeOffset(date.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero));

    public static ListingScorer Scorer(ScoringOptions? options = null, TimeProvider? clock = null)
    {
        options ??= Defaults;
        return new ListingScorer([new BudgetFitFactor(options), new RecencyFactor(options)], clock ?? ClockAt(Today));
    }
}
