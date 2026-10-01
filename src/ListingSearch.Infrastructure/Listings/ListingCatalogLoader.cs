using ListingSearch.Core.Listings;
using ListingSearch.Core.Search;
using Microsoft.Extensions.Hosting;

namespace ListingSearch.Infrastructure.Listings;

// Loads the listings from every source into one catalog, once, when the app starts. Hosted services start
// before the server takes requests, so a search never sees an empty catalog, and a missing or invalid file
// stops startup with the source's message.
public sealed class ListingCatalogLoader(IEnumerable<IListingSource> sources) : IHostedService
{
    // Set once at startup and read by request threads.
    private volatile ListingCatalog? _catalog;

    public bool IsLoaded => _catalog is not null;

    public ListingCatalog Catalog =>
        _catalog ?? throw new InvalidOperationException("The listings have not been loaded yet.");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var listings = new List<Listing>();
        foreach (var source in sources)
            listings.AddRange(await source.GetListingsAsync(cancellationToken));

        _catalog = new ListingCatalog(listings);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
