using ListingSearch.Core.Listings;

namespace ListingSearch.Core.Search;

// A listing with its searchable text prepared once, at load time, so searches don't redo the work.
public sealed class SearchableListing(Listing listing)
{
    public Listing Listing { get; } = listing;
    public string NormalizedCity { get; } = TextNormalizer.Normalize(listing.City);
    public IReadOnlyList<string> DescriptionWords { get; } = TextNormalizer.SplitWords(listing.Description);
}
