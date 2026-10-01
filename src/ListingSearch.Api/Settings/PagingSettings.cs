using ListingSearch.Core.Search;

namespace ListingSearch.Api.Settings;

// The "Paging" section of appsettings, checked at startup against the rules in docs/decisions.md.
public sealed class PagingSettings
{
    public const string Section = "Paging";

    public int DefaultPageSize { get; init; } = 10;
    public int MaxPageSize { get; init; } = 50;

    public PagingLimits ToLimits() => new(DefaultPageSize, MaxPageSize);
}
