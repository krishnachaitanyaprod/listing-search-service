using ListingSearch.Core.Listings;
using ListingSearch.Infrastructure.Listings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ListingSearch.Infrastructure;

public static class ServiceCollectionExtensions
{
    // The JSON listing source and the catalog it fills once at startup.
    public static IServiceCollection AddListingSearchInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IListingSource>(provider => new JsonFileListingSource(
            ResolveListingsPath(configuration["Listings:FilePath"]),
            provider.GetRequiredService<ILogger<JsonFileListingSource>>()));

        services.AddSingleton<ListingCatalogLoader>();
        services.AddHostedService(provider => provider.GetRequiredService<ListingCatalogLoader>());
        services.AddSingleton(provider => provider.GetRequiredService<ListingCatalogLoader>().Catalog);

        return services;
    }

    // A relative path is resolved against the app's own folder, so it works from any working folder,
    // in tests and when published. An absolute path is used as it is.
    private static string ResolveListingsPath(string? configuredPath) =>
        string.IsNullOrWhiteSpace(configuredPath)
            ? throw new InvalidOperationException("The Listings:FilePath setting is missing.")
            : Path.GetFullPath(configuredPath, AppContext.BaseDirectory);
}
