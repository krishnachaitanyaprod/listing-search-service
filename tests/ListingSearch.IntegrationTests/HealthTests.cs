using System.Net;
using ListingSearch.Api.Health;
using ListingSearch.Core.Listings;
using ListingSearch.Infrastructure.Listings;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ListingSearch.IntegrationTests;

public class HealthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoint_IsHealthy_WhenTheAppIsRunning(string path)
    {
        var response = await factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReadyCheck_IsUnhealthyUntilTheCatalogIsLoaded()
    {
        var loader = new ListingCatalogLoader([new EmptySource()]);
        var check = new ListingCatalogHealthCheck(loader);

        var before = await check.CheckHealthAsync(new HealthCheckContext());
        await loader.StartAsync(CancellationToken.None);
        var after = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, before.Status);
        Assert.Equal(HealthStatus.Healthy, after.Status);
    }

    private sealed class EmptySource : IListingSource
    {
        public Task<IReadOnlyList<Listing>> GetListingsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Listing>>([]);
    }
}
