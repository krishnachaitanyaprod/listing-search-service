using ListingSearch.Core.Search;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Contracts;

// The query string of GET /api/v1/listings/search. Every parameter is optional. The names are set here
// so a value that isn't a number (minPrice=abc) is reported under the same name the caller sent.
public sealed class ListingSearchRequest
{
    [FromQuery(Name = "minPrice")] public decimal? MinPrice { get; init; }
    [FromQuery(Name = "maxPrice")] public decimal? MaxPrice { get; init; }
    [FromQuery(Name = "minBedrooms")] public int? MinBedrooms { get; init; }
    [FromQuery(Name = "city")] public string? City { get; init; }
    [FromQuery(Name = "keyword")] public string? Keyword { get; init; }
    [FromQuery(Name = "targetBudget")] public decimal? TargetBudget { get; init; }
    [FromQuery(Name = "page")] public int? Page { get; init; }
    [FromQuery(Name = "pageSize")] public int? PageSize { get; init; }

    public SearchQuery ToQuery() => new()
    {
        MinPrice = MinPrice,
        MaxPrice = MaxPrice,
        MinBedrooms = MinBedrooms,
        City = City,
        Keyword = Keyword,
        TargetBudget = TargetBudget,
        Page = Page ?? 1,
        PageSize = PageSize
    };
}
