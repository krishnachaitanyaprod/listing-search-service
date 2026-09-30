using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search;

// Built once from the loaded listings, so a search never goes back to the source.
// KnownCities backs the "unknown city" check in docs/decisions.md.
public sealed class ListingCatalog
{
    public ListingCatalog(IEnumerable<Listing> listings)
    {
        Listings = listings.Select(listing => new SearchableListing(listing)).ToArray();
        KnownCities = Listings.Select(listing => listing.NormalizedCity).ToHashSet(StringComparer.Ordinal);
    }

    public IReadOnlyList<SearchableListing> Listings { get; }

    // Normalised with the same rules as SearchCriteria.NormalizedCity.
    public IReadOnlySet<string> KnownCities { get; }
}
