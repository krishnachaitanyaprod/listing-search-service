using ListingSearch.Core.Search;

namespace ListingSearch.UnitTests.Search;

public class SearchQueryValidatorTests
{
    private const int MaxPageSize = 50;

    private readonly SearchQueryValidator _validator = new(new PagingLimits(DefaultPageSize: 10, MaxPageSize: MaxPageSize));

    // Valid queries

    [Fact]
    public void QueryWithNoFilters_IsValid()
    {
        Assert.Empty(_validator.Validate(new SearchQuery()));
    }

    [Fact]
    public void QueryWithEveryFieldSet_IsValid()
    {
        var query = new SearchQuery
        {
            MinPrice = 400000m,
            MaxPrice = 500000m,
            MinBedrooms = 2,
            City = "Springfield",
            Keyword = "pets",
            TargetBudget = 450000m,
            Page = 2,
            PageSize = 10
        };

        Assert.Empty(_validator.Validate(query));
    }

    [Fact]
    public void MinPriceEqualToMaxPrice_IsValid()
    {
        Assert.Empty(_validator.Validate(new SearchQuery { MinPrice = 450000m, MaxPrice = 450000m }));
    }

    [Fact]
    public void ZeroPricesAndBedrooms_AreValid()
    {
        Assert.Empty(_validator.Validate(new SearchQuery { MinPrice = 0m, MaxPrice = 0m, MinBedrooms = 0 }));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(MaxPageSize)]
    public void PageSizeAtEitherLimit_IsValid(int pageSize)
    {
        Assert.Empty(_validator.Validate(new SearchQuery { PageSize = pageSize }));
    }

    [Fact]
    public void CityAndKeywordAtMaxLength_AreValid()
    {
        var text = new string('a', SearchQueryValidator.MaxTextLength);

        Assert.Empty(_validator.Validate(new SearchQuery { City = text, Keyword = text }));
    }

    // Invalid queries

    [Fact]
    public void MinPriceGreaterThanMaxPrice_IsReportedOnMinPrice()
    {
        var errors = _validator.Validate(new SearchQuery { MinPrice = 500001m, MaxPrice = 500000m });

        AssertOnlyErrorOn(errors, "minPrice");
    }

    [Theory]
    [InlineData("minPrice")]
    [InlineData("maxPrice")]
    [InlineData("minBedrooms")]
    public void NegativeValue_IsReportedOnItsField(string field)
    {
        var query = field switch
        {
            "minPrice" => new SearchQuery { MinPrice = -1m },
            "maxPrice" => new SearchQuery { MaxPrice = -1m },
            "minBedrooms" => new SearchQuery { MinBedrooms = -1 },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        AssertOnlyErrorOn(_validator.Validate(query), field);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TargetBudgetOfZeroOrLess_IsReported(int targetBudget)
    {
        AssertOnlyErrorOn(_validator.Validate(new SearchQuery { TargetBudget = targetBudget }), "targetBudget");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PageBelowOne_IsReported(int page)
    {
        AssertOnlyErrorOn(_validator.Validate(new SearchQuery { Page = page }), "page");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(MaxPageSize + 1)]
    public void PageSizeOutsideOneToMax_IsReported(int pageSize)
    {
        AssertOnlyErrorOn(_validator.Validate(new SearchQuery { PageSize = pageSize }), "pageSize");
    }

    [Fact]
    public void CityOverMaxLength_IsReported()
    {
        var city = new string('a', SearchQueryValidator.MaxTextLength + 1);

        AssertOnlyErrorOn(_validator.Validate(new SearchQuery { City = city }), "city");
    }

    [Fact]
    public void KeywordOverMaxLength_IsReported()
    {
        var keyword = new string('a', SearchQueryValidator.MaxTextLength + 1);

        AssertOnlyErrorOn(_validator.Validate(new SearchQuery { Keyword = keyword }), "keyword");
    }

    [Fact]
    public void EveryProblem_IsReportedInOneResult()
    {
        var query = new SearchQuery
        {
            MinPrice = 600000m,
            MaxPrice = 500000m,
            MinBedrooms = -1,
            TargetBudget = 0m,
            Page = 0,
            PageSize = 0
        };

        var errors = _validator.Validate(query);

        Assert.Equal(
            new[] { "minBedrooms", "minPrice", "page", "pageSize", "targetBudget" },
            errors.Keys.Order(StringComparer.Ordinal));
    }

    private static void AssertOnlyErrorOn(IReadOnlyDictionary<string, string[]> errors, string field)
    {
        var fieldErrors = Assert.Single(errors);
        Assert.Equal(field, fieldErrors.Key);
        Assert.NotEmpty(fieldErrors.Value);
    }
}
