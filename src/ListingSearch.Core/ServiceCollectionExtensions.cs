using ListingSearch.Core.Search;
using ListingSearch.Core.Search.Filters;
using ListingSearch.Core.Search.Scoring;
using Microsoft.Extensions.DependencyInjection;

namespace ListingSearch.Core;

public static class ServiceCollectionExtensions
{
    // The search and everything it runs. The host registers ScoringOptions and PagingLimits from its settings,
    // and Infrastructure registers the ListingCatalog. A new filter or score factor is one more line here.
    public static IServiceCollection AddListingSearchCore(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IListingFilter, PriceRangeFilter>();
        services.AddSingleton<IListingFilter, MinBedroomsFilter>();
        services.AddSingleton<IListingFilter, CityFilter>();
        services.AddSingleton<IListingFilter, KeywordFilter>();

        services.AddSingleton<IScoreFactor, BudgetFitFactor>();
        services.AddSingleton<IScoreFactor, RecencyFactor>();

        services.AddSingleton<SearchQueryValidator>();
        services.AddSingleton<ListingScorer>();
        services.AddSingleton<ListingSearchService>();

        return services;
    }
}
