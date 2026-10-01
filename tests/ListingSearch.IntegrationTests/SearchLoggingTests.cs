using ListingSearch.Api.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ListingSearch.IntegrationTests;

public class SearchLoggingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string Category = typeof(ListingsController).FullName!;

    [Fact]
    public async Task CompletedSearch_WritesOneLogLine_WithTheFiltersPageTotalAndElapsedTime()
    {
        var logs = await SearchAsync("?city=Springfield&keyword=pet&targetBudget=450000&pageSize=2");

        var entry = Assert.Single(logs.For(Category));
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal("Springfield", entry.Properties["City"]);
        Assert.Equal("pet", entry.Properties["Keyword"]);
        Assert.Equal(450000m, entry.Properties["TargetBudget"]);
        Assert.Null(entry.Properties["MinPrice"]);
        Assert.Null(entry.Properties["MaxPrice"]);
        Assert.Null(entry.Properties["MinBedrooms"]);
        Assert.Equal(1, entry.Properties["Page"]);
        Assert.Equal(2, entry.Properties["PageSize"]);
        Assert.Equal(2, entry.Properties["TotalCount"]);
        Assert.InRange(Assert.IsType<double>(entry.Properties["ElapsedMs"]), 0, 10_000);
    }

    [Fact]
    public async Task RejectedSearch_WritesOneLogLine_WithTheInvalidFields()
    {
        var logs = await SearchAsync("?city=Springfeld&pageSize=0");

        var entry = Assert.Single(logs.For(Category));
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal("Springfeld", entry.Properties["City"]);
        Assert.Equal("city, pageSize", entry.Properties["InvalidFields"]);
        Assert.InRange(Assert.IsType<double>(entry.Properties["ElapsedMs"]), 0, 10_000);
    }

    private async Task<LogCapture> SearchAsync(string query)
    {
        var logs = new LogCapture();
        using var app = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<ILoggerProvider>(logs)));

        await app.CreateClient().GetAsync("/api/v1/listings/search" + query);
        return logs;
    }
}
