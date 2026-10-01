using System.Text.Json;
using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Scoring;

namespace ListingSearch.Api;

public static class ServiceCollectionExtensions
{
    // Controllers, ProblemDetails, OpenAPI, and Core's settings from appsettings (Core has no configuration code).
    public static IServiceCollection AddListingSearchApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => GetRequiredSection<ScoringOptions>(configuration, "Scoring"));
        services.AddSingleton(_ => GetRequiredSection<PagingLimits>(configuration, "Paging"));

        services.AddControllers(options =>
            // ASP.NET names the field by its C# property (MinPrice); use the query name (minPrice) like the other errors.
            options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) =>
                $"The value '{value}' is not valid for {JsonNamingPolicy.CamelCase.ConvertName(field)}."));

        // ProblemDetails for every error response, each with a traceId.
        services.AddProblemDetails();

        services.AddOpenApi();

        return services;
    }

    private static T GetRequiredSection<T>(IConfiguration configuration, string section) =>
        configuration.GetSection(section).Get<T>()
        ?? throw new InvalidOperationException($"The '{section}' settings are missing.");
}
