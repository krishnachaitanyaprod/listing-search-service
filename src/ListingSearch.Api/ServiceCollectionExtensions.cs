using System.Text.Json;
using ListingSearch.Api.Health;
using ListingSearch.Api.Settings;
using Microsoft.Extensions.Options;

namespace ListingSearch.Api;

public static class ServiceCollectionExtensions
{
    // Controllers, ProblemDetails, health checks, OpenAPI, and Core's settings (Core has no configuration code).
    public static IServiceCollection AddListingSearchApi(this IServiceCollection services, IConfiguration configuration)
    {
        AddSettings(services, configuration);

        services.AddControllers(options =>
            // ASP.NET names the field by its C# property (MinPrice); use the query name (minPrice) like the other errors.
            options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) =>
                $"The value '{value}' is not valid for {JsonNamingPolicy.CamelCase.ConvertName(field)}."));

        // ProblemDetails for every error response, each with a traceId.
        services.AddProblemDetails();

        services.AddHealthChecks()
            .AddCheck<ListingCatalogHealthCheck>("listing-catalog", tags: [HealthTags.Ready]);

        services.AddOpenApi();

        return services;
    }

    // The rules in docs/decisions.md, checked when the app starts. Every broken rule is reported at once.
    private static void AddSettings(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ScoringSettings>()
            .Bind(configuration.GetSection(ScoringSettings.Section))
            .Validate(s => s.BudgetWeight >= 0, "Scoring:BudgetWeight must be 0 or more.")
            .Validate(s => s.RecencyWeight >= 0, "Scoring:RecencyWeight must be 0 or more.")
            .Validate(s => s.BudgetWeight > 0 || s.RecencyWeight > 0,
                "Scoring: at least one of BudgetWeight and RecencyWeight must be greater than 0.")
            .Validate(s => s.BudgetTolerance > 0, "Scoring:BudgetTolerance must be greater than 0.")
            .Validate(s => s.OverBudgetPenalty >= 1, "Scoring:OverBudgetPenalty must be 1 or more.")
            .Validate(s => s.RecencyHalfLifeDays > 0, "Scoring:RecencyHalfLifeDays must be greater than 0.")
            .ValidateOnStart();

        services.AddOptions<PagingSettings>()
            .Bind(configuration.GetSection(PagingSettings.Section))
            .Validate(s => s.MaxPageSize > 0, "Paging:MaxPageSize must be greater than 0.")
            .Validate(s => s.DefaultPageSize >= 1 && s.DefaultPageSize <= s.MaxPageSize,
                "Paging:DefaultPageSize must be between 1 and Paging:MaxPageSize.")
            .ValidateOnStart();

        services.AddSingleton(provider => provider.GetRequiredService<IOptions<ScoringSettings>>().Value.ToOptions());
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<PagingSettings>>().Value.ToLimits());
    }
}
