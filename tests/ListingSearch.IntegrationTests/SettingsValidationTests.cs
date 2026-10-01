using Microsoft.AspNetCore.Hosting;

namespace ListingSearch.IntegrationTests;

// Each case starts the real app with one bad setting and expects startup to stop with that rule's message.
public class SettingsValidationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("Scoring:BudgetWeight", "-0.1", "Scoring:BudgetWeight must be 0 or more.")]
    [InlineData("Scoring:RecencyWeight", "-1", "Scoring:RecencyWeight must be 0 or more.")]
    [InlineData("Scoring:BudgetTolerance", "0", "Scoring:BudgetTolerance must be greater than 0.")]
    [InlineData("Scoring:OverBudgetPenalty", "0.5", "Scoring:OverBudgetPenalty must be 1 or more.")]
    [InlineData("Scoring:RecencyHalfLifeDays", "0", "Scoring:RecencyHalfLifeDays must be greater than 0.")]
    [InlineData("Paging:MaxPageSize", "0", "Paging:MaxPageSize must be greater than 0.")]
    [InlineData("Paging:DefaultPageSize", "0", "Paging:DefaultPageSize must be between 1 and Paging:MaxPageSize.")]
    [InlineData("Paging:DefaultPageSize", "51", "Paging:DefaultPageSize must be between 1 and Paging:MaxPageSize.")]
    [InlineData("Listings:FilePath", "", "Listings:FilePath must be set.")]
    [InlineData("Listings:FilePath", "  ", "Listings:FilePath must be set.")]
    [InlineData("GeneratedListings:Count", "-1", "GeneratedListings:Count must be 0 or more.")]
    public void BadSetting_StopsStartup_WithItsRule(string key, string value, string message)
    {
        Assert.Contains(message, StartupError((key, value)));
    }

    [Fact]
    public void BothWeightsZero_StopsStartup()
    {
        var error = StartupError(("Scoring:BudgetWeight", "0"), ("Scoring:RecencyWeight", "0"));

        Assert.Contains("Scoring: at least one of BudgetWeight and RecencyWeight must be greater than 0.", error);
    }

    // Listings:FilePath is read when the catalog loader is built, just before the other settings are checked,
    // so a bad path is reported on its own. Scoring and Paging mistakes come back together.
    [Fact]
    public void SeveralBadScoringAndPagingSettings_AreReportedTogether()
    {
        var error = StartupError(
            ("Scoring:BudgetTolerance", "0"), ("Scoring:OverBudgetPenalty", "0.5"), ("Paging:DefaultPageSize", "0"));

        Assert.Contains("Scoring:BudgetTolerance must be greater than 0.", error);
        Assert.Contains("Scoring:OverBudgetPenalty must be 1 or more.", error);
        Assert.Contains("Paging:DefaultPageSize must be between 1 and Paging:MaxPageSize.", error);
    }

    private string StartupError(params (string Key, string Value)[] settings)
    {
        using var app = factory.WithWebHostBuilder(builder =>
        {
            foreach (var (key, value) in settings)
                builder.UseSetting(key, value);
        });

        var error = Record.Exception(() => app.CreateClient());

        Assert.NotNull(error);
        return error.ToString();
    }
}
