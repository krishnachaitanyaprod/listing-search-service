using ListingSearch.Core.Listings;
using ListingSearch.Infrastructure.Listings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ListingSearch.Infrastructure;

public static class ServiceCollectionExtensions
{
    // The JSON listing source and the catalog it fills once at startup.
    public static IServiceCollection AddListingSearchInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ListingsSettings>()
            .Bind(configuration.GetSection(ListingsSettings.Section))
            .Validate(s => !string.IsNullOrWhiteSpace(s.FilePath), "Listings:FilePath must be set.")
            .ValidateOnStart();

        services.AddSingleton<IListingSource>(provider => new JsonFileListingSource(
            ResolveListingsPath(provider.GetRequiredService<IOptions<ListingsSettings>>().Value.FilePath),
            provider.GetRequiredService<ILogger<JsonFileListingSource>>()));

        services.AddSingleton<ListingCatalogLoader>();
        services.AddHostedService(provider => provider.GetRequiredService<ListingCatalogLoader>());
        services.AddSingleton(provider => provider.GetRequiredService<ListingCatalogLoader>().Catalog);

        return services;
    }

    // A relative path is resolved against the app's own folder, so it works from any working folder,
    // in tests and when published. An absolute path is used as it is.
    private static string ResolveListingsPath(string configuredPath) =>
        Path.GetFullPath(configuredPath, AppContext.BaseDirectory);
}
