using ListingSearch.Infrastructure.Listings;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ListingSearch.Api.Health;

// Backs /health/ready: the app can serve searches once the catalog is loaded.
public sealed class ListingCatalogHealthCheck(ListingCatalogLoader loader) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(loader.IsLoaded
            ? HealthCheckResult.Healthy($"{loader.Catalog.Listings.Count} listings loaded.")
            : HealthCheckResult.Unhealthy("The listings have not been loaded yet."));
}
