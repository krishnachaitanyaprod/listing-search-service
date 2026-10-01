using System.Diagnostics;
using ListingSearch.Api.Contracts;
using ListingSearch.Core.Search;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Controllers;

// HTTP only: maps the query string in, the search outcome out. The rules live in Core.
[ApiController]
[Route("api/v1/listings")]
public sealed partial class ListingsController(ListingSearchService searchService, ILogger<ListingsController> logger)
    : ControllerBase
{
    // A value that isn't a number never gets here: [ApiController] returns the same 400 shape for it.
    [HttpGet("search")]
    [ProducesResponseType<ListingSearchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public ActionResult<ListingSearchResponse> Search([FromQuery] ListingSearchRequest request)
    {
        var query = request.ToQuery();
        var started = Stopwatch.GetTimestamp();
        var outcome = searchService.Search(query);
        var elapsedMs = Math.Round(Stopwatch.GetElapsedTime(started).TotalMilliseconds, 2);

        if (outcome.Succeeded)
        {
            var result = outcome.Result!;
            LogSearchCompleted(query.City, query.Keyword, query.MinPrice, query.MaxPrice, query.MinBedrooms,
                query.TargetBudget, result.Page, result.PageSize, result.TotalCount, elapsedMs);
            return ListingSearchResponse.From(result);
        }

        LogSearchRejected(query.City, query.Keyword, query.MinPrice, query.MaxPrice, query.MinBedrooms,
            query.TargetBudget, query.Page, query.PageSize,
            string.Join(", ", outcome.Errors.Keys.Order(StringComparer.Ordinal)), elapsedMs);

        foreach (var (field, messages) in outcome.Errors)
            foreach (var message in messages)
                ModelState.AddModelError(field, message);

        return ValidationProblem(ModelState);
    }

    // One structured line per search: every filter is its own property, so logs can be queried by it.
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Search city={City} keyword={Keyword} minPrice={MinPrice} maxPrice={MaxPrice} minBedrooms={MinBedrooms} " +
                  "targetBudget={TargetBudget} page={Page} pageSize={PageSize} found {TotalCount} in {ElapsedMs} ms")]
    private partial void LogSearchCompleted(string? city, string? keyword, decimal? minPrice, decimal? maxPrice,
        int? minBedrooms, decimal? targetBudget, int page, int pageSize, int totalCount, double elapsedMs);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Search city={City} keyword={Keyword} minPrice={MinPrice} maxPrice={MaxPrice} minBedrooms={MinBedrooms} " +
                  "targetBudget={TargetBudget} page={Page} pageSize={PageSize} rejected on {InvalidFields} in {ElapsedMs} ms")]
    private partial void LogSearchRejected(string? city, string? keyword, decimal? minPrice, decimal? maxPrice,
        int? minBedrooms, decimal? targetBudget, int page, int? pageSize, string invalidFields, double elapsedMs);
}
