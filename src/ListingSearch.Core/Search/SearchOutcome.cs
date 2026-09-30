namespace ListingSearch.Core.Search;

// Either a page of ranked results or field errors keyed by query parameter name. Bad input never throws.
public sealed class SearchOutcome
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    private SearchOutcome(PagedResult<ScoredListing>? result, IReadOnlyDictionary<string, string[]> errors)
    {
        Result = result;
        Errors = errors;
    }

    public PagedResult<ScoredListing>? Result { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
    public bool Succeeded => Result is not null;

    public static SearchOutcome Success(PagedResult<ScoredListing> result) => new(result, NoErrors);

    public static SearchOutcome Invalid(IReadOnlyDictionary<string, string[]> errors) => new(null, errors);

    public static SearchOutcome Invalid(string field, string message) =>
        Invalid(new Dictionary<string, string[]> { [field] = [message] });
}
