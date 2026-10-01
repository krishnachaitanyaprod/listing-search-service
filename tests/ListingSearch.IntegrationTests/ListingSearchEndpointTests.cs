using System.Net;
using System.Text.Json;
using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ListingSearch.IntegrationTests;

public class ListingSearchEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/listings/search";

    [Fact]
    public async Task ValidSearch_ReturnsRankedItemsWithTheirFactors()
    {
        var (status, body) = await GetAsync("?targetBudget=450000&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(1, body.GetProperty("page").GetInt32());
        Assert.Equal(5, body.GetProperty("pageSize").GetInt32());
        Assert.Equal(12, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, body.GetProperty("totalPages").GetInt32());

        var items = body.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(
            new[] { "MLS_A:A1", "MLS_B:B7", "MLS_B:B11", "MLS_A:A5", "MLS_A:A6" },
            items.Select(item => item.GetProperty("key").GetString()));

        // The whole contract for one item: these fields and no others, in camelCase.
        var a1 = items[0];
        Assert.Equal(
            new[]
            {
                "address", "bathrooms", "bedrooms", "city", "description", "factors", "id", "key",
                "listedDate", "price", "score", "source", "sqft", "state", "status", "zip"
            },
            a1.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal("MLS_A", a1.GetProperty("source").GetString());
        Assert.Equal("A1", a1.GetProperty("id").GetString());
        Assert.Equal("123 Main St, Apt 4B", a1.GetProperty("address").GetString());
        Assert.Equal("Springfield", a1.GetProperty("city").GetString());
        Assert.Equal("VA", a1.GetProperty("state").GetString());
        Assert.Equal("22150", a1.GetProperty("zip").GetString());
        Assert.Equal(450000m, a1.GetProperty("price").GetDecimal());
        Assert.Equal(2, a1.GetProperty("bedrooms").GetInt32());
        Assert.Equal(1.5, a1.GetProperty("bathrooms").GetDouble());
        Assert.Equal(980, a1.GetProperty("sqft").GetInt32());
        Assert.Equal("2026-08-29", a1.GetProperty("listedDate").GetString());
        Assert.Equal("active", a1.GetProperty("status").GetString());
        Assert.Equal(
            "Bright top-floor condo near shops and transit. Pet friendly.",
            a1.GetProperty("description").GetString());

        // Rounded on the way out: score to 1 decimal, each factor to 2.
        Assert.Equal(84.3, a1.GetProperty("score").GetDouble());
        var factors = a1.GetProperty("factors");
        Assert.Equal(1.0, factors.GetProperty("budgetFit").GetDouble());
        Assert.Equal(0.48, factors.GetProperty("recency").GetDouble());
    }

    [Fact]
    public async Task NoTargetBudget_GivesANullBudgetFit()
    {
        var (status, body) = await GetAsync("?city=Springfield");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.All(body.GetProperty("items").EnumerateArray(), item =>
        {
            var factors = item.GetProperty("factors");
            Assert.Equal(JsonValueKind.Null, factors.GetProperty("budgetFit").ValueKind);
            Assert.Equal(JsonValueKind.Number, factors.GetProperty("recency").ValueKind);
        });
    }

    [Theory]
    [InlineData("?minPrice=500000&maxPrice=400000", "minPrice")]
    [InlineData("?minPrice=abc", "minPrice")]
    [InlineData("?pageSize=0", "pageSize")]
    [InlineData("?city=Springfeld", "city")]
    [InlineData("?page=4&pageSize=5", "page")]
    public async Task BadInput_IsA400ValidationProblem_OnThatField(string query, string field)
    {
        var errors = await AssertValidationProblemAsync(query);

        Assert.Equal(field, Assert.Single(errors).Name);
    }

    [Fact]
    public async Task NonNumericValue_IsNamedInCamelCase_LikeTheOtherErrors()
    {
        var errors = await AssertValidationProblemAsync("?minPrice=abc");

        var message = Assert.Single(Assert.Single(errors).Value.EnumerateArray()).GetString();
        Assert.Equal("The value 'abc' is not valid for minPrice.", message);
    }

    [Fact]
    public async Task UnknownCity_AndAnInvalidPageSize_AreReportedTogether()
    {
        var errors = await AssertValidationProblemAsync("?city=Springfeld&pageSize=0");

        Assert.Equal(new[] { "city", "pageSize" }, errors.Select(e => e.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task KnownCityThatTheOtherFiltersExclude_IsA200WithAnEmptyList()
    {
        var (status, body) = await GetAsync("?city=Reston&maxPrice=400000");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Empty(body.GetProperty("items").EnumerateArray());
        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, body.GetProperty("totalPages").GetInt32());
        Assert.Equal(1, body.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task UnexpectedError_IsA500ProblemDetails_WithoutTheStackTrace()
    {
        using var failingApp = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<IListingFilter, FailingFilter>()));
        var response = await failingApp.CreateClient().GetAsync(Url);
        var text = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(text);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("traceId").GetString()));
        Assert.DoesNotContain(FailingFilter.Message, text);
        Assert.DoesNotContain(nameof(FailingFilter), text);
        Assert.DoesNotContain(" at ", text);
    }

    [Fact]
    public void MissingListingsFile_StopsStartup_NamingThePath()
    {
        // An absolute path is used as it is.
        var missing = Path.Combine(Path.GetTempPath(), $"no-listings-{Guid.NewGuid():N}.json");
        using var app = factory.WithWebHostBuilder(builder => builder.UseSetting("Listings:FilePath", missing));

        var error = Record.Exception(() => app.CreateClient());

        Assert.NotNull(error);
        Assert.Contains($"Listings file not found: {missing}", error.ToString());
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(string query)
    {
        var response = await factory.CreateClient().GetAsync(Url + query);
        var text = await response.Content.ReadAsStringAsync();
        var body = text.Length > 0 ? JsonDocument.Parse(text).RootElement : default;
        return (response.StatusCode, body);
    }

    // Checks the 400 shape and returns its field errors.
    private async Task<List<JsonProperty>> AssertValidationProblemAsync(string query)
    {
        var response = await factory.CreateClient().GetAsync(Url + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
        var errors = body.GetProperty("errors").EnumerateObject().ToList();
        Assert.All(errors, error => Assert.NotEmpty(error.Value.EnumerateArray()));
        return errors;
    }

    private sealed class FailingFilter : IListingFilter
    {
        public const string Message = "Simulated failure with internal detail";

        public bool Matches(SearchableListing listing, SearchCriteria criteria) =>
            throw new InvalidOperationException(Message);
    }
}
