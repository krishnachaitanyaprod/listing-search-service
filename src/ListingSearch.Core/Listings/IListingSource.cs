namespace ListingSearch.Core.Listings;

public interface IListingSource
{
    Task<IReadOnlyList<Listing>> GetListingsAsync(CancellationToken cancellationToken = default);
}
