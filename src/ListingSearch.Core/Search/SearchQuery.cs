namespace ListingSearch.Core.Search;

public sealed record SearchQuery
{
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinBedrooms { get; init; }
    public string? City { get; init; }
    public string? Keyword { get; init; }
    public decimal? TargetBudget { get; init; }
    public int Page { get; init; } = 1;

    // Null means the configured default page size.
    public int? PageSize { get; init; }
}
