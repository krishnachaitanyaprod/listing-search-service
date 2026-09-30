using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search.Scoring;

// 1 on budget, falling to 0 at BudgetTolerance under the target, or at BudgetTolerance / OverBudgetPenalty
// over it (25% under or 12.5% over with the defaults). Doesn't apply without a targetBudget.
public sealed class BudgetFitFactor(ScoringOptions options) : IScoreFactor
{
    public const string FactorName = "budgetFit";

    public string Name => FactorName;
    public double Weight => options.BudgetWeight;

    public double? Evaluate(Listing listing, SearchCriteria criteria, DateOnly today)
    {
        if (criteria.TargetBudget is not { } target)
            return null;

        // Money stays decimal; only the ratio to the target becomes a double.
        var difference = listing.Price - target;
        var ratio = (double)(Math.Abs(difference) / target);
        var distance = difference > 0 ? options.OverBudgetPenalty * ratio : ratio;

        return Math.Max(0, 1 - distance / options.BudgetTolerance);
    }
}
