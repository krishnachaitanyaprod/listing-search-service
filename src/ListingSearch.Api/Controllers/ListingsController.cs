using ListingSearch.Api.Contracts;
using ListingSearch.Core.Search;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Controllers;

// HTTP only: maps the query string in, the search outcome out. The rules live in Core.
[ApiController]
[Route("api/v1/listings")]
public sealed class ListingsController(ListingSearchService searchService) : ControllerBase
{
    // A value that isn't a number never gets here: [ApiController] returns the same 400 shape for it.
    [HttpGet("search")]
    [ProducesResponseType<ListingSearchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public ActionResult<ListingSearchResponse> Search([FromQuery] ListingSearchRequest request)
    {
        var outcome = searchService.Search(request.ToQuery());
        if (outcome.Succeeded)
            return ListingSearchResponse.From(outcome.Result!);

        foreach (var (field, messages) in outcome.Errors)
            foreach (var message in messages)
                ModelState.AddModelError(field, message);

        return ValidationProblem(ModelState);
    }
}
