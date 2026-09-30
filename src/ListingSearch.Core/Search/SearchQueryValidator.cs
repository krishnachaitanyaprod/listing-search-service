namespace ListingSearch.Core.Search;

// Checks the rules that don't need the listing data. Unknown city and page-past-the-end
// are checked by the search itself. Errors are keyed by query parameter name.
public sealed class SearchQueryValidator(PagingLimits limits)
{
    public const int MaxTextLength = 100;

    public IReadOnlyDictionary<string, string[]> Validate(SearchQuery query)
    {
        var errors = new Dictionary<string, List<string>>();

        void Add(string field, string message)
        {
            if (!errors.TryGetValue(field, out var messages))
            {
                messages = [];
                errors[field] = messages;
            }
            messages.Add(message);
        }

        if (query.MinPrice < 0)
            Add("minPrice", "minPrice must be 0 or more.");
        if (query.MaxPrice < 0)
            Add("maxPrice", "maxPrice must be 0 or more.");
        if (query.MinPrice > query.MaxPrice)
            Add("minPrice", "minPrice must not be greater than maxPrice.");
        if (query.MinBedrooms < 0)
            Add("minBedrooms", "minBedrooms must be 0 or more.");
        if (query.TargetBudget <= 0)
            Add("targetBudget", "targetBudget must be greater than 0.");
        if (query.Page < 1)
            Add("page", "page must be 1 or more.");
        if (query.PageSize < 1 || query.PageSize > limits.MaxPageSize)
            Add("pageSize", $"pageSize must be between 1 and {limits.MaxPageSize}.");
        if (query.City?.Length > MaxTextLength)
            Add("city", $"city must be {MaxTextLength} characters or fewer.");
        if (query.Keyword?.Length > MaxTextLength)
            Add("keyword", $"keyword must be {MaxTextLength} characters or fewer.");

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
    }
}
